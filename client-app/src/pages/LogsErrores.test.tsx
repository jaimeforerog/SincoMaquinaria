import { render, screen, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import LogsErrores from './LogsErrores';

// Mock useAuth
vi.mock('../contexts/AuthContext', () => ({
    useAuth: vi.fn(() => ({
        token: 'fake-token',
        logout: vi.fn(),
        refreshAccessToken: vi.fn()
    }))
}));

// Mock fetch
global.fetch = vi.fn();

const mockLogs = [
    {
        id: "1",
        fecha: "2024-01-15T10:00:00Z",
        path: "/api/test",
        message: "Error de conexión",
        stackTrace: "System.Exception: Timeout"
    }
];

describe('LogsErrores Component', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        (global.fetch as any).mockResolvedValue({
            ok: true,
            json: async () => mockLogs
        });
    });

    it('renders the component title', async () => {
        await act(async () => {
            render(<LogsErrores />);
        });

        expect(screen.getByText('Logs de Errores')).toBeInTheDocument();
    });

    it('fetches logs on mount', async () => {
        await act(async () => {
            render(<LogsErrores />);
        });

        expect(global.fetch).toHaveBeenCalledWith('/admin/logs', expect.objectContaining({
            headers: expect.objectContaining({
                'Authorization': 'Bearer fake-token'
            })
        }));
    });

    it('renders Refrescar button', async () => {
        await act(async () => {
            render(<LogsErrores />);
        });

        expect(screen.getByText('Refrescar')).toBeInTheDocument();
    });

    it('shows empty state when no logs', async () => {
        (global.fetch as any).mockResolvedValue({
            ok: true,
            json: async () => []
        });

        await act(async () => {
            render(<LogsErrores />);
        });

        expect(screen.getByText(/No hay errores registrados/i)).toBeInTheDocument();
    });

    it('renders table headers', async () => {
        // Need to ensure logs are returned so headers are rendered
        (global.fetch as any).mockResolvedValue({
            ok: true,
            json: async () => mockLogs
        });

        await act(async () => {
            render(<LogsErrores />);
        });

        expect(screen.getByText('Fecha')).toBeInTheDocument();
        expect(screen.getByText('Ruta')).toBeInTheDocument();
        expect(screen.getByText('Mensaje')).toBeInTheDocument();
    });
});
