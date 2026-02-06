import { renderHook } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useAuthFetch } from './useAuthFetch';
import { useAuth } from '../contexts/AuthContext';

// Mock useAuth
const mockLogout = vi.fn();
const mockToken = 'fake-token';

vi.mock('../contexts/AuthContext', () => ({
    useAuth: () => ({
        token: mockToken,
        logout: mockLogout
    })
}));

// Mock global fetch
global.fetch = vi.fn();

describe('useAuthFetch', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('adds authorization header with token', async () => {
        (global.fetch as any).mockResolvedValue({
            ok: true,
            status: 200,
            json: async () => ({})
        });

        const { result } = renderHook(() => useAuthFetch());
        const authFetch = result.current;

        await authFetch('/api/test');

        expect(global.fetch).toHaveBeenCalledWith('/api/test', expect.objectContaining({
            headers: expect.objectContaining({
                'Authorization': `Bearer ${mockToken}`
            })
        }));
    });

    it('calls logout on 401 response', async () => {
        (global.fetch as any).mockResolvedValue({
            ok: false,
            status: 401,
            json: async () => ({})
        });

        const { result } = renderHook(() => useAuthFetch());
        const authFetch = result.current;

        try {
            await authFetch('/api/protected');
        } catch (e) {
            // Error expected
        }

        expect(mockLogout).toHaveBeenCalled();
    });

    it('passes other options correctly', async () => {
        (global.fetch as any).mockResolvedValue({ ok: true });

        const { result } = renderHook(() => useAuthFetch());
        const authFetch = result.current;

        await authFetch('/api/post', {
            method: 'POST',
            body: JSON.stringify({ data: 'test' })
        });

        expect(global.fetch).toHaveBeenCalledWith('/api/post', expect.objectContaining({
            method: 'POST',
            body: '{"data":"test"}'
        }));
    });
});
