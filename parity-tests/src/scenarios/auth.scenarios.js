import bcrypt from "bcrypt";
import crypto from "node:crypto";
import { signToken, signExpiredToken } from "../auth/jwt.js";

/**
 * Each scenario's `setup` seeds fixtures into BOTH backends' own databases (via the
 * raw-driver seeders — there is no public "create user" endpoint on either side) and
 * returns a `{ node, dotnet }` fixture object so `buildRequest` can address each side
 * with data appropriate to that side's own ID format (Mongo ObjectId vs GUID) and its
 * own JWT secret.
 *
 * `expectedDifferences` names differences this scenario is KNOWN, ahead of the run, to
 * produce — each pointing at the audit decision that already justifies it. The
 * comparator still runs the full diff regardless; this list only changes how the
 * report CLASSIFIES whatever is actually found, never what is found.
 */
export function createAuthScenarios({ config, mongoSeeder, sqlSeeder }) {
  async function seedActiveUser(email, password) {
    const passwordHash = await bcrypt.hash(password, 10);
    const node = await mongoSeeder.seedUser({ name: "Parity Admin", email, passwordHash, isActive: true });
    const dotnet = await sqlSeeder.seedUser({ name: "Parity Admin", email, passwordHash, isActive: true });
    return { node, dotnet, password };
  }

  async function seedDisabledUser(email, password) {
    const passwordHash = await bcrypt.hash(password, 10);
    const node = await mongoSeeder.seedUser({ name: "Parity Disabled Admin", email, passwordHash, isActive: false });
    const dotnet = await sqlSeeder.seedUser({ name: "Parity Disabled Admin", email, passwordHash, isActive: false });
    return { node, dotnet, password };
  }

  return [
    {
      name: "auth/login-success",
      module: "auth",
      notes: "Valid credentials for an active account must succeed identically on both sides.",
      setup: () => seedActiveUser("login-success@parity.test", "CorrectPassword123"),
      buildRequest: (fixture) => ({
        method: "POST",
        path: "/auth/login",
        body: { email: "login-success@parity.test", password: fixture.password },
      }),
    },
    {
      name: "auth/login-wrong-password",
      module: "auth",
      notes: "Node's exact message (\"Invalid email or password\") must not distinguish wrong-password from unknown-email.",
      setup: () => seedActiveUser("login-wrongpw@parity.test", "CorrectPassword123"),
      buildRequest: () => ({
        method: "POST",
        path: "/auth/login",
        body: { email: "login-wrongpw@parity.test", password: "TotallyWrongPassword" },
      }),
    },
    {
      name: "auth/login-unknown-email",
      module: "auth",
      notes: "No account with this email exists on either side.",
      setup: () => ({}),
      buildRequest: () => ({
        method: "POST",
        path: "/auth/login",
        body: { email: "nobody-registered@parity.test", password: "SomePassword123" },
      }),
    },
    {
      name: "auth/login-mixedcase-email",
      module: "auth",
      notes:
        "Decision D-7 theorized Node's login lookup is case-sensitive against lowercased storage (the incoming " +
        "email is trimmed by Zod but never lowercased before the repository query) and would therefore 401 here. " +
        "Verified empirically against a live Mongo for the first time, rather than assumed from reading the " +
        "Mongoose schema/query code alone.",
      setup: () => seedActiveUser("login-mixedcase@parity.test", "CorrectPassword123"),
      buildRequest: (fixture) => ({
        method: "POST",
        path: "/auth/login",
        body: { email: "Login-MixedCase@Parity.test", password: fixture.password },
      }),
    },
    {
      name: "auth/login-disabled-account",
      module: "auth",
      notes: "server/src/modules/auth/auth.service.js checks isActive BEFORE comparing the password — 403, not 401, even with the correct password.",
      setup: () => seedDisabledUser("login-disabled@parity.test", "CorrectPassword123"),
      buildRequest: (fixture) => ({
        method: "POST",
        path: "/auth/login",
        body: { email: "login-disabled@parity.test", password: fixture.password },
      }),
    },
    {
      name: "auth/login-empty-body",
      module: "auth",
      notes:
        "Both email and password keys entirely absent. Known edge case (Phase 4 audit): Zod's custom .email()/.min(8) messages apply to a " +
        "present-but-invalid value, not necessarily to a wholly MISSING key, which may produce Zod's own default type-error message instead. " +
        "Run honestly and report whatever is actually found.",
      setup: () => ({}),
      unorderedArrayKeys: ["errors"],
      buildRequest: () => ({ method: "POST", path: "/auth/login", body: {} }),
    },
    {
      name: "auth/login-invalid-format-present",
      module: "auth",
      notes: "Both fields present but invalid (not missing) — the case both sides' custom validator messages were specifically written for.",
      setup: () => ({}),
      unorderedArrayKeys: ["errors"],
      buildRequest: () => ({
        method: "POST",
        path: "/auth/login",
        body: { email: "not-an-email", password: "short1" },
      }),
    },
    {
      name: "auth/me-missing-token",
      module: "auth",
      notes: "No Authorization header at all.",
      setup: () => ({}),
      buildRequest: () => ({ method: "GET", path: "/auth/me" }),
    },
    {
      name: "auth/me-malformed-token",
      module: "auth",
      notes: "A syntactically invalid JWT.",
      setup: () => ({}),
      expectedDifferences: [
        {
          path: "$.status",
          decision: "D-3",
          note:
            "Node's authenticate middleware never catches jwt.verify's throw, so a malformed token 500s " +
            "(server/src/middlewares/auth.middleware.js). The ASP.NET side deliberately fixes this to 401 " +
            "per CLAUDE.md §9 (never expose internal exception details) — an intentional, already-approved deviation, not a defect.",
        },
      ],
      buildRequest: () => ({
        method: "GET",
        path: "/auth/me",
        headers: { Authorization: "Bearer not-a-real-jwt-at-all" },
      }),
    },
    {
      name: "auth/me-expired-token",
      module: "auth",
      notes: "A validly-signed but expired token, signed separately for each side with that side's own known secret.",
      setup: () => ({
        node: { token: signExpiredToken({ secret: config.node.jwtSecret, id: "64f0000000000000000000aa", role: "admin" }) },
        dotnet: { token: signExpiredToken({ secret: config.dotnet.jwtSecret, id: crypto.randomUUID(), role: "admin" }) },
      }),
      expectedDifferences: [
        {
          path: "$.status",
          decision: "D-3",
          note: "Same root cause as me-malformed-token — jwt.verify's TokenExpiredError is equally uncaught by Node's middleware.",
        },
      ],
      buildRequest: (fixture, target) => ({
        method: "GET",
        path: "/auth/me",
        headers: { Authorization: `Bearer ${fixture[target].token}` },
      }),
    },
    {
      name: "auth/me-valid-token-success",
      module: "auth",
      notes: "A real, active, seeded user, addressed by each side's own generated id, in a validly-signed token for that side.",
      setup: async () => {
        const fixture = await seedActiveUser("me-success@parity.test", "CorrectPassword123");
        return {
          node: { token: signToken({ secret: config.node.jwtSecret, id: fixture.node.id, role: "admin" }) },
          dotnet: { token: signToken({ secret: config.dotnet.jwtSecret, id: fixture.dotnet.id, role: "admin" }) },
        };
      },
      buildRequest: (fixture, target) => ({
        method: "GET",
        path: "/auth/me",
        headers: { Authorization: `Bearer ${fixture[target].token}` },
      }),
    },
    {
      name: "auth/me-valid-token-missing-user",
      module: "auth",
      notes:
        "A validly-signed token whose id is well-formed for that side's own ID scheme (a syntactically valid but nonexistent Mongo " +
        "ObjectId for Node, a nonexistent GUID for .NET) yet matches no real account — a deleted user's stale session.",
      setup: () => ({
        node: { token: signToken({ secret: config.node.jwtSecret, id: "64f0000000000000000000bb", role: "admin" }) },
        dotnet: { token: signToken({ secret: config.dotnet.jwtSecret, id: crypto.randomUUID(), role: "admin" }) },
      }),
      buildRequest: (fixture, target) => ({
        method: "GET",
        path: "/auth/me",
        headers: { Authorization: `Bearer ${fixture[target].token}` },
      }),
    },
    {
      name: "auth/me-valid-token-disabled-user",
      module: "auth",
      notes: "A real, disabled, seeded user — the token itself is perfectly valid; the account is not.",
      setup: async () => {
        const fixture = await seedDisabledUser("me-disabled@parity.test", "CorrectPassword123");
        return {
          node: { token: signToken({ secret: config.node.jwtSecret, id: fixture.node.id, role: "admin" }) },
          dotnet: { token: signToken({ secret: config.dotnet.jwtSecret, id: fixture.dotnet.id, role: "admin" }) },
        };
      },
      buildRequest: (fixture, target) => ({
        method: "GET",
        path: "/auth/me",
        headers: { Authorization: `Bearer ${fixture[target].token}` },
      }),
    },
  ];
}
