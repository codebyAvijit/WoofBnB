/**
 * Coordinate clusters are deliberately far apart so radius-bounded nearby queries in
 * one scenario can never see fixtures created by another — the harness runs scenarios
 * sequentially against long-lived, accumulating databases (mirroring a real running
 * system) rather than resetting state between every scenario, so this separation is
 * what keeps radius-bounded scenarios independent without needing per-scenario cleanup.
 */
const CLUSTERS = {
  createBasic: { lng: 77.209, lat: 28.6139 }, // Connaught Place, New Delhi — matches server/docs/ examples
  nearbyOrdering: { lng: 72.8777, lat: 19.076 }, // Mumbai
  nearbyDefaultRadius: { lng: 77.5946, lat: 12.9716 }, // Bangalore
  nearbyEmpty: { lng: 0.0, lat: 0.0 }, // Null Island — nothing is ever seeded here
};

function validPayload(overrides = {}) {
  return {
    name: "Parity Pet Sitter",
    email: "placeholder@parity.test",
    phone: "9876543210",
    bio: "Professional pet sitter with plenty of experience caring for pets and dogs.",
    address: "Connaught Place, New Delhi",
    location: { type: "Point", coordinates: [CLUSTERS.createBasic.lng, CLUSTERS.createBasic.lat] },
    workingHours: { start: "09:00", end: "18:00" },
    amenities: ["Dog Walking", "Indoor Stay"],
    profileImage: "",
    ...overrides,
  };
}

/**
 * Creates a pet sitter through each side's OWN public POST /petsitters — there is no
 * raw-DB seeder for this module (unlike auth/users): the create endpoint is public on
 * both sides, so seeding through it is more faithful than reaching around it, and it
 * doubles as exercising the exact code path being tested elsewhere.
 */
async function createOnBothSides({ callNode, callDotnet }, payload) {
  const [node, dotnet] = await Promise.all([
    callNode({ method: "POST", path: "/petsitters", body: payload }),
    callDotnet({ method: "POST", path: "/petsitters", body: payload }),
  ]);
  return { node, dotnet };
}

export function createPetSitterScenarios({ callNode, callDotnet }) {
  return [
    {
      name: "petsitter/create-success",
      module: "petsitter",
      notes: "Full valid payload — deep-compares the entire nested response shape.",
      setup: () => ({}),
      buildRequest: () => ({
        method: "POST",
        path: "/petsitters",
        body: validPayload({ email: "create-success@parity.test" }),
      }),
    },
    {
      name: "petsitter/create-empty-amenities-array",
      module: "petsitter",
      notes: "amenities: [] is a valid value (the key just has to be present) on both sides.",
      setup: () => ({}),
      buildRequest: () => ({
        method: "POST",
        path: "/petsitters",
        body: validPayload({ email: "create-empty-amenities@parity.test", amenities: [] }),
      }),
    },
    {
      name: "petsitter/create-name-too-short",
      module: "petsitter",
      setup: () => ({}),
      buildRequest: () => ({
        method: "POST",
        path: "/petsitters",
        body: validPayload({ email: "create-name-short@parity.test", name: "J" }),
      }),
    },
    {
      name: "petsitter/create-invalid-email",
      module: "petsitter",
      setup: () => ({}),
      buildRequest: () => ({
        method: "POST",
        path: "/petsitters",
        body: validPayload({ email: "not-an-email" }),
      }),
    },
    {
      name: "petsitter/create-invalid-phone",
      module: "petsitter",
      setup: () => ({}),
      buildRequest: () => ({
        method: "POST",
        path: "/petsitters",
        body: validPayload({ email: "create-phone-invalid@parity.test", phone: "123" }),
      }),
    },
    {
      name: "petsitter/create-bio-too-short",
      module: "petsitter",
      setup: () => ({}),
      buildRequest: () => ({
        method: "POST",
        path: "/petsitters",
        body: validPayload({ email: "create-bio-short@parity.test", bio: "too short" }),
      }),
    },
    {
      name: "petsitter/create-address-too-short",
      module: "petsitter",
      setup: () => ({}),
      buildRequest: () => ({
        method: "POST",
        path: "/petsitters",
        body: validPayload({ email: "create-address-short@parity.test", address: "abcd" }),
      }),
    },
    {
      name: "petsitter/create-missing-amenities-key",
      module: "petsitter",
      notes: "The amenities key is entirely absent, not an empty array.",
      setup: () => ({}),
      buildRequest: () => {
        const payload = validPayload({ email: "create-amenities-missing@parity.test" });
        delete payload.amenities;
        return { method: "POST", path: "/petsitters", body: payload };
      },
    },
    {
      name: "petsitter/create-invalid-amenity-value",
      module: "petsitter",
      setup: () => ({}),
      buildRequest: () => ({
        method: "POST",
        path: "/petsitters",
        body: validPayload({ email: "create-amenity-invalid@parity.test", amenities: ["Dog Walking", "Not A Real Amenity"] }),
      }),
    },
    {
      name: "petsitter/create-coordinates-out-of-range",
      module: "petsitter",
      notes:
        "CONFIRMED EMPIRICALLY (first live run of this exact scenario): Node does not silently accept out-of-range " +
        "coordinates as decision D-5 originally assumed from reading the Zod schema alone. " +
        "server/src/modules/petsitter/petsitter.model.js also declares a 2dsphere index, and MongoDB rejects an " +
        "out-of-range GeoJSON Point for an indexed geometry at write time ('Can't extract geo keys ... " +
        "Longitude/latitude is out of bounds'). That error is a plain MongoServerError, not an AppError, so Node's " +
        "petsitter service never catches it — it falls through to the generic error middleware as an unhandled 500, " +
        "which also LEAKS the full inserted document (including profileImage, workingHours, etc.) and the raw driver " +
        "error text into the response body. D-5's conclusion stands and is now confirmed rather than assumed: Node's " +
        "real failure mode here is a raw, information-leaking 500, and the ASP.NET side's clean 400 is a correct, " +
        "deliberate improvement — not a parity defect to fix.",
      setup: () => ({}),
      expectedDifferences: [
        {
          path: "$.status",
          decision: "D-5",
          note: "Node 500s via an uncaught MongoServerError (2dsphere index rejects the out-of-range GeoJSON Point); ASP.NET's validator " +
            "produces a clean 400 by design. See scenario notes for the full empirical confirmation.",
        },
      ],
      buildRequest: () => ({
        method: "POST",
        path: "/petsitters",
        body: validPayload({
          email: "create-coords-outofrange@parity.test",
          location: { type: "Point", coordinates: [200.0, 28.6139] },
        }),
      }),
    },
    {
      name: "petsitter/create-duplicate-email-exact-case",
      module: "petsitter",
      setup: async (ctx) => {
        await createOnBothSides(ctx, validPayload({ email: "duplicate-exact@parity.test" }));
        return {};
      },
      buildRequest: () => ({
        method: "POST",
        path: "/petsitters",
        body: validPayload({ email: "duplicate-exact@parity.test" }),
      }),
    },
    {
      name: "petsitter/create-duplicate-email-mixed-case",
      module: "petsitter",
      notes:
        "Decision D-8 theorized Node's own case-sensitive pre-check misses this and 500s at Mongo's unique index. " +
        "Verified empirically here against a live Mongo for the first time, rather than assumed.",
      setup: async (ctx) => {
        await createOnBothSides(ctx, validPayload({ email: "duplicate-mixedcase@parity.test" }));
        return {};
      },
      buildRequest: () => ({
        method: "POST",
        path: "/petsitters",
        body: validPayload({ email: "Duplicate-MixedCase@Parity.test" }),
      }),
    },
    {
      name: "petsitter/create-unauthenticated",
      module: "petsitter",
      notes: "No Authorization header — server/src/modules/petsitter/petsitter.routes.js applies no authenticate middleware to any route.",
      setup: () => ({}),
      buildRequest: () => ({
        method: "POST",
        path: "/petsitters",
        body: validPayload({ email: "create-no-auth@parity.test" }),
      }),
    },
    {
      name: "petsitter/getall-ordering",
      module: "petsitter",
      notes:
        "Creates two distinctly-named sitters in sequence on both sides, then checks their RELATIVE order in GET / — " +
        "robust against every other sitter this run has already accumulated, since it only asserts the relative " +
        "position of these two known names rather than exact array equality.",
      setup: async (ctx) => {
        await createOnBothSides(ctx, validPayload({ email: "getall-older@parity.test", name: "GetAll Older Sitter" }));
        await new Promise((resolve) => setTimeout(resolve, 50));
        await createOnBothSides(ctx, validPayload({ email: "getall-newer@parity.test", name: "GetAll Newer Sitter" }));
        return {};
      },
      buildRequest: () => ({ method: "GET", path: "/petsitters" }),
      customCompare: ({ nodeResult, dotnetResult }) => {
        const differences = [];

        if (nodeResult.status !== dotnetResult.status) {
          differences.push({ path: "$.status", kind: "status-mismatch", node: nodeResult.status, dotnet: dotnetResult.status });
          return { pass: false, differences };
        }

        for (const [label, result] of [["node", nodeResult], ["dotnet", dotnetResult]]) {
          const names = result.body?.data?.map((d) => d.name) ?? [];
          const olderIndex = names.indexOf("GetAll Older Sitter");
          const newerIndex = names.indexOf("GetAll Newer Sitter");

          if (olderIndex === -1 || newerIndex === -1) {
            differences.push({ path: `$.data (${label})`, kind: "missing-fixture", detail: "Seeded fixture not found in response." });
          } else if (newerIndex >= olderIndex) {
            differences.push({
              path: `$.data (${label})`,
              kind: "ordering-violation",
              detail: `Expected the more recently created sitter first (createdAt DESC); newerIndex=${newerIndex}, olderIndex=${olderIndex}.`,
            });
          }
        }

        return { pass: differences.length === 0, differences };
      },
    },
    {
      name: "petsitter/getall-unauthenticated",
      module: "petsitter",
      setup: () => ({}),
      buildRequest: () => ({ method: "GET", path: "/petsitters" }),
      customCompare: ({ nodeResult, dotnetResult }) => ({
        pass: nodeResult.status === dotnetResult.status && nodeResult.status === 200,
        differences:
          nodeResult.status === dotnetResult.status && nodeResult.status === 200
            ? []
            : [{ path: "$.status", kind: "status-mismatch", node: nodeResult.status, dotnet: dotnetResult.status }],
      }),
    },
    {
      name: "petsitter/nearby-ordering",
      module: "petsitter",
      notes: "A sitter ~150m from the query point must be returned before one ~2km away, on both sides.",
      setup: async (ctx) => {
        const origin = CLUSTERS.nearbyOrdering;
        await createOnBothSides(
          ctx,
          validPayload({
            email: "nearby-near@parity.test",
            name: "Nearby Near Sitter",
            location: { type: "Point", coordinates: [origin.lng + 0.0013, origin.lat] }, // ~150m
          }),
        );
        await createOnBothSides(
          ctx,
          validPayload({
            email: "nearby-far@parity.test",
            name: "Nearby Far Sitter",
            location: { type: "Point", coordinates: [origin.lng + 0.02, origin.lat] }, // ~2.1km
          }),
        );
        return {};
      },
      buildRequest: () => ({
        method: "GET",
        path: "/petsitters/nearby",
        query: { lat: CLUSTERS.nearbyOrdering.lat, lng: CLUSTERS.nearbyOrdering.lng, radius: 15000 },
      }),
      customCompare: ({ nodeResult, dotnetResult }) => {
        const differences = [];

        if (nodeResult.status !== dotnetResult.status) {
          differences.push({ path: "$.status", kind: "status-mismatch", node: nodeResult.status, dotnet: dotnetResult.status });
          return { pass: false, differences };
        }

        for (const [label, result] of [["node", nodeResult], ["dotnet", dotnetResult]]) {
          const names = (result.body?.data ?? [])
            .map((d) => d.name)
            .filter((name) => name === "Nearby Near Sitter" || name === "Nearby Far Sitter");

          if (names.join(",") !== "Nearby Near Sitter,Nearby Far Sitter") {
            differences.push({
              path: `$.data (${label})`,
              kind: "ordering-violation",
              detail: `Expected ["Nearby Near Sitter","Nearby Far Sitter"], got [${names.join(",")}]`,
            });
          }
        }

        return { pass: differences.length === 0, differences };
      },
    },
    {
      name: "petsitter/nearby-default-radius",
      module: "petsitter",
      notes:
        "CONFIRMED EMPIRICALLY, correcting the original audit's own theoretical analysis: omitting radius does NOT " +
        "produce an 'unbounded search' on Node as the Phase 2 audit assumed from reading the mongoose driver's casting " +
        "source alone. The real behavior, only visible by actually running the query, is a 500: MongoDB's server " +
        "rejects `$maxDistance: undefined` outright with \"$maxDistance must be a number\" — a hard query-execution " +
        "error, not a silently-unbounded result set. Decision D-6's RECOMMENDATION (implement the documented, working " +
        "5000m default) was already correct and remains correct; only the audit's description of Node's specific " +
        "failure mode needed this empirical correction.",
      expectedDifferences: [
        {
          path: "$.status",
          decision: "D-6",
          note:
            "Node 500s (\"$maxDistance must be a number\") because the Express 5 req.query getter has no setter, so " +
            "validate.middleware.js's write-back of the Zod-coerced/defaulted query object silently fails and the " +
            "raw (radius-less) query reaches Mongo's $near operator. ASP.NET's clean 5000m default is Node's own " +
            "documented, intended behavior working correctly — a deliberate fix of a confirmed Node bug, not a defect.",
        },
      ],
      setup: async (ctx) => {
        const origin = CLUSTERS.nearbyDefaultRadius;
        await createOnBothSides(
          ctx,
          validPayload({
            email: "nearby-default-in@parity.test",
            name: "Default Radius Within",
            location: { type: "Point", coordinates: [origin.lng + 0.003, origin.lat] }, // ~330m
          }),
        );
        await createOnBothSides(
          ctx,
          validPayload({
            email: "nearby-default-out@parity.test",
            name: "Default Radius Beyond",
            location: { type: "Point", coordinates: [origin.lng + 0.08, origin.lat] }, // ~8.7km
          }),
        );
        return {};
      },
      buildRequest: () => ({
        method: "GET",
        path: "/petsitters/nearby",
        query: { lat: CLUSTERS.nearbyDefaultRadius.lat, lng: CLUSTERS.nearbyDefaultRadius.lng },
      }),
      customCompare: ({ nodeResult, dotnetResult }) => {
        const differences = [];

        if (nodeResult.status !== dotnetResult.status) {
          differences.push({ path: "$.status", kind: "status-mismatch", node: nodeResult.status, dotnet: dotnetResult.status });
          return { pass: false, differences };
        }

        for (const [label, result] of [["node", nodeResult], ["dotnet", dotnetResult]]) {
          const names = new Set((result.body?.data ?? []).map((d) => d.name));

          if (!names.has("Default Radius Within")) {
            differences.push({ path: `$.data (${label})`, kind: "missing-fixture", detail: "Expected within-default-radius fixture missing." });
          }
          if (names.has("Default Radius Beyond")) {
            differences.push({ path: `$.data (${label})`, kind: "unexpected-fixture", detail: "Beyond-default-radius fixture should have been excluded." });
          }
        }

        return { pass: differences.length === 0, differences };
      },
    },
    {
      name: "petsitter/nearby-empty-result",
      module: "petsitter",
      notes: "Nothing is ever seeded at Null Island — data must be [], never null, on both sides.",
      setup: () => ({}),
      buildRequest: () => ({
        method: "GET",
        path: "/petsitters/nearby",
        query: { lat: CLUSTERS.nearbyEmpty.lat, lng: CLUSTERS.nearbyEmpty.lng, radius: 100 },
      }),
    },
    {
      name: "petsitter/nearby-invalid-latitude",
      module: "petsitter",
      setup: () => ({}),
      buildRequest: () => ({ method: "GET", path: "/petsitters/nearby", query: { lat: 91, lng: 77.209 } }),
    },
    {
      name: "petsitter/nearby-invalid-longitude",
      module: "petsitter",
      setup: () => ({}),
      buildRequest: () => ({ method: "GET", path: "/petsitters/nearby", query: { lat: 28.6139, lng: 181 } }),
    },
    {
      name: "petsitter/nearby-invalid-radius",
      module: "petsitter",
      setup: () => ({}),
      buildRequest: () => ({ method: "GET", path: "/petsitters/nearby", query: { lat: 28.6139, lng: 77.209, radius: 0 } }),
    },
    {
      name: "petsitter/nearby-missing-lat-lng",
      module: "petsitter",
      notes: "Both required query parameters entirely absent.",
      setup: () => ({}),
      unorderedArrayKeys: ["errors"],
      buildRequest: () => ({ method: "GET", path: "/petsitters/nearby", query: {} }),
    },
    {
      name: "petsitter/nearby-malformed-radius-string",
      module: "petsitter",
      notes:
        "Known, already-documented gap from the PetSitter phase: Node's z.coerce.number() turns \"abc\" into NaN, which fails " +
        ".positive() → 400. ASP.NET's query-string binder fails to parse \"abc\" into double?, silently leaves it null, and the " +
        "validator treats null as \"not provided\" → defaults to 5000 → 200. Expected to differ; reported as an accepted, " +
        "already-documented edge case, not investigated as a new defect.",
      setup: () => ({}),
      expectedDifferences: [
        {
          path: "$.status",
          decision: "Phase-5-known-gap",
          note: "Documented verbatim in the PetSitter phase report: a malformed (non-numeric) radius string is the one case " +
            "where 'missing' and 'unparseable' are NOT treated identically on the ASP.NET side, unlike lat/lng.",
        },
      ],
      buildRequest: () => ({
        method: "GET",
        path: "/petsitters/nearby",
        query: { lat: 28.6139, lng: 77.209, radius: "abc" },
      }),
    },
    {
      name: "petsitter/nearby-unauthenticated",
      module: "petsitter",
      setup: () => ({}),
      buildRequest: () => ({
        method: "GET",
        path: "/petsitters/nearby",
        query: { lat: 28.6139, lng: 77.209, radius: 5000 },
      }),
      customCompare: ({ nodeResult, dotnetResult }) => ({
        pass: nodeResult.status === dotnetResult.status && nodeResult.status === 200,
        differences:
          nodeResult.status === dotnetResult.status && nodeResult.status === 200
            ? []
            : [{ path: "$.status", kind: "status-mismatch", node: nodeResult.status, dotnet: dotnetResult.status }],
      }),
    },
  ];
}
