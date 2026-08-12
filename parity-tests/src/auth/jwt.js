import crypto from "node:crypto";

function base64url(input) {
  return Buffer.from(input).toString("base64").replace(/=+$/, "").replace(/\+/g, "-").replace(/\//g, "_");
}

/**
 * Minimal, dependency-free HS256 JWT signer — deliberately NOT reusing jsonwebtoken
 * (Node's) or System.IdentityModel (.NET's) libraries, so the harness's own signing
 * logic can't accidentally inherit either side's quirks. Produces exactly the
 * { id, role, iat, exp } payload both server/src/modules/auth/auth.token.service.js
 * and WoofBnB.Infrastructure.Security.JwtTokenService emit.
 */
export function signToken({ secret, id, role, expiresInSeconds = 86400 }) {
  const nowSeconds = Math.floor(Date.now() / 1000);

  const header = { alg: "HS256", typ: "JWT" };
  const payload = { id, role, iat: nowSeconds, exp: nowSeconds + expiresInSeconds };

  const encodedHeader = base64url(JSON.stringify(header));
  const encodedPayload = base64url(JSON.stringify(payload));
  const signingInput = `${encodedHeader}.${encodedPayload}`;

  const signature = crypto.createHmac("sha256", secret).update(signingInput).digest("base64")
    .replace(/=+$/, "").replace(/\+/g, "-").replace(/\//g, "_");

  return `${signingInput}.${signature}`;
}

export function signExpiredToken({ secret, id, role }) {
  return signToken({ secret, id, role, expiresInSeconds: -3600 });
}
