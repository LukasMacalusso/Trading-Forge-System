import axios from 'axios';
import { API_BASE_URL } from '../../Application/Common/constants';
import { TokenRepository } from '../Repositories/TokenRepository';

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

/** Clears the token and redirects to login on 401. */
httpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      TokenRepository.clear();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
