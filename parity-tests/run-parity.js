import { MongoMemoryServer } from "mongodb-memory-server";
import { config } from "./src/config.js";
import { createNodeProcess } from "./src/process/nodeProcess.js";
import { createDotnetProcess } from "./src/process/dotnetProcess.js";
import { createMongoSeeder } from "./src/seed/mongoSeed.js";
import { createSqlSeeder } from "./src/seed/sqlSeed.js";
import { callApi } from "./src/http/httpClient.js";
import { createAuthScenarios } from "./src/scenarios/auth.scenarios.js";
import { createPetSitterScenarios } from "./src/scenarios/petsitter.scenarios.js";
import { runScenarios } from "./src/runner.js";
import { renderReport, writeReport } from "./src/report.js";

const startedAt = new Date();

console.log("=== WoofBnB Phase 8 — API Differential Parity Harness ===\n");

console.log("Starting disposable local MongoDB (mongodb-memory-server)...");
const mongod = await MongoMemoryServer.create();
const mongoUri = mongod.getUri("woofbnb");
console.log(`  -> ${mongoUri}\n`);

const nodeProcess = createNodeProcess(config);
const dotnetProcess = createDotnetProcess(config);

let results = [];
let hardFailure = null;

try {
  console.log("Starting Node.js backend (real server/src/server.js, unmodified)...");
  await nodeProcess.start(mongoUri);
  console.log(`  -> ${config.node.baseUrl}\n`);

  console.log("Resetting WoofBnB_Parity and starting the ASP.NET Core backend...");
  await dotnetProcess.start();
  console.log(`  -> ${config.dotnet.baseUrl}\n`);

  const mongoSeeder = createMongoSeeder(mongoUri, "woofbnb");
  await mongoSeeder.connect();
  const sqlSeeder = createSqlSeeder(config.dotnet.connectionString, config.dotnet.databaseName);

  const callNode = (request) => callApi(config.node.baseUrl, request);
  const callDotnet = (request) => callApi(config.dotnet.baseUrl, request);

  const scenarios = [
    ...createAuthScenarios({ config, mongoSeeder, sqlSeeder }),
    ...createPetSitterScenarios({ callNode, callDotnet }),
  ];

  console.log(`Running ${scenarios.length} scenarios sequentially...\n`);
  results = await runScenarios(scenarios, { callNode, callDotnet });

  await mongoSeeder.disconnect();
} catch (error) {
  hardFailure = error;
  console.error("\nFATAL: harness could not complete the run.");
  console.error(error);
} finally {
  console.log("\nTearing down both backends and the disposable MongoDB...");
  await nodeProcess.stop();
  await dotnetProcess.stop();
  await mongod.stop();
}

if (hardFailure) {
  process.exit(2);
}

const finishedAt = new Date();
const report = renderReport(results, { startedAt, finishedAt });
writeReport(config.reportPath, report);

const totalUnjustified = results.reduce((sum, r) => sum + r.unjustified.length, 0);
const totalDocumented = results.reduce((sum, r) => sum + r.documented.length, 0);
const passed = results.filter((r) => r.unjustified.length === 0).length;

console.log("\n=== SUMMARY ===");
console.log(`Scenarios: ${passed}/${results.length} passed`);
console.log(`Unjustified differences: ${totalUnjustified}`);
console.log(`Documented differences: ${totalDocumented}`);
console.log(`Report written to: ${config.reportPath}`);

process.exit(totalUnjustified > 0 ? 1 : 0);
