import axios, { AxiosError } from 'axios';
import { API_BASE_URL } from '@utils/constants';
import { TokenRepository } from '@utils/TokenRepository';

export const httpClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

/** Injects the JWT token into every request automatically. */
httpClient.interceptors.request.use((config) => {
  const token = TokenRepository.get();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

let isRefreshing = false;
let failedQueue: Array<{ resolve: (token: string) => void; reject: (error: any) => void }> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach(prom => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token as string);
    }
  });
  failedQueue = [];
};

/** Clears the token and redirects to login on 401, trying to refresh first. */
httpClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config;
    
    // If it's a 401 and we haven't already retried this request
    if (error.response?.status === 401 && originalRequest && !(originalRequest as any)._retry) {
      // Don't intercept refresh requests to avoid infinite loops
      if (originalRequest.url?.includes('/api/identity/refresh') || originalRequest.url?.includes('/api/identity/login')) {
        return Promise.reject(error);
      }

      (originalRequest as any)._retry = true;

      const refreshToken = TokenRepository.getRefresh();
      const accessToken = TokenRepository.get();

      if (!refreshToken || !accessToken) {
        TokenRepository.clear();
        window.location.href = '/login';
        return Promise.reject(error);
      }

      if (isRefreshing) {
        return new Promise(function(resolve, reject) {
          failedQueue.push({ resolve, reject });
        }).then(token => {
          originalRequest.headers.Authorization = 'Bearer ' + token;
          return httpClient(originalRequest);
        }).catch(err => {
          return Promise.reject(err);
        });
      }

      isRefreshing = true;

      try {
        const { data } = await axios.post<{ accessToken: string; refreshToken: string }>(
          `${API_BASE_URL}/api/identity/refresh`,
          { accessToken, refreshToken }
        );

        TokenRepository.save(data.accessToken, data.refreshToken);
        
        // Use auth store to update token state if possible, but we don't have direct access here without causing circular deps sometimes.
        // It's usually fine since the store relies on the token prop in state, or the UI will eventually catch up, but let's just update headers.
        
        httpClient.defaults.headers.common['Authorization'] = 'Bearer ' + data.accessToken;
        originalRequest.headers.Authorization = 'Bearer ' + data.accessToken;
        
        processQueue(null, data.accessToken);
        
        return httpClient(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        TokenRepository.clear();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);
