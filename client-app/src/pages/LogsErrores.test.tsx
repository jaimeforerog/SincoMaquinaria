import { render, screen, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import LogsErrores from './LogsErrores';

// Mock fetch
global.fetch = vi.fn();

const mockLogs = [
    {
        id: '1',
        fecha: '2024-01-15T10:30:00Z',
        path: '/api/ordenes',
        message: 'Error de conexión',
        stackTrace: 'at OrderService.Create()'
    },
    {
        id: '2',
        fecha: '2024-01-16T14:00:00Z',
        path: '/api/equipos',
        message: 'Equipo no encontrado',
        stackTrace: null
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

        expect(global.fetch).toHaveBeenCalledWith('/admin/logs');
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

        expect(screen.getByText('No hay errores registrados.')).toBeInTheDocument();
    });

    it('renders table headers', async () => {
        await act(async () => {
            render(<LogsErrores />);
        });

        expect(screen.getByText('Fecha')).toBeInTheDocument();
        expect(screen.getByText('Ruta')).toBeInTheDocument();
        expect(screen.getByText('Mensaje')).toBeInTheDocument();
    });
});
