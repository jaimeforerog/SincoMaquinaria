import { render, screen, waitFor, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { BrowserRouter } from 'react-router-dom';
import Dashboard from './Dashboard';

// Mock useAuthFetch
const mockAuthFetch = vi.fn();
vi.mock('../hooks/useAuthFetch', () => ({
    useAuthFetch: () => mockAuthFetch
}));

// Mock useDashboardSocket
vi.mock('../hooks/useDashboardSocket', () => ({
    useDashboardSocket: () => { }
}));



const renderWithRouter = (component: React.ReactNode) => {
    return render(
        <BrowserRouter>
            {component}
        </BrowserRouter>
    );
};

const mockStats = {
    equiposCount: 5,
    rutinasCount: 3,
    ordenesActivasCount: 2
};

describe('Dashboard Component', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        mockAuthFetch.mockImplementation((url: string) => {
            if (url.includes('/dashboard/stats')) {
                return Promise.resolve({ ok: true, json: async () => mockStats });
            }
            return Promise.resolve({ ok: true, json: async () => ({}) });
        });
    });

    it('renders dashboard title', async () => {
        await act(async () => {
            renderWithRouter(<Dashboard />);
        });

        expect(screen.getByText('Dashboard Operativo')).toBeInTheDocument();
        expect(screen.getByText('Centro de control de Maquinaria Amarilla')).toBeInTheDocument();
    });

    it('fetches stats on mount', async () => {
        await act(async () => {
            renderWithRouter(<Dashboard />);
        });

        expect(mockAuthFetch).toHaveBeenCalledWith('/dashboard/stats');
    });

    it('displays KPI cards with correct values', async () => {
        await act(async () => {
            renderWithRouter(<Dashboard />);
        });

        await waitFor(() => {
            expect(screen.getByText('Equipos')).toBeInTheDocument();
            expect(screen.getByText('5')).toBeInTheDocument();

            expect(screen.getByText('Rutinas')).toBeInTheDocument();
            expect(screen.getByText('3')).toBeInTheDocument();

            expect(screen.getByText('Órdenes Activas')).toBeInTheDocument();
            expect(screen.getByText('2')).toBeInTheDocument();
        });
    });

    it('renders links to management pages', async () => {
        await act(async () => {
            renderWithRouter(<Dashboard />);
        });

        const equiposLink = screen.getByText('Equipos').closest('a');
        expect(equiposLink).toHaveAttribute('href', '/gestion-equipos');

        const rutinasLink = screen.getByText('Rutinas').closest('a');
        expect(rutinasLink).toHaveAttribute('href', '/editar-rutinas');

        const historialLink = screen.getByText('Órdenes Activas').closest('a');
        expect(historialLink).toHaveAttribute('href', '/historial');
    });
});
