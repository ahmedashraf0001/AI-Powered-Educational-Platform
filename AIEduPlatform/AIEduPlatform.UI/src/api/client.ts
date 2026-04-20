import axios from 'axios';
import { useAuthStore } from '@/stores/authStore';

const LOCALHOST_PATTERN = /^(https?:\/\/)?(localhost|127\.0\.0\.1)(:\d+)?(\/|$)/i;
const configuredApiUrl = (import.meta.env.VITE_API_URL ?? '').trim().replace(/\/+$/, '');
const API_URL =
  import.meta.env.PROD && LOCALHOST_PATTERN.test(configuredApiUrl)
    ? '/api'
    : configuredApiUrl || '/api';

const client = axios.create({
  baseURL: API_URL,
  headers: { 'Content-Type': 'application/json' },
});

// Request interceptor — attach JWT token
client.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor — handle 401 + refresh
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (err: unknown) => void;
}> = [];

function processQueue(error: unknown, token: string | null) {
  failedQueue.forEach((p) => {
    if (error) p.reject(error);
    else if (token) p.resolve(token);
  });
  failedQueue = [];
}

function attachUserMessage(error: any) {
  const data = error?.response?.data;
  const validationErrors = data?.error?.errors || data?.errors;

  const firstValidationMessage =
    validationErrors && typeof validationErrors === 'object' && !Array.isArray(validationErrors)
      ? Object.values(validationErrors as Record<string, unknown[] | unknown>)
          .flatMap((entry) => (Array.isArray(entry) ? entry : [entry]))
          .find((entry) => typeof entry === 'string')
      : null;

  const normalizedErrors = Array.isArray(data?.errors)
    ? data.errors
    : Array.isArray(data?.errors?.errors)
      ? data.errors.errors
      : null;

  const message =
    (typeof firstValidationMessage === 'string' ? firstValidationMessage : null) ||
    normalizedErrors?.[0]?.message ||
    normalizedErrors?.[0] ||
    data?.message ||
    data?.detail ||
    data?.title ||
    error?.message ||
    'An unexpected error occurred.';

  error.userMessage = message;
  return error;
}

client.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      const { accessToken, refreshToken, setTokens, logout } = useAuthStore.getState();
      if (!refreshToken) {
        logout();
        window.location.href = '/login';
        return Promise.reject(error);
      }

      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({
            resolve: (token: string) => {
              originalRequest.headers.Authorization = `Bearer ${token}`;
              resolve(client(originalRequest));
            },
            reject,
          });
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const { data } = await axios.post(`${API_URL}/auth/refresh-token`, {
          accessToken,
          refreshToken,
        });

        const tokens = data.data;
        setTokens(tokens);
        processQueue(null, tokens.accessToken);
        originalRequest.headers.Authorization = `Bearer ${tokens.accessToken}`;
        return client(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        logout();
        window.location.href = '/login';
        return Promise.reject(attachUserMessage(refreshError));
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(attachUserMessage(error));
  }
);

export default client;
