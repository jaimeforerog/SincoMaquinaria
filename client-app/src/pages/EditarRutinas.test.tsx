import { render, screen, waitFor, act, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import EditarRutinas from './EditarRutinas';

const mockAuthFetch = vi.fn();
vi.mock('../hooks/useAuthFetch', () => ({
    useAuthFetch: () => mockAuthFetch
}));

const createTestQueryClient = () => new QueryClient({
    defaultOptions: { queries: { retry: false } },
});

const renderWithProviders = () => {
    const queryClient = createTestQueryClient();
    return render(
        <QueryClientProvider client={queryClient}>
            <EditarRutinas />
        </QueryClientProvider>
    );
};

const mockGrupos = [
    { codigo: 'GRP1', nombre: 'Excavadoras', activo: true },
    { codigo: 'GRP2', nombre: 'Retroexcavadoras', activo: true },
    { codigo: 'GRP3', nombre: 'Inactivo', activo: false }
];
const mockMedidores = [
    { codigo: 'MED1', nombre: 'Horometro', unidad: 'HR', activo: true },
    { codigo: 'MED2', nombre: 'Inactivo', unidad: 'X', activo: false }
];
const mockRutinaDetail1 = {
    id: 'rut-1', descripcion: 'Rutina 500hrs', grupo: 'Excavadoras',
    partes: [{
        id: 'p1', descripcion: 'Motor',
        actividades: [{
            id: 'a1', descripcion: 'Cambio de aceite', clase: 'Preventivo',
            frecuencia: 500, unidadMedida: 'HR', nombreMedidor: 'Horometro',
            alertaFaltando: 50, frecuencia2: 0, unidadMedida2: '', nombreMedidor2: '',
            alertaFaltando2: 0, insumo: 'Aceite 15W40', cantidad: 10
        }]
    }]
};
const mockRutinaDetail2 = { id: 'rut-2', descripcion: 'Rutina 1000hrs', grupo: 'Retroexcavadoras', partes: [] };

describe('EditarRutinas', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        mockAuthFetch.mockImplementation((url: string) => {
            if (url === '/configuracion/grupos') return Promise.resolve({ ok: true, json: async () => mockGrupos });
            if (url === '/configuracion/medidores') return Promise.resolve({ ok: true, json: async () => mockMedidores });
            if (url === '/rutinas/con-detalles?pageSize=1000') return Promise.resolve({ ok: true, json: async () => ({ data: [mockRutinaDetail1, mockRutinaDetail2] }) });
            return Promise.resolve({ ok: true, json: async () => ({}) });
        });
    });

    it('renders page title "Editar Rutinas de Mantenimiento"', async () => {
        await act(async () => { renderWithProviders(); });

        await waitFor(() => {
            expect(screen.getByText('Editar Rutinas de Mantenimiento')).toBeInTheDocument();
        });
    });

    it('renders subtitle "Gestione las rutinas de mantenimiento"', async () => {
        await act(async () => { renderWithProviders(); });

        await waitFor(() => {
            expect(screen.getByText('Gestione las rutinas de mantenimiento')).toBeInTheDocument();
        });
    });

    it('renders Nueva Rutina button', async () => {
        await act(async () => { renderWithProviders(); });

        await waitFor(() => {
            expect(screen.getByText('Nueva Rutina')).toBeInTheDocument();
        });
    });

    it('fetches rutinas, grupos, medidores on mount', async () => {
        await act(async () => { renderWithProviders(); });

        await waitFor(() => {
            expect(mockAuthFetch).toHaveBeenCalledWith('/rutinas/con-detalles?pageSize=1000');
            expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/grupos');
            expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/medidores');
        });
    });

    it('displays rutina names in accordions', async () => {
        await act(async () => { renderWithProviders(); });

        await waitFor(() => {
            expect(screen.getByText('Rutina 500hrs')).toBeInTheDocument();
            expect(screen.getByText('Rutina 1000hrs')).toBeInTheDocument();
        });
    });

    it('displays grupo chips', async () => {
        await act(async () => { renderWithProviders(); });

        await waitFor(() => {
            expect(screen.getByText('Excavadoras')).toBeInTheDocument();
            expect(screen.getByText('Retroexcavadoras')).toBeInTheDocument();
        });
    });

    it('shows empty state when no rutinas', async () => {
        mockAuthFetch.mockImplementation((url: string) => {
            if (url === '/configuracion/grupos') return Promise.resolve({ ok: true, json: async () => mockGrupos });
            if (url === '/configuracion/medidores') return Promise.resolve({ ok: true, json: async () => mockMedidores });
            if (url === '/rutinas/con-detalles?pageSize=1000') return Promise.resolve({ ok: true, json: async () => ({ data: [] }) });
            return Promise.resolve({ ok: true, json: async () => ({}) });
        });

        await act(async () => { renderWithProviders(); });

        await waitFor(() => {
            expect(screen.getByText(/No hay rutinas para editar/)).toBeInTheDocument();
        });
    });

    it('shows error when fetch fails', async () => {
        mockAuthFetch.mockImplementation((url: string) => {
            if (url === '/configuracion/grupos') return Promise.resolve({ ok: true, json: async () => mockGrupos });
            if (url === '/configuracion/medidores') return Promise.resolve({ ok: true, json: async () => mockMedidores });
            if (url === '/rutinas/con-detalles?pageSize=1000') return Promise.resolve({ ok: false, status: 500, statusText: 'Server Error' });
            return Promise.resolve({ ok: true, json: async () => ({}) });
        });

        await act(async () => { renderWithProviders(); });

        await waitFor(() => {
            expect(screen.getByText(/Error 500/)).toBeInTheDocument();
        });
    });

    it('shows connection error on network throw', async () => {
        mockAuthFetch.mockImplementation((url: string) => {
            if (url === '/configuracion/grupos') return Promise.resolve({ ok: true, json: async () => mockGrupos });
            if (url === '/configuracion/medidores') return Promise.resolve({ ok: true, json: async () => mockMedidores });
            if (url === '/rutinas/con-detalles?pageSize=1000') return Promise.reject(new Error('Network error'));
            return Promise.resolve({ ok: true, json: async () => ({}) });
        });

        await act(async () => { renderWithProviders(); });

        await waitFor(() => {
            expect(screen.getByText('Network error')).toBeInTheDocument();
        });
    });

    it('opens create dialog', async () => {
        await act(async () => { renderWithProviders(); });

        await waitFor(() => { expect(screen.getByText('Rutina 500hrs')).toBeInTheDocument(); });

        fireEvent.click(screen.getByText('Nueva Rutina'));

        await waitFor(() => {
            expect(screen.getByText('Crear Nueva Rutina')).toBeInTheDocument();
        });
    });

    it('shows parte when accordion expanded', async () => {
        await act(async () => { renderWithProviders(); });

        await waitFor(() => { expect(screen.getByText('Rutina 500hrs')).toBeInTheDocument(); });

        // Click the accordion to expand
        fireEvent.click(screen.getByText('Rutina 500hrs'));

        await waitFor(() => {
            expect(screen.getByText('Motor')).toBeInTheDocument();
            const buttons = screen.getAllByText('Agregar Parte');
            expect(buttons.length).toBeGreaterThanOrEqual(1);
        });
    });

    it('shows actividad details inside parte', async () => {
        await act(async () => { renderWithProviders(); });

        await waitFor(() => { expect(screen.getByText('Rutina 500hrs')).toBeInTheDocument(); });

        fireEvent.click(screen.getByText('Rutina 500hrs'));

        await waitFor(() => {
            expect(screen.getByText('Cambio de aceite')).toBeInTheDocument();
            expect(screen.getByText('Preventivo')).toBeInTheDocument();
        });
    });

    it('shows no partes message', async () => {
        await act(async () => { renderWithProviders(); });

        await waitFor(() => { expect(screen.getByText('Rutina 1000hrs')).toBeInTheDocument(); });

        fireEvent.click(screen.getByText('Rutina 1000hrs'));

        await waitFor(() => {
            expect(screen.getByText(/No hay partes definidas para esta rutina/)).toBeInTheDocument();
        });
    });
});
