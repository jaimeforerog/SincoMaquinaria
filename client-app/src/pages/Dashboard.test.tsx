import { render, screen, waitFor, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { BrowserRouter } from 'react-router-dom';
import Dashboard from './Dashboard';

// Mock fetch
global.fetch = vi.fn();

const mockOrdenes = [
    {
        id: '1',
        numero: 'OT-2024-001',
        equipoId: 'eq-1',
        tipo: 'Correctivo',
        estado: 'Borrador',
        fechaCreacion: '2024-01-15T10:30:00Z',
        porcentajeAvanceGeneral: 25
    },
    {
        id: '2',
        numero: 'OT-2024-002',
        equipoId: 'eq-2',
        tipo: 'Preventivo',
        estado: 'Finalizada',
        fechaCreacion: '2024-01-16T14:00:00Z',
        porcentajeAvanceGeneral: 100
    }
];

const mockEquipos = [
    { id: 'eq-1', placa: 'ABC-123', descripcion: 'Excavadora CAT' },
    { id: 'eq-2', placa: 'DEF-456', descripcion: 'Retroexcavadora JD' }
];

const renderWithRouter = (component: React.ReactNode) => {
    return render(
        <BrowserRouter>
            {component}
        </BrowserRouter>
    );
};

describe('Dashboard Component', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        (global.fetch as any)
            .mockResolvedValueOnce({ ok: true, json: async () => mockOrdenes })
            .mockResolvedValueOnce({ ok: true, json: async () => mockEquipos });
    });

    it('renders dashboard title', async () => {
        await act(async () => {
            renderWithRouter(<Dashboard />);
        });

        expect(screen.getByText('Dashboard')).toBeInTheDocument();
    });

    it('fetches ordenes and equipos on mount', async () => {
        await act(async () => {
            renderWithRouter(<Dashboard />);
        });

        expect(global.fetch).toHaveBeenCalledWith('/ordenes');
        expect(global.fetch).toHaveBeenCalledWith('/equipos');
    });

    it('displays KPI cards', async () => {
        await act(async () => {
            renderWithRouter(<Dashboard />);
        });

        await waitFor(() => {
            expect(screen.getByText('Órdenes Activas')).toBeInTheDocument();
        });
    });

    it('displays total orders count', async () => {
        await act(async () => {
            renderWithRouter(<Dashboard />);
        });

        await waitFor(() => {
            expect(screen.getByText('Total Órdenes')).toBeInTheDocument();
        });
    });

    it('displays equipment count', async () => {
        await act(async () => {
            renderWithRouter(<Dashboard />);
        });

        await waitFor(() => {
            expect(screen.getByText('Equipos Registrados')).toBeInTheDocument();
        });
    });

    it('renders recent orders section', async () => {
        await act(async () => {
            renderWithRouter(<Dashboard />);
        });

        await waitFor(() => {
            expect(screen.getByText('Órdenes Recientes')).toBeInTheDocument();
        });
    });

    it('displays order numbers in recent list', async () => {
        await act(async () => {
            renderWithRouter(<Dashboard />);
        });

        await waitFor(() => {
            expect(screen.getByText('OT-2024-001')).toBeInTheDocument();
        });
    });

    it('renders Ver Historial link', async () => {
        await act(async () => {
            renderWithRouter(<Dashboard />);
        });

        await waitFor(() => {
            expect(screen.getByText('Ver Historial Completo')).toBeInTheDocument();
        });
    });

    it('renders Nueva Orden link', async () => {
        await act(async () => {
            renderWithRouter(<Dashboard />);
        });

        await waitFor(() => {
            expect(screen.getByText('Nueva Orden')).toBeInTheDocument();
        });
    });
});
