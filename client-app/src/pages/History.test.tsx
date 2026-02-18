import { render, screen, waitFor, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { BrowserRouter } from 'react-router-dom';
import History from './History';

// Mock useAuthFetch
const mockAuthFetch = vi.fn();
vi.mock('../hooks/useAuthFetch', () => ({
    useAuthFetch: () => mockAuthFetch
}));

const mockOrdenes = [
    {
        id: '1',
        numero: 'OT-2024-001',
        equipoId: 'eq-1',
        tipo: 'Correctivo',
        estado: 'Borrador',
        fechaCreacion: '2024-01-15T10:30:00Z'
    },
    {
        id: '2',
        numero: 'OT-2024-002',
        equipoId: 'eq-2',
        tipo: 'Preventivo',
        estado: 'Finalizada',
        fechaCreacion: '2024-01-16T14:00:00Z'
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

describe('History Component', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders the title', async () => {
        mockAuthFetch.mockResolvedValue({
            ok: true,
            json: async () => []
        });

        await act(async () => {
            renderWithRouter(<History />);
        });

        expect(screen.getByText('Historial de Mantenimiento')).toBeInTheDocument();
    });

    it('renders subtitle', async () => {
        mockAuthFetch.mockResolvedValue({
            ok: true,
            json: async () => []
        });

        await act(async () => {
            renderWithRouter(<History />);
        });

        expect(screen.getByText('Registro completo de órdenes de trabajo')).toBeInTheDocument();
    });

    it('renders Nueva Orden button', async () => {
        mockAuthFetch.mockResolvedValue({
            ok: true,
            json: async () => []
        });

        await act(async () => {
            renderWithRouter(<History />);
        });

        expect(screen.getByText('Nueva Orden')).toBeInTheDocument();
    });

    it('fetches ordenes and equipos on mount', async () => {
        mockAuthFetch
            .mockResolvedValueOnce({ ok: true, json: async () => ({ data: mockEquipos }) })
            .mockResolvedValueOnce({ ok: true, json: async () => ({ data: mockOrdenes, totalCount: 2 }) });

        await act(async () => {
            renderWithRouter(<History />);
        });

        expect(mockAuthFetch).toHaveBeenCalledWith('/equipos?PageSize=200');
        expect(mockAuthFetch).toHaveBeenCalledWith('/ordenes?Page=1&PageSize=20');
    });

    it('renders empty state when no orders', async () => {
        mockAuthFetch.mockResolvedValue({
            ok: true,
            json: async () => []
        });

        await act(async () => {
            renderWithRouter(<History />);
        });

        await waitFor(() => {
            expect(screen.getByText('No hay historial disponible.')).toBeInTheDocument();
        });
    });

    it('renders orders when data is loaded', async () => {
        mockAuthFetch
            .mockResolvedValueOnce({ ok: true, json: async () => ({ data: mockEquipos }) })
            .mockResolvedValueOnce({ ok: true, json: async () => ({ data: mockOrdenes, totalCount: 2 }) });

        await act(async () => {
            renderWithRouter(<History />);
        });

        await waitFor(() => {
            expect(screen.getByText('OT-2024-001')).toBeInTheDocument();
            expect(screen.getByText('OT-2024-002')).toBeInTheDocument();
        });
    });

    it('displays equipment placa', async () => {
        mockAuthFetch
            .mockResolvedValueOnce({ ok: true, json: async () => ({ data: mockEquipos }) })
            .mockResolvedValueOnce({ ok: true, json: async () => ({ data: mockOrdenes, totalCount: 2 }) });

        await act(async () => {
            renderWithRouter(<History />);
        });

        await waitFor(() => {
            expect(screen.getByText('ABC-123')).toBeInTheDocument();
        });
    });

    it('displays order type', async () => {
        mockAuthFetch
            .mockResolvedValueOnce({ ok: true, json: async () => ({ data: mockEquipos }) })
            .mockResolvedValueOnce({ ok: true, json: async () => ({ data: mockOrdenes, totalCount: 2 }) });

        await act(async () => {
            renderWithRouter(<History />);
        });

        await waitFor(() => {
            expect(screen.getByText('Correctivo')).toBeInTheDocument();
            expect(screen.getByText('Preventivo')).toBeInTheDocument();
        });
    });

    it('displays order status chips', async () => {
        mockAuthFetch
            .mockResolvedValueOnce({ ok: true, json: async () => ({ data: mockEquipos }) })
            .mockResolvedValueOnce({ ok: true, json: async () => ({ data: mockOrdenes, totalCount: 2 }) });

        await act(async () => {
            renderWithRouter(<History />);
        });

        await waitFor(() => {
            expect(screen.getByText('Borrador')).toBeInTheDocument();
            expect(screen.getByText('Finalizada')).toBeInTheDocument();
        });
    });

    it('renders table headers', async () => {
        mockAuthFetch.mockResolvedValue({
            ok: true,
            json: async () => ({ data: mockOrdenes })
        });

        await act(async () => {
            renderWithRouter(<History />);
        });

        expect(screen.getByText('Fecha')).toBeInTheDocument();
        expect(screen.getByText('Número')).toBeInTheDocument();
        expect(screen.getByText('Equipo')).toBeInTheDocument();
        expect(screen.getByText('Tipo')).toBeInTheDocument();
        expect(screen.getByText('Estado')).toBeInTheDocument();
    });

    it('renders Ver Detalle links', async () => {
        mockAuthFetch
            .mockResolvedValueOnce({ ok: true, json: async () => ({ data: mockEquipos }) })
            .mockResolvedValueOnce({ ok: true, json: async () => ({ data: mockOrdenes, totalCount: 2 }) });

        await act(async () => {
            renderWithRouter(<History />);
        });

        await waitFor(() => {
            const links = screen.getAllByText('Ver Detalle');
            expect(links.length).toBe(2);
        });
    });
});
