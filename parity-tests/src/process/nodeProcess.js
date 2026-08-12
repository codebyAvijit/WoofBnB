import { spawn } from "node:child_process";
import { waitForReady } from "./waitForReady.js";

/**
 * Launches the REAL, unmodified server/src/server.js as a child process. Only
 * environment variables are overridden (PORT, MONGODB_URI, JWT_SECRET,
 * JWT_EXPIRES_IN) — no file under server/ is read, written, or patched. dotenv's
 * default behaviour (server/src/server.js calls dotenv.config() with no options) is to
 * NOT overwrite a variable that's already present in process.env, so these overrides
 * win over server/.env for this process only, verified empirically before relying on it.
 */
export function createNodeProcess(config) {
  let child = null;

  return {
    async start(mongoUri) {
      child = spawn(
        process.execPath,
        [config.node.entryScript],
        {
          cwd: config.node.cwd,
          env: {
            ...process.env,
            NODE_ENV: "development",
            PORT: String(config.node.port),
            MONGODB_URI: mongoUri,
            CLIENT_URL: "http://localhost:5173",
            JWT_SECRET: config.node.jwtSecret,
            JWT_EXPIRES_IN: config.node.jwtExpiresIn,
            BCRYPT_SALT_ROUNDS: String(config.node.bcryptSaltRounds),
          },
          stdio: ["ignore", "pipe", "pipe"],
        },
      );

      const output = [];
      child.stdout.on("data", (chunk) => output.push(chunk.toString()));
      child.stderr.on("data", (chunk) => output.push(chunk.toString()));

      child.on("exit", (code, signal) => {
        if (code !== 0 && code !== null) {
          console.error(`[node] process exited early (code=${code}, signal=${signal})`);
          console.error(output.join(""));
        }
      });

      try {
        await waitForReady(config.node.healthUrl, config.readiness, "Node backend");
      } catch (error) {
        console.error("[node] startup output:\n" + output.join(""));
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
