import { render, screen, waitFor, act, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { AuthProvider, useAuth } from './AuthContext';

const AuthConsumer = () => {
    const { user, token, isAuthenticated, isLoading, login, logout } = useAuth();
    return (
        <div>
            <span data-testid="isLoading">{String(isLoading)}</span>
            <span data-testid="isAuthenticated">{String(isAuthenticated)}</span>
            <span data-testid="user">{user ? user.nombre : 'null'}</span>
            <span data-testid="token">{token || 'null'}</span>
            <button data-testid="login-btn" onClick={() => login('test@test.com', 'pass123').catch(() => {})}>Login</button>
            <button data-testid="logout-btn" onClick={() => logout()}>Logout</button>
        </div>
    );
};

const mockUser = { id: 'u1', email: 'test@test.com', nombre: 'Test User', rol: 'Admin' };

describe('AuthContext', () => {
    beforeEach(() => {
        localStorage.clear();
        vi.restoreAllMocks();
    });

    afterEach(() => {
        localStorage.clear();
    });

    it('throws error outside AuthProvider', () => {
        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
        expect(() => render(<AuthConsumer />)).toThrow('useAuth must be used within an AuthProvider');
        consoleSpy.mockRestore();
    });

    it('no user when localStorage empty', async () => {
        render(<AuthProvider><AuthConsumer /></AuthProvider>);

        await waitFor(() => {
            expect(screen.getByTestId('isLoading').textContent).toBe('false');
        });

        expect(screen.getByTestId('isAuthenticated').textContent).toBe('false');
        expect(screen.getByTestId('user').textContent).toBe('null');
    });

    it('restores user from localStorage on mount', async () => {
        localStorage.setItem('authToken', 'stored-token');
        localStorage.setItem('refreshToken', 'stored-refresh');
        localStorage.setItem('authUser', JSON.stringify(mockUser));

        global.fetch = vi.fn().mockResolvedValue({
            ok: true,
            status: 200,
            json: async () => mockUser,
        });

        render(<AuthProvider><AuthConsumer /></AuthProvider>);

        await waitFor(() => {
            expect(screen.getByTestId('isLoading').textContent).toBe('false');
        });

        expect(screen.getByTestId('isAuthenticated').textContent).toBe('true');
        expect(screen.getByTestId('user').textContent).toBe('Test User');
    });

    it('login stores tokens in localStorage', async () => {
        const loginResponse = {
            id: 'u1', email: 'test@test.com', nombre: 'Test User', rol: 'Admin',
            token: 'new-token', refreshToken: 'new-refresh'
        };

        global.fetch = vi.fn().mockResolvedValue({
            ok: true,
            status: 200,
            json: async () => loginResponse,
        });

        render(<AuthProvider><AuthConsumer /></AuthProvider>);

        await waitFor(() => {
            expect(screen.getByTestId('isLoading').textContent).toBe('false');
        });

        await act(async () => {
            fireEvent.click(screen.getByTestId('login-btn'));
        });

        await waitFor(() => {
            expect(localStorage.getItem('authToken')).toBe('new-token');
            expect(localStorage.getItem('refreshToken')).toBe('new-refresh');
            expect(screen.getByTestId('isAuthenticated').textContent).toBe('true');
        });
    });

    it('login calls POST /auth/login', async () => {
        global.fetch = vi.fn().mockResolvedValue({
            ok: true,
            status: 200,
            json: async () => ({
                id: 'u1', email: 'test@test.com', nombre: 'Test User', rol: 'Admin',
                token: 'tk', refreshToken: 'rt'
            }),
        });

        render(<AuthProvider><AuthConsumer /></AuthProvider>);

        await waitFor(() => {
            expect(screen.getByTestId('isLoading').textContent).toBe('false');
        });

        await act(async () => {
            fireEvent.click(screen.getByTestId('login-btn'));
        });

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith('/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email: 'test@test.com', password: 'pass123' }),
            });
        });
    });

    it('login throws on non-ok response', async () => {
        global.fetch = vi.fn().mockResolvedValue({
            ok: false,
            status: 401,
            text: async () => 'Invalid credentials',
        });

        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

        render(<AuthProvider><AuthConsumer /></AuthProvider>);

        await waitFor(() => {
            expect(screen.getByTestId('isLoading').textContent).toBe('false');
        });

        await act(async () => {
            fireEvent.click(screen.getByTestId('login-btn'));
        });

        await waitFor(() => {
            expect(screen.getByTestId('isAuthenticated').textContent).toBe('false');
        });

        consoleSpy.mockRestore();
    });

    it('logout clears localStorage', async () => {
        localStorage.setItem('authToken', 'stored-token');
        localStorage.setItem('refreshToken', 'stored-refresh');
        localStorage.setItem('authUser', JSON.stringify(mockUser));

        global.fetch = vi.fn().mockResolvedValue({
            ok: true,
            status: 200,
            json: async () => mockUser,
        });

        render(<AuthProvider><AuthConsumer /></AuthProvider>);

        await waitFor(() => {
            expect(screen.getByTestId('isAuthenticated').textContent).toBe('true');
        });

        await act(async () => {
            fireEvent.click(screen.getByTestId('logout-btn'));
        });

        await waitFor(() => {
            expect(localStorage.getItem('authToken')).toBeNull();
            expect(localStorage.getItem('refreshToken')).toBeNull();
            expect(localStorage.getItem('authUser')).toBeNull();
            expect(screen.getByTestId('isAuthenticated').textContent).toBe('false');
        });
    });

    it('clears auth when 401 and refresh fails', async () => {
        localStorage.setItem('authToken', 'expired-token');
        localStorage.setItem('refreshToken', 'expired-refresh');
        localStorage.setItem('authUser', JSON.stringify(mockUser));

        global.fetch = vi.fn()
            .mockResolvedValueOnce({ ok: false, status: 401 }) // GET /auth/me
            .mockResolvedValueOnce({ ok: false, status: 401 }); // POST /auth/refresh

        const consoleSpy = vi.spyOn(console, 'log').mockImplementation(() => {});
        const consoleWarn = vi.spyOn(console, 'warn').mockImplementation(() => {});

        render(<AuthProvider><AuthConsumer /></AuthProvider>);

        await waitFor(() => {
            expect(screen.getByTestId('isAuthenticated').textContent).toBe('false');
        }, { timeout: 3000 });

        consoleSpy.mockRestore();
        consoleWarn.mockRestore();
    });

    it('clears auth on non-401 error', async () => {
        localStorage.setItem('authToken', 'some-token');
        localStorage.setItem('refreshToken', 'some-refresh');
        localStorage.setItem('authUser', JSON.stringify(mockUser));

        global.fetch = vi.fn().mockResolvedValueOnce({ ok: false, status: 500 }); // GET /auth/me returns 500

        const consoleWarn = vi.spyOn(console, 'warn').mockImplementation(() => {});

        render(<AuthProvider><AuthConsumer /></AuthProvider>);

        await waitFor(() => {
            expect(screen.getByTestId('isAuthenticated').textContent).toBe('false');
        }, { timeout: 3000 });

        consoleWarn.mockRestore();
    });

    it('keeps session on network error', async () => {
        localStorage.setItem('authToken', 'good-token');
        localStorage.setItem('refreshToken', 'good-refresh');
        localStorage.setItem('authUser', JSON.stringify(mockUser));

        global.fetch = vi.fn().mockRejectedValueOnce(new Error('Network Error'));

        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

        render(<AuthProvider><AuthConsumer /></AuthProvider>);

        // User should still be authenticated after network error
        await waitFor(() => {
            expect(screen.getByTestId('isLoading').textContent).toBe('false');
        });

        // Wait a tick for background validation to complete
        await act(async () => {
            await new Promise(r => setTimeout(r, 100));
        });

        expect(screen.getByTestId('isAuthenticated').textContent).toBe('true');
        expect(screen.getByTestId('user').textContent).toBe('Test User');

        consoleSpy.mockRestore();
    });
});
