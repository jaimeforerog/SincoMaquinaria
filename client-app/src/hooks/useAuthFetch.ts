import { useCallback } from 'react';
import { useAuth } from '../contexts/AuthContext';

export const useAuthFetch = () => {
  const { token, logout, refreshAccessToken } = useAuth();

  const authFetch = useCallback(async (url: string, options: RequestInit = {}) => {
    const makeRequest = async (accessToken: string | null) => {
      const headers: HeadersInit = {
        ...options.headers,
      };

      if (!(options.body instanceof FormData)) {
        // Default to JSON if not FormData and not specified
        if (!(headers as any)['Content-Type']) {
          (headers as any)['Content-Type'] = 'application/json';
        }
      }

      // Add Authorization header if token exists
      if (accessToken) {
        headers['Authorization'] = `Bearer ${accessToken}`;
      }

      return fetch(url, {
        ...options,
        headers,
      });
    };

    // Make initial request
    let response = await makeRequest(token);

    // If 401 Unauthorized, try to refresh token
    if (response.status === 401) {
      console.log('Received 401, attempting token refresh...');

      const refreshed = await refreshAccessToken();

      if (refreshed) {
        // Retry request with new token
        const newToken = localStorage.getItem('authToken');
        response = await makeRequest(newToken);

        // If still 401 after refresh, logout
        if (response.status === 401) {
          await logout();
          throw new Error('Sesión expirada. Por favor inicia sesión nuevamente.');
        }
      } else {
        // Refresh failed, logout
        await logout();
        throw new Error('Sesión expirada. Por favor inicia sesión nuevamente.');
      }
    }

    return response;
  }, [token, logout, refreshAccessToken]);

  return authFetch;
};
