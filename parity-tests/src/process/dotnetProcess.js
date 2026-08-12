import { spawn, execFile } from "node:child_process";
import { promisify } from "node:util";
import { waitForReady } from "./waitForReady.js";

const execFileAsync = promisify(execFile);

/**
 * Launches the REAL, unmodified ASP.NET Core API as a child process (`dotnet run`
 * against the already-built output). Configuration is overridden the same way as the
 * Node side — environment variables only, using ASP.NET Core's standard
 * double-underscore convention for nested keys (Jwt__Secret, etc.) — no appsettings
 * file or source file is edited.
 *
 * Runs against a dedicated `WoofBnB_Parity` database, dropped and recreated from the
 * same InitialCreate migration already committed in Phase 3, so this harness never
 * touches the `WoofBnB` database used for manual/dev verification.
 */
export function createDotnetProcess(config) {
  let child = null;

  async function resetDatabase() {
    await execFileAsync("sqlcmd", [
      "-S", "(localdb)\\MSSQLLocalDB",
      "-C",
      "-Q", `ALTER DATABASE [${config.dotnet.databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE IF EXISTS [${config.dotnet.databaseName}];`,
    ]).catch(() => {
      // Database didn't exist yet on the first run — fine, nothing to drop.
    });

    await execFileAsync(
      "dotnet",
      [
        "ef", "database", "update",
        "--project", config.dotnet.infrastructureProjectDir,
        "--startup-project", config.dotnet.apiProjectDir,
      ],
      {
        env: {
          ...process.env,
          ASPNETCORE_ENVIRONMENT: "Development",
          ConnectionStrings__DefaultConnection: config.dotnet.connectionString,
          Jwt__Secret: config.dotnet.jwtSecret,
        },
      },
    );
  }

  return {
    async start() {
      await resetDatabase();

      child = spawn(
        "dotnet",
        ["run", "--no-build", "--no-launch-profile", "--project", config.dotnet.apiProjectDir],
        {
          env: {
            ...process.env,
            ASPNETCORE_ENVIRONMENT: "Development",
            ASPNETCORE_URLS: `http://localhost:${config.dotnet.port}`,
            ConnectionStrings__DefaultConnection: config.dotnet.connectionString,
            Jwt__Secret: config.dotnet.jwtSecret,
            Jwt__ExpiresInMinutes: String(config.dotnet.jwtExpiresInMinutes),
            Security__BcryptWorkFactor: String(config.dotnet.bcryptWorkFactor),
            Cors__ClientUrl: config.dotnet.corsClientUrl,
          },
          stdio: ["ignore", "pipe", "pipe"],
        },
      );

      const output = [];
      child.stdout.on("data", (chunk) => output.push(chunk.toString()));
      child.stderr.on("data", (chunk) => output.push(chunk.toString()));

      child.on("exit", (code, signal) => {
        if (code !== 0 && code !== null) {
          console.error(`[dotnet] process exited early (code=${code}, signal=${signal})`);
          console.error(output.join(""));
        }
      });

      try {
        await waitForReady(config.dotnet.healthUrl, config.readiness, "ASP.NET backend");
      } catch (error) {
        console.error("[dotnet] startup output:\n" + output.join(""));
        throw error;
      }
    },

    async stop() {
      if (!child) return;
      child.kill();
      await new Promise((resolve) => child.once("exit", resolve));
      child = null;
    },
  };
}
