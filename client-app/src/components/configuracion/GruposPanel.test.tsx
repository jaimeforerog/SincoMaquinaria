import { render, screen, waitFor, fireEvent, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import GruposPanel from './GruposPanel';
import { useAuthFetch } from '../../hooks/useAuthFetch';

vi.mock('../../hooks/useAuthFetch');
const mockAuthFetch = vi.fn();

describe('GruposPanel', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        (useAuthFetch as any).mockReturnValue(mockAuthFetch);
    });

    const mockGrupos = [
        { codigo: 'G1', nombre: 'Grupo 1', descripcion: 'Desc 1', activo: true },
        { codigo: 'G2', nombre: 'Grupo 2', descripcion: '', activo: false }
    ];

    it('renders and fetches groups on mount', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockGrupos });
        render(<GruposPanel />);

        expect(screen.getByText('Nuevo Grupo')).toBeInTheDocument();
        await waitFor(() => { expect(screen.getByText('Grupo 1')).toBeInTheDocument(); });
        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/grupos');
        expect(screen.getByText('Desc 1')).toBeInTheDocument();
    });

    it('shows empty state when no groups', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => [] });
        await act(async () => { render(<GruposPanel />); });

        await waitFor(() => {
            expect(screen.getByText('No hay grupos creados aún.')).toBeInTheDocument();
        });
    });

    it('shows Sin descripción for empty description', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockGrupos });
        await act(async () => { render(<GruposPanel />); });

        await waitFor(() => {
            expect(screen.getByText('Sin descripción')).toBeInTheDocument();
        });
    });

    it('shows error on fetch failure', async () => {
        mockAuthFetch.mockRejectedValue(new Error('Network error'));
        await act(async () => { render(<GruposPanel />); });

        await waitFor(() => {
            expect(screen.getByText('Error al cargar grupos')).toBeInTheDocument();
        });
    });

    it('creates a new group', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => [] });
        await act(async () => { render(<GruposPanel />); });

        fireEvent.change(screen.getByLabelText(/Nombre del Grupo/i), { target: { value: 'Nuevo' } });
        fireEvent.change(screen.getByLabelText(/Descripción/i), { target: { value: 'Desc' } });

        await act(async () => {
            fireEvent.click(screen.getByText('Crear Grupo'));
        });

        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/grupos', expect.objectContaining({
            method: 'POST',
            body: JSON.stringify({ nombre: 'Nuevo', descripcion: 'Desc' })
        }));
    });

    it('shows validation error when nombre empty on create', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => [] });
        await act(async () => { render(<GruposPanel />); });

        await act(async () => {
            fireEvent.click(screen.getByText('Crear Grupo'));
        });

        expect(screen.getByText('Nombre obligatorio')).toBeInTheDocument();
    });

    it('enters edit mode on edit button click', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockGrupos });
        await act(async () => { render(<GruposPanel />); });

        await waitFor(() => { expect(screen.getByText('Grupo 1')).toBeInTheDocument(); });

        const editButtons = screen.getAllByTestId('EditIcon');
        fireEvent.click(editButtons[0]);

        await waitFor(() => {
            expect(screen.getByDisplayValue('Grupo 1')).toBeInTheDocument();
        });
    });

    it('cancels editing', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockGrupos });
        await act(async () => { render(<GruposPanel />); });

        await waitFor(() => { expect(screen.getByText('Grupo 1')).toBeInTheDocument(); });

        const editButtons = screen.getAllByTestId('EditIcon');
        fireEvent.click(editButtons[0]);

        await waitFor(() => { expect(screen.getByDisplayValue('Grupo 1')).toBeInTheDocument(); });

        fireEvent.click(screen.getByTestId('CancelIcon'));

        await waitFor(() => {
            expect(screen.queryByDisplayValue('Grupo 1')).toBeNull();
        });
    });

    it('saves edit', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockGrupos });
        await act(async () => { render(<GruposPanel />); });

        await waitFor(() => { expect(screen.getByText('Grupo 1')).toBeInTheDocument(); });

        const editButtons = screen.getAllByTestId('EditIcon');
        fireEvent.click(editButtons[0]);

        await waitFor(() => { expect(screen.getByDisplayValue('Grupo 1')).toBeInTheDocument(); });

        fireEvent.change(screen.getByDisplayValue('Grupo 1'), { target: { value: 'Grupo Editado' } });

        await act(async () => {
            fireEvent.click(screen.getByTestId('SaveIcon'));
        });

        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/grupos/G1', expect.objectContaining({
            method: 'PUT'
        }));
    });

    it('toggles estado', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockGrupos });
        const { container } = await act(async () => render(<GruposPanel />));

        await waitFor(() => { expect(screen.getByText('Grupo 1')).toBeInTheDocument(); });

        const switches = container.querySelectorAll('input[type="checkbox"]');
        await act(async () => {
            fireEvent.click(switches[0]);
        });

        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/grupos/G1/estado', expect.objectContaining({
            method: 'PUT',
            body: JSON.stringify({ activo: false })
        }));
    });
});
