import { useQuery, useMutation, useQueryClient, UseQueryOptions } from '@tanstack/react-query';
import { useAuthFetch } from './useAuthFetch';
import { useCallback } from 'react';

/**
 * Generic hook for GET requests using React Query.
 * Replaces the repeated useState/useEffect/authFetch pattern.
 */
export function useApiQuery<T>(
    queryKey: string[],
    url: string,
    options?: Omit<UseQueryOptions<T, Error>, 'queryKey'>
) {
    const authFetch = useAuthFetch();

    const queryFn = useCallback(async (): Promise<T> => {
        const res = await authFetch(url);
        if (!res.ok) {
            throw new Error(`Error ${res.status}: ${res.statusText}`);
        }
        return res.json();
    }, [authFetch, url]);

    return useQuery<T, Error>({
        queryKey,
        queryFn,
        ...options,
    });
}

/**
 * Generic hook for POST/PUT/DELETE requests using React Query mutations.
 * Automatically invalidates related query keys on success.
 */
export function useApiMutation<TData = unknown, TVariables = unknown>(
    url: string,
    options?: {
        method?: 'POST' | 'PUT' | 'DELETE' | 'PATCH';
        invalidateKeys?: string[][];
        onSuccess?: (data: TData) => void;
        onError?: (error: Error) => void;
    }
) {
    const authFetch = useAuthFetch();
    const queryClient = useQueryClient();
    const method = options?.method ?? 'POST';

    return useMutation<TData, Error, TVariables>({
        mutationFn: async (variables: TVariables) => {
            const res = await authFetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(variables),
            });
            if (!res.ok) {
                const errorData = await res.json().catch(() => ({}));
                throw new Error(errorData.error || errorData.message || `Error ${res.status}`);
            }
            // Some endpoints return empty responses
            const text = await res.text();
            return text ? JSON.parse(text) : ({} as TData);
        },
        onSuccess: (data) => {
            // Invalidate related queries to trigger refetch
            if (options?.invalidateKeys) {
                options.invalidateKeys.forEach(key => {
                    queryClient.invalidateQueries({ queryKey: key });
                });
            }
            options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}

/**
 * Hook for dynamic URL mutations (e.g., PUT /equipos/:id).
 * The URL is computed from the variables.
 */
export function useApiDynamicMutation<TData = unknown>(
    options?: {
        method?: 'POST' | 'PUT' | 'DELETE' | 'PATCH';
        invalidateKeys?: string[][];
        onSuccess?: (data: TData) => void;
        onError?: (error: Error) => void;
    }
) {
    const authFetch = useAuthFetch();
    const queryClient = useQueryClient();
    const method = options?.method ?? 'POST';

    return useMutation<TData, Error, { url: string; body?: unknown }>({
        mutationFn: async ({ url, body }) => {
            const fetchOptions: RequestInit = { method };
            if (body !== undefined) {
                fetchOptions.headers = { 'Content-Type': 'application/json' };
                fetchOptions.body = JSON.stringify(body);
            }
            const res = await authFetch(url, fetchOptions);
            if (!res.ok) {
                const errorData = await res.json().catch(() => ({}));
                throw new Error(errorData.error || errorData.message || `Error ${res.status}`);
            }
            const text = await res.text();
            return text ? JSON.parse(text) : ({} as TData);
        },
        onSuccess: (data) => {
            if (options?.invalidateKeys) {
                options.invalidateKeys.forEach(key => {
                    queryClient.invalidateQueries({ queryKey: key });
                });
            }
            options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}
