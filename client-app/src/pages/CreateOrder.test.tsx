import { render, screen, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { BrowserRouter } from 'react-router-dom';
import CreateOrder from './CreateOrder';

// Mock useAuthFetch
const mockAuthFetch = vi.fn();
vi.mock('../hooks/useAuthFetch', () => ({
    useAuthFetch: () => mockAuthFetch
}));

const mockEquipos = [
    { id: 'eq-1', placa: 'ABC-123', descripcion: 'Excavadora CAT' },
    { id: 'eq-2', placa: 'DEF-456', descripcion: 'Retroexcavadora JD' }
];

const mockRutinas = [
    {
        id: 'rut-1',
        descripcion: 'Rutina 500hrs',
        grupo: 'GRUPO-A',
        partes: [
            {
                actividades: [
                    { frecuencia: 250 },
                    { frecuencia: 500 }
                ]
            }
        ]
    }
];

const renderComponent = () => {
    return render(
        <BrowserRouter>
            <CreateOrder />
        </BrowserRouter>
    );
};

describe('CreateOrder Component', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        mockAuthFetch
            .mockResolvedValueOnce({ ok: true, json: async () => mockRutinas })
            .mockResolvedValueOnce({ ok: true, json: async () => mockEquipos });
    });

    it('renders the form with title', async () => {
        await act(async () => {
            renderComponent();
        });

        expect(screen.getByText('Nueva Orden de Trabajo')).toBeInTheDocument();
    });

    it('renders subtitle', async () => {
        await act(async () => {
            renderComponent();
        });

        expect(screen.getByText('Diligencia los datos para crear una nueva orden')).toBeInTheDocument();
    });

    it('renders equipment autocomplete section', async () => {
        await act(async () => {
            renderComponent();
        });

        expect(screen.getByText('1. Selecciona el Equipo')).toBeInTheDocument();
    });

    it('renders order details section', async () => {
        await act(async () => {
            renderComponent();
        });

        expect(screen.getByText('2. Detalles de la Orden')).toBeInTheDocument();
    });

    it('disables submit button when no equipment selected', async () => {
        await act(async () => {
            renderComponent();
        });

        const submitButton = screen.getByText('Crear Orden');
        expect(submitButton).toBeDisabled();
    });

    it('shows date picker with today date', async () => {
        await act(async () => {
            renderComponent();
        });

        const dateInput = screen.getByLabelText('Fecha de la OT');
        expect(dateInput).toBeInTheDocument();
        expect(dateInput).toHaveValue(new Date().toISOString().split('T')[0]);
    });

    it('renders order type options', async () => {
        await act(async () => {
            renderComponent();
        });

        // Type selector is visible
        // MUI select renders the label as a legend or input label, use getByLabelText
        expect(screen.getByLabelText('Tipo de Orden')).toBeInTheDocument();
    });

    it('shows equipment search input', async () => {
        await act(async () => {
            renderComponent();
        });

        const searchInput = screen.getByLabelText(/Buscar equipo/i);
        expect(searchInput).toBeInTheDocument();
    });

    it('fetches rutinas and equipos on mount', async () => {
        await act(async () => {
            renderComponent();
        });

        expect(mockAuthFetch).toHaveBeenCalledWith('/rutinas');
        expect(mockAuthFetch).toHaveBeenCalledWith('/equipos');
    });

    it('shows Crear Orden button', async () => {
        await act(async () => {
            renderComponent();
        });

        expect(screen.getByText('Crear Orden')).toBeInTheDocument();
    });
});
