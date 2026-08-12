/**
 * Every field ignored by the differential comparison, and why. This is the single
 * source of truth for "volatile" — nothing is ignored anywhere else in the harness.
 * None of these key names are reused elsewhere in the API surface for anything else
 * (verified against server/src/modules/**\/*.mapper.js and the .NET DTOs in
 * server-dotnet-claude/src/WoofBnB.Application/**\/DTOs/**), so matching by trailing
 * key name — rather than a full structural path — is safe and stays readable.
 *
 * A normalized field's PRESENCE and TYPE are still compared; only its exact VALUE is
 * ignored. A field present on one side and absent on the other is still reported as a
 * real difference, even if that field's name is in this list.
 */
export const VOLATILE_FIELDS = [
  {
    key: "id",
    reason:
      "Primary key. Node uses a MongoDB ObjectId (24-hex string); the ASP.NET side " +
      "uses a GUID (decision D-1). Different format and different value on every " +
      "insert on both sides — inherently incomparable by value, only by type=string.",
  },
  {
    key: "createdAt",
    reason: "Set from each side's own wall clock at insert time; the two live processes never insert at the identical instant.",
  },
  {
    key: "updatedAt",
    reason: "Same as createdAt — set from each side's own wall clock.",
  },
  {
    key: "lastLogin",
    reason: "Set to \"now\" by each side's own login call; the two calls happen microseconds apart at best.",
  },
  {
    key: "timestamp",
    reason: "The ApiResponse/ApiError envelope's own generation timestamp — by definition different on every call to two separate processes.",
  },
  {
    key: "accessToken",
    reason:
      "Each side signs with a harness-assigned secret that intentionally differs between the two processes (see config.js) — the token strings can never match. " +
      "Presence and non-empty-string-ness are still checked.",
  },
  {
    key: "stack",
    reason:
      "Dev-only debug field. Content is a language-specific stack trace with file paths unique to each codebase — never " +
      "comparable by value, so the value is always ignored here. PRESENCE is compared, but — confirmed by an actual run, " +
      "not assumed — is only symmetric for NON-validation errors (401/403/404/409), which route through " +
      "server/src/middlewares/error.middleware.js on Node and WoofBnB.Api's ExceptionHandlingMiddleware on .NET, both " +
      "attaching stack under equivalent dev settings. Validation errors (400) never get a stack on Node at all, in any " +
      "environment, because validate.middleware.js returns its response directly and never reaches error.middleware.js — " +
      "while ASP.NET's ExceptionHandlingMiddleware attaches Stack uniformly to every AppException including validation " +
      "ones. This asymmetry is therefore treated as globally expected (see runner.js's GLOBAL_EXPECTED_DIFFERENCES), not " +
      "silently dropped — every occurrence is still surfaced in the report as a documented difference, just not counted " +
      "as an unjustified one.",
  },
];

const VOLATILE_KEYS = new Set(VOLATILE_FIELDS.map((f) => f.key));

/**
 * Deep-clones `value`, replacing any volatile leaf with a sentinel that still records
 * its original JS type (so compare.js can assert type equality without ever looking at
 * the actual value).
 */
export function normalize(value, keyName) {
  if (Array.isArray(value)) {
    return value.map((item) => normalize(item));
  }

  if (value !== null && typeof value === "object") {
    const result = {};
    for (const [key, child] of Object.entries(value)) {
      result[key] = normalize(child, key);
    }
    return result;
  }

  if (keyName && VOLATILE_KEYS.has(keyName)) {
    return { __normalized__: true, type: value === null ? "null" : typeof value };
  }

  return value;
}
