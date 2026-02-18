import { render, screen, waitFor, fireEvent, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import MedidoresPanel from './MedidoresPanel';
import { useAuthFetch } from '../../hooks/useAuthFetch';

vi.mock('../../hooks/useAuthFetch');
const mockAuthFetch = vi.fn();

describe('MedidoresPanel', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        (useAuthFetch as any).mockReturnValue(mockAuthFetch);
    });

    const mockMedidores = [
        { codigo: 'HORAS', nombre: 'Horas Motor', unidad: 'HR', activo: true },
        { codigo: 'KM', nombre: 'Kilometraje', unidad: 'KM', activo: false }
    ];

    it('renders and fetches medidores on mount', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockMedidores });
        render(<MedidoresPanel />);

        expect(screen.getByText('Nuevo Medidor')).toBeInTheDocument();
        await waitFor(() => {
            expect(screen.getByText('Horas Motor')).toBeInTheDocument();
            expect(screen.getByText('HR')).toBeInTheDocument();
        });
        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/medidores');
    });

    it('shows empty state when no medidores', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => [] });
        await act(async () => { render(<MedidoresPanel />); });

        await waitFor(() => {
            expect(screen.getByText('No hay medidores configurados.')).toBeInTheDocument();
        });
    });

    it('shows error on fetch failure', async () => {
        mockAuthFetch.mockRejectedValue(new Error('Connection failed'));
        await act(async () => { render(<MedidoresPanel />); });

        await waitFor(() => {
            expect(screen.getByText('Connection failed')).toBeInTheDocument();
        });
    });

    it('creates a new medidor', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => [] });
        await act(async () => { render(<MedidoresPanel />); });

        fireEvent.change(screen.getByLabelText(/Nombre del Medidor/i), { target: { value: 'Galones' } });
        fireEvent.change(screen.getByLabelText(/Unidad de Medida/i), { target: { value: 'gal' } });

        await act(async () => {
            fireEvent.click(screen.getByText('Crear Medidor'));
        });

        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/medidores', expect.objectContaining({
            method: 'POST',
            body: JSON.stringify({ nombre: 'Galones', unidad: 'GAL' })
        }));
    });

    it('shows validation when fields empty on create', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => [] });
        await act(async () => { render(<MedidoresPanel />); });

        await act(async () => {
            fireEvent.click(screen.getByText('Crear Medidor'));
        });

        expect(screen.getByText('Campos obligatorios')).toBeInTheDocument();
    });

    it('enters edit mode', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockMedidores });
        await act(async () => { render(<MedidoresPanel />); });

        await waitFor(() => { expect(screen.getByText('Horas Motor')).toBeInTheDocument(); });

        const editButtons = screen.getAllByTestId('EditIcon');
        fireEvent.click(editButtons[0]);

        await waitFor(() => {
            expect(screen.getByDisplayValue('Horas Motor')).toBeInTheDocument();
            expect(screen.getByDisplayValue('HR')).toBeInTheDocument();
        });
    });

    it('cancels editing', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockMedidores });
        await act(async () => { render(<MedidoresPanel />); });

        await waitFor(() => { expect(screen.getByText('Horas Motor')).toBeInTheDocument(); });

        fireEvent.click(screen.getAllByTestId('EditIcon')[0]);
        await waitFor(() => { expect(screen.getByDisplayValue('Horas Motor')).toBeInTheDocument(); });

        fireEvent.click(screen.getByTestId('CancelIcon'));

        await waitFor(() => {
            expect(screen.queryByDisplayValue('Horas Motor')).toBeNull();
        });
    });

    it('saves edit', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockMedidores });
        await act(async () => { render(<MedidoresPanel />); });

        await waitFor(() => { expect(screen.getByText('Horas Motor')).toBeInTheDocument(); });

        fireEvent.click(screen.getAllByTestId('EditIcon')[0]);
        await waitFor(() => { expect(screen.getByDisplayValue('Horas Motor')).toBeInTheDocument(); });

        fireEvent.change(screen.getByDisplayValue('Horas Motor'), { target: { value: 'Edited' } });

        await act(async () => {
            fireEvent.click(screen.getByTestId('SaveIcon'));
        });

        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/medidores/HORAS', expect.objectContaining({
            method: 'PUT'
        }));
    });

    it('toggles estado', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockMedidores });
        const { container } = await act(async () => render(<MedidoresPanel />));

        await waitFor(() => { expect(screen.getByText('Horas Motor')).toBeInTheDocument(); });

        const switches = container.querySelectorAll('input[type="checkbox"]');
        await act(async () => {
            fireEvent.click(switches[0]);
        });

        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/medidores/HORAS/estado', expect.objectContaining({
            method: 'PUT',
            body: JSON.stringify({ activo: false })
        }));
    });
});
