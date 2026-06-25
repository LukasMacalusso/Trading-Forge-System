const TOKEN_KEY = 'tf_jwt';

/** Abstracts localStorage access for the JWT token. */
export const TokenRepository = {
  get: (): string | null => localStorage.getItem(TOKEN_KEY),
  save: (token: string): void => localStorage.setItem(TOKEN_KEY, token),
  clear: (): void => localStorage.removeItem(TOKEN_KEY),
};
