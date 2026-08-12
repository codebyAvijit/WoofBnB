import { compareResponses } from "./compare/compare.js";

/**
 * Differences classified as documented on EVERY scenario, not just specific ones —
 * `errorCode` is decision D-2 (an additive field Node's contract never had at all, by
 * design), so its presence-only-on-.NET is expected on every single error response,
 * not just a hand-picked few. Scenario-specific `expectedDifferences` are for
 * per-scenario known divergences (D-3, the Phase-5 radius gap, etc.); this list is for
 * structural, always-present differences that apply globally.
 */
const GLOBAL_EXPECTED_DIFFERENCES = [
  {
    path: "$.errorCode",
    decision: "D-2",
    note:
      "Node's ApiError has no error-code concept at all. errorCode is an additive field on the ASP.NET side's error " +
      "envelope, approved as non-breaking since nothing in the approved frontend reads it — present on every ASP.NET " +
      "error response and absent on every Node one, by design, not by accident.",
  },
  {
    path: "$.stack",
    decision: "documented-architectural-asymmetry",
    note:
      "Confirmed by this run, refining normalize.js's original assumption: stack presence is NOT simply " +
      "'both sides attach it under equivalent dev settings'. Node's validate.middleware.js builds and returns its " +
      "400 response directly — it never reaches server/src/middlewares/error.middleware.js, which is the ONLY place " +
      "Node ever attaches `err.stack`. So Node NEVER includes stack on a validation-error (400) response, in any " +
      "environment, while ASP.NET's ExceptionHandlingMiddleware attaches Stack uniformly to every AppException in " +
      "Development, including validation ones. For every other error type (401/403/404/409, routed through Node's " +
      "error.middleware.js), presence was confirmed symmetric once NODE_ENV=development was set for the harness's " +
      "Node process (see nodeProcess.js) — only the validation-error path is architecturally asymmetric by Node's " +
      "own design, not by a harness or ASP.NET defect.",
  },
];

function classifyDifferences(differences, expectedDifferences = []) {
  const allExpected = [...GLOBAL_EXPECTED_DIFFERENCES, ...expectedDifferences];
  const documented = [];
  const unjustified = [];

  for (const difference of differences) {
    const match = allExpected.find((expected) => expected.path === difference.path);

    if (match) {
      documented.push({ ...difference, decision: match.decision, note: match.note });
    } else {
      unjustified.push(difference);
    }
  }

  return { documented, unjustified };
}

/**
 * When a scenario explicitly expects the STATUS CODE itself to differ (e.g. Node 500 vs
 * ASP.NET 401 for decision D-3), continuing on to structurally diff two response bodies
 * that represent fundamentally different failure modes would manufacture dozens of
 * secondary "differences" that don't mean anything on their own — the one difference
 * that matters (the status) is already known and already justified. If the statuses
 * turn out to actually match (the expected divergence didn't materialize this run), the
 * full body comparison still runs normally, because then a real comparison IS meaningful.
 */
function runComparison(scenario, nodeResult, dotnetResult) {
  if (scenario.customCompare) {
    return scenario.customCompare({ nodeResult, dotnetResult });
  }

  const statusExpectedToDiffer = (scenario.expectedDifferences ?? []).some((d) => d.path === "$.status");

  if (statusExpectedToDiffer && nodeResult.status !== dotnetResult.status) {
    return {
      pass: false,
      differences: [{ path: "$.status", kind: "status-mismatch", node: nodeResult.status, dotnet: dotnetResult.status }],
    };
  }

  return compareResponses(nodeResult, dotnetResult, { unorderedArrayKeys: scenario.unorderedArrayKeys });
}

export async function runScenarios(scenarios, context) {
  const { callNode, callDotnet } = context;
  const results = [];

  for (const scenario of scenarios) {
    let fixture;
    let nodeRequest;
    let dotnetRequest;
    let nodeResult;
    let dotnetResult;
    let error = null;

    try {
      fixture = await scenario.setup(context);
      nodeRequest = scenario.buildRequest(fixture, "node");
      dotnetRequest = scenario.buildRequest(fixture, "dotnet");

      [nodeResult, dotnetResult] = await Promise.all([callNode(nodeRequest), callDotnet(dotnetRequest)]);
    } catch (caught) {
      error = caught;
    }

    if (error) {
      results.push({
        name: scenario.name,
        module: scenario.module,
        notes: scenario.notes,
        error: error.message,
        documented: [],
        unjustified: [{ path: "$", kind: "harness-error", detail: error.stack }],
      });
      console.error(`[FAIL] ${scenario.name} — harness error: ${error.message}`);
      continue;
    }

    const { documented, unjustified } = classifyDifferences(
      runComparison(scenario, nodeResult, dotnetResult).differences,
      scenario.expectedDifferences,
    );

    results.push({
      name: scenario.name,
      module: scenario.module,
      notes: scenario.notes,
      nodeRequest,
      dotnetRequest,
      nodeResult,
      dotnetResult,
      documented,
      unjustified,
    });

    const status = unjustified.length === 0 ? "PASS" : "FAIL";
    console.log(`[${status}] ${scenario.name}${documented.length ? ` (${documented.length} documented diff)` : ""}`);

    if (unjustified.length > 0) {
      for (const diff of unjustified) {
        console.log(`         unjustified: ${diff.path} — ${diff.kind}`);
      }
    }
  }

  return results;
}
