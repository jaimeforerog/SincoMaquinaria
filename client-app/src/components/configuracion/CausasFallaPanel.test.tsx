import { render, screen, waitFor, fireEvent, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import CausasFallaPanel from './CausasFallaPanel';
import { useAuthFetch } from '../../hooks/useAuthFetch';

vi.mock('../../hooks/useAuthFetch');
const mockAuthFetch = vi.fn();

describe('CausasFallaPanel', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        (useAuthFetch as any).mockReturnValue(mockAuthFetch);
    });

    const mockCausas = [
        { codigo: 'C1', descripcion: 'Causa 1', activo: true },
        { codigo: 'C2', descripcion: 'Causa 2', activo: false }
    ];

    it('renders and fetches causes on mount', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockCausas });
        render(<CausasFallaPanel />);

        expect(screen.getByText('Nueva Causa')).toBeInTheDocument();
        await waitFor(() => { expect(screen.getByText('Causa 1')).toBeInTheDocument(); });
        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/causas-falla');
    });

    it('shows empty state when no causes', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => [] });
        await act(async () => { render(<CausasFallaPanel />); });

        await waitFor(() => {
            expect(screen.getByText('No hay causas de falla configuradas.')).toBeInTheDocument();
        });
    });

    it('shows error on fetch failure', async () => {
        mockAuthFetch.mockRejectedValue(new Error('Network'));
        await act(async () => { render(<CausasFallaPanel />); });

        await waitFor(() => {
            expect(screen.getByText('Error al cargar causas de falla')).toBeInTheDocument();
        });
    });

    it('creates a new causa', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => [] });
        await act(async () => { render(<CausasFallaPanel />); });

        fireEvent.change(screen.getByLabelText(/Descripción de Causa/i), { target: { value: 'Nueva causa' } });

        await act(async () => {
            fireEvent.click(screen.getByText('Crear Causa'));
        });

        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/causas-falla', expect.objectContaining({
            method: 'POST',
            body: JSON.stringify({ descripcion: 'Nueva causa' })
        }));
    });

    it('shows validation when description empty on create', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => [] });
        await act(async () => { render(<CausasFallaPanel />); });

        await act(async () => {
            fireEvent.click(screen.getByText('Crear Causa'));
        });

        expect(screen.getByText('Descripción obligatoria')).toBeInTheDocument();
    });

    it('enters edit mode', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockCausas });
        await act(async () => { render(<CausasFallaPanel />); });

        await waitFor(() => { expect(screen.getByText('Causa 1')).toBeInTheDocument(); });

        const editButtons = screen.getAllByTestId('EditIcon');
        fireEvent.click(editButtons[0]);

        await waitFor(() => {
            expect(screen.getByDisplayValue('Causa 1')).toBeInTheDocument();
        });
    });

    it('cancels editing', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockCausas });
        await act(async () => { render(<CausasFallaPanel />); });

        await waitFor(() => { expect(screen.getByText('Causa 1')).toBeInTheDocument(); });

        fireEvent.click(screen.getAllByTestId('EditIcon')[0]);
        await waitFor(() => { expect(screen.getByDisplayValue('Causa 1')).toBeInTheDocument(); });

        fireEvent.click(screen.getByTestId('CancelIcon'));

        await waitFor(() => {
            expect(screen.queryByDisplayValue('Causa 1')).toBeNull();
        });
    });

    it('saves edit', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockCausas });
        await act(async () => { render(<CausasFallaPanel />); });

        await waitFor(() => { expect(screen.getByText('Causa 1')).toBeInTheDocument(); });

        fireEvent.click(screen.getAllByTestId('EditIcon')[0]);
        await waitFor(() => { expect(screen.getByDisplayValue('Causa 1')).toBeInTheDocument(); });

        fireEvent.change(screen.getByDisplayValue('Causa 1'), { target: { value: 'Edited' } });

        await act(async () => {
            fireEvent.click(screen.getByTestId('SaveIcon'));
        });

        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/causas-falla/C1', expect.objectContaining({
            method: 'PUT'
        }));
    });

    it('toggles estado', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockCausas });
        const { container } = await act(async () => render(<CausasFallaPanel />));

        await waitFor(() => { expect(screen.getByText('Causa 1')).toBeInTheDocument(); });

        const switches = container.querySelectorAll('input[type="checkbox"]');
        await act(async () => {
            fireEvent.click(switches[0]);
        });

        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/causas-falla/C1/estado', expect.objectContaining({
            method: 'PUT',
            body: JSON.stringify({ activo: false })
        }));
    });
});
