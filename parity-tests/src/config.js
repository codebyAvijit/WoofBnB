import path from "node:path";
import { fileURLToPath } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(here, "..", "..");

/**
 * Every value below is either read from the repo's own real configuration files
 * (server/.env, server-dotnet-claude/src/WoofBnB.Api/appsettings.Development.json —
 * both already inspected, not guessed) or is an explicit override this harness applies
 * at process-launch time via environment variables, WITHOUT editing either app's
 * source or config files. Nothing here is a blind assumption:
 *
 * - Node's real MONGODB_URI (server/.env) points at an Atlas cluster
 *   (cluster0.zt2xpwa.mongodb.net) whose DNS no longer resolves from this environment —
 *   confirmed by direct lookup, not assumed. The harness spins up a disposable local
 *   MongoDB (mongodb-memory-server) and overrides MONGODB_URI for the spawned Node
 *   process only; server/.env itself is never touched.
 * - Both apps' JWT secrets are overridden to harness-known values for this run only,
 *   so the harness can mint validly-signed tokens for each side's "missing user" /
 *   "disabled user" / "expired token" scenarios without needing to read either app's
 *   real secret out of user-secrets or .env.
 * - The ASP.NET side runs against a dedicated `WoofBnB_Parity` database (not the
 *   `WoofBnB` database used by manual/dev testing), dropped and recreated fresh at the
 *   start of every run via the same InitialCreate migration already committed in
 *   Phase 3 — no schema changes, no shared state with anything else on this machine.
 */

const nodePort = Number(process.env.PARITY_NODE_PORT ?? 5050);
const dotnetPort = Number(process.env.PARITY_DOTNET_PORT ?? 5251);

export const config = {
  repoRoot,

  node: {
    cwd: path.join(repoRoot, "server"),
    entryScript: path.join(repoRoot, "server", "src", "server.js"),
    port: nodePort,
    baseUrl: `http://localhost:${nodePort}/api`,
    healthUrl: `http://localhost:${nodePort}/api/health`,
    jwtSecret: "parity-harness-known-secret-for-node-side-only",
    jwtExpiresIn: "1d",
    bcryptSaltRounds: 10,
  },

  dotnet: {
    apiProjectDir: path.join(repoRoot, "server-dotnet-claude", "src", "WoofBnB.Api"),
    infrastructureProjectDir: path.join(repoRoot, "server-dotnet-claude", "src", "WoofBnB.Infrastructure"),
    port: dotnetPort,
    baseUrl: `http://localhost:${dotnetPort}/api`,
    healthUrl: `http://localhost:${dotnetPort}/api/health`,
    jwtSecret: "parity-harness-known-secret-for-dotnet-side-only-32bytes",
    jwtExpiresInMinutes: 1440,
    bcryptWorkFactor: 10,
    databaseName: "WoofBnB_Parity",
    connectionString:
      "Server=(localdb)\\MSSQLLocalDB;Database=WoofBnB_Parity;Trusted_Connection=True;TrustServerCertificate=True;",
    corsClientUrl: "http://localhost:5173",
  },

  readiness: {
    timeoutMs: 60_000,
    pollIntervalMs: 500,
  },

  reportPath: path.join(here, "..", "PARITY_REPORT.md"),
};
