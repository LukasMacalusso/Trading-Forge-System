export type UserRole = 'Trader' | 'SystemAdmin';

/** ASP.NET serialises `ClaimTypes.Role` under this URI in the JWT payload. */
const ROLE_CLAIM_URI = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

interface JwtClaims {
  sub?: string;
  email?: string;
  role?: string | string[];
  [key: string]: unknown;
}

/**
 * Decodes a JWT payload without verifying its signature. Verification lives on
 * the backend — this only reads claims the client already trusts (the server
 * issued the token) for display and role-based routing.
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

/** Reads the trader's role from the token, tolerant of the claim-key variants. */
export function getRoleFromToken(token: string | null): UserRole | null {
  const claims = decodeJwt(token);
  if (!claims) return null;
  const raw = claims.role ?? (claims[ROLE_CLAIM_URI] as string | string[] | undefined);
  const value = Array.isArray(raw) ? raw[0] : raw;
  return value === 'SystemAdmin' || value === 'Trader' ? value : null;
}
