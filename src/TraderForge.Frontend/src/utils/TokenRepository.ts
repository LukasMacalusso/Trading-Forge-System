const TOKEN_KEY = 'tf_jwt';
const REFRESH_TOKEN_KEY = 'tf_refresh';

/** Abstracts localStorage access for the JWT tokens. */
export const TokenRepository = {
  get: (): string | null => localStorage.getItem(TOKEN_KEY),
  getRefresh: (): string | null => localStorage.getItem(REFRESH_TOKEN_KEY),
  save: (token: string, refreshToken?: string): void => {
    localStorage.setItem(TOKEN_KEY, token);
    if (refreshToken) localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  },
  clear: (): void => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
  },
};
