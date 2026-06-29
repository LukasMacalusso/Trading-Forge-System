/** Claims emitted by the backend identity token (see IdentityService). */
interface JwtClaims {
  sub?: string;
  email?: string;
  [key: string]: unknown;
}

/**
 * Decodes the payload of a JWT without verifying its signature.
 * Verification stays on the backend — this only reads display claims (email,
 * subject) the client already trusts because the server issued the token.
 */
export function decodeJwt(token: string | null): JwtClaims | null {
  if (!token) return null;
  const payload = token.split('.')[1];
  if (!payload) return null;
  try {
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
    const json = decodeURIComponent(
      atob(normalized)
        .split('')
        .map((c) => `%${c.charCodeAt(0).toString(16).padStart(2, '0')}`)
        .join(''),
    );
    return JSON.parse(json) as JwtClaims;
  } catch {
    return null;
  }
}
