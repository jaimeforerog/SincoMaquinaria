import { useCallback } from 'react';
import { useAuth } from '../contexts/AuthContext';

export const useAuthFetch = () => {
  const { token, logout } = useAuth();

  const authFetch = useCallback(async (url: string, options: RequestInit = {}) => {
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
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(url, {
      ...options,
      headers,
    });

    // If 401 Unauthorized, logout user
    if (response.status === 401) {
      logout();
      throw new Error('Sesión expirada. Por favor inicia sesión nuevamente.');
    }

    return response;
  }, [token, logout]);

  return authFetch;
};
