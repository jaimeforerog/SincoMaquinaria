import { render, screen, waitFor, act, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import Auditoria from './Auditoria';

const mockAuthFetch = vi.fn();
vi.mock('../hooks/useAuthFetch', () => ({
    useAuthFetch: () => mockAuthFetch
}));

const mockModulos = ['Ordenes', 'Equipos', 'Empleados'];
const mockUsuarios = [{ id: 'u1', nombre: 'Admin User' }, { id: 'u2', nombre: 'Regular User' }];
const mockEventos = [{ tipo: 'OrdenDeTrabajoCreada', modulo: 'Ordenes' }];
const mockAuditEvents = [
    {
        id: 'evt-1', streamId: 'stream-001', tipo: 'OrdenDeTrabajoCreada', modulo: 'Ordenes',
        fecha: '2024-06-15T10:30:00Z', version: 1,
        datos: { usuarioNombre: 'Admin User', detalles: { numero: 'OT-001' } }
    },
    {
        id: 'evt-2', streamId: 'stream-002', tipo: 'EquipoActualizado', modulo: 'Equipos',
        fecha: '2024-06-16T14:00:00Z', version: 2,
        datos: { usuarioNombre: 'Regular User' }
    }
];
const mockPagedResponse = { data: mockAuditEvents, page: 1, pageSize: 25, totalCount: 2 };

describe('Auditoria', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        mockAuthFetch.mockImplementation((url: string) => {
            if (url.includes('/api/auditoria/modulos')) return Promise.resolve({ ok: true, json: async () => mockModulos });
            if (url.includes('/api/auditoria/usuarios')) return Promise.resolve({ ok: true, json: async () => mockUsuarios });
            if (url.includes('/api/auditoria/eventos')) return Promise.resolve({ ok: true, json: async () => mockEventos });
            if (url.includes('/api/auditoria?')) return Promise.resolve({ ok: true, json: async () => mockPagedResponse });
            return Promise.resolve({ ok: true, json: async () => [] });
        });
    });

    it('renders page title "Auditoría del Sistema"', async () => {
        await act(async () => { render(<Auditoria />); });
        expect(screen.getByText('Auditoría del Sistema')).toBeInTheDocument();
    });

    it('renders subtitle', async () => {
        await act(async () => { render(<Auditoria />); });
        expect(screen.getByText('Registro de todos los cambios realizados en la aplicación')).toBeInTheDocument();
    });

    it('renders Filtros section', async () => {
        await act(async () => { render(<Auditoria />); });
        expect(screen.getByText('Filtros')).toBeInTheDocument();
    });

    it('renders Consultar button', async () => {
        await act(async () => { render(<Auditoria />); });
        expect(screen.getByText('Consultar')).toBeInTheDocument();
    });

    it('fetches modulos and usuarios on mount', async () => {
        await act(async () => { render(<Auditoria />); });

        expect(mockAuthFetch).toHaveBeenCalledWith('/api/auditoria/modulos');
        expect(mockAuthFetch).toHaveBeenCalledWith('/api/auditoria/usuarios');
    });

    it('renders table headers', async () => {
        await act(async () => { render(<Auditoria />); });

        expect(screen.getByText('Fecha')).toBeInTheDocument();
        // "Módulo" appears in both filter label and table header
        const modulos = screen.getAllByText('Módulo');
        expect(modulos.length).toBeGreaterThanOrEqual(1);
        expect(screen.getByText('Evento')).toBeInTheDocument();
        // "Usuario" also appears in both filter label and table header
        const usuarios = screen.getAllByText('Usuario');
        expect(usuarios.length).toBeGreaterThanOrEqual(1);
        expect(screen.getByText('Versión')).toBeInTheDocument();
    });

    it('shows message before searching', async () => {
        await act(async () => { render(<Auditoria />); });

        expect(screen.getByText(/Seleccione los filtros y presione/)).toBeInTheDocument();
    });

    it('displays events count 0 initially', async () => {
        await act(async () => { render(<Auditoria />); });

        expect(screen.getByText('0')).toBeInTheDocument();
        expect(screen.getByText('Eventos Encontrados')).toBeInTheDocument();
    });

    it('displays date range card', async () => {
        await act(async () => { render(<Auditoria />); });

        expect(screen.getByText('Rango de Fechas')).toBeInTheDocument();
    });

    it('fetches events when Consultar clicked', async () => {
        await act(async () => { render(<Auditoria />); });

        await act(async () => {
            fireEvent.click(screen.getByText('Consultar'));
        });

        await waitFor(() => {
            expect(screen.getByText('Orden Creada')).toBeInTheDocument();
        });
    });

    it('displays user names in event rows', async () => {
        await act(async () => { render(<Auditoria />); });

        await act(async () => {
            fireEvent.click(screen.getByText('Consultar'));
        });

        await waitFor(() => {
            expect(screen.getByText('Admin User')).toBeInTheDocument();
            expect(screen.getByText('Regular User')).toBeInTheDocument();
        });
    });

    it('displays module chips', async () => {
        await act(async () => { render(<Auditoria />); });

        await act(async () => {
            fireEvent.click(screen.getByText('Consultar'));
        });

        await waitFor(() => {
            // Module chips in the table rows
            const ordenes = screen.getAllByText('Ordenes');
            expect(ordenes.length).toBeGreaterThanOrEqual(1);
        });
    });

    it('shows no events message', async () => {
        mockAuthFetch.mockImplementation((url: string) => {
            if (url.includes('/api/auditoria/modulos')) return Promise.resolve({ ok: true, json: async () => mockModulos });
            if (url.includes('/api/auditoria/usuarios')) return Promise.resolve({ ok: true, json: async () => mockUsuarios });
            if (url.includes('/api/auditoria?')) return Promise.resolve({ ok: true, json: async () => ({ data: [], page: 1, pageSize: 25, totalCount: 0 }) });
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        await act(async () => { render(<Auditoria />); });

        await act(async () => {
            fireEvent.click(screen.getByText('Consultar'));
        });

        await waitFor(() => {
            expect(screen.getByText('No hay eventos registrados en el rango de fechas seleccionado')).toBeInTheDocument();
        });
    });

    it('expands row to show details', async () => {
        await act(async () => { render(<Auditoria />); });

        await act(async () => {
            fireEvent.click(screen.getByText('Consultar'));
        });

        await waitFor(() => {
            expect(screen.getByText('Orden Creada')).toBeInTheDocument();
        });

        // Click the first event row to expand - click on the row's text
        const ordenCreadaText = screen.getByText('Orden Creada');
        await act(async () => {
            // Click the closest TableRow ancestor
            const row = ordenCreadaText.closest('tr');
            if (row) fireEvent.click(row);
        });

        await waitFor(() => {
            const details = screen.getAllByText('Detalles del Evento');
            expect(details.length).toBeGreaterThanOrEqual(1);
            expect(screen.getByText('stream-001')).toBeInTheDocument();
        });
    });
});
