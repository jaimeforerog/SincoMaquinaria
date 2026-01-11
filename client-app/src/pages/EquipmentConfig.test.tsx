import { render, screen, fireEvent, waitFor, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import EquipmentConfig from './EquipmentConfig';

// Mock authFetch
const mockAuthFetch = vi.fn();

// Mock the hook module
vi.mock('../hooks/useAuthFetch', () => ({
    useAuthFetch: () => mockAuthFetch
}));

const mockEquipos = [
    {
        id: 'eq-1',
        placa: 'ABC-123',
        descripcion: 'Excavadora CAT 320',
        serie: 'SER001',
        codigo: 'COD001',
        tipoMedidorId: 'HOROMETRO',
        tipoMedidorId2: '',
        grupo: 'EXCAVADORAS',
        rutina: 'RUT-001'
    },
    {
        id: 'eq-2',
        placa: 'DEF-456',
        descripcion: 'Retroexcavadora JD 310',
        serie: 'SER002',
        codigo: 'COD002',
        tipoMedidorId: 'HOROMETRO',
        tipoMedidorId2: 'ODOMETRO',
        grupo: 'RETROEXCAVADORAS',
        rutina: 'RUT-002'
    }
];

describe('EquipmentConfig Component', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        mockAuthFetch.mockResolvedValue({
            ok: true,
            json: async () => mockEquipos
        });
    });

    it('renders the component title', async () => {
        await act(async () => {
            render(<EquipmentConfig />);
        });

        expect(screen.getByText('Configuración de Equipos')).toBeInTheDocument();
    });

    it('fetches equipment list on mount', async () => {
        await act(async () => {
            render(<EquipmentConfig />);
        });

        expect(mockAuthFetch).toHaveBeenCalledWith('/equipos');
    });

    it('renders table headers', async () => {
        await act(async () => {
            render(<EquipmentConfig />);
        });

        expect(screen.getByText('Placa')).toBeInTheDocument();
        expect(screen.getByText('Descripción')).toBeInTheDocument();
        expect(screen.getByText('Grupo')).toBeInTheDocument();
        expect(screen.getByText('Acción')).toBeInTheDocument();
    });

    it('displays equipment placas', async () => {
        await act(async () => {
            render(<EquipmentConfig />);
        });

        await waitFor(() => {
            expect(screen.getByText('ABC-123')).toBeInTheDocument();
            expect(screen.getByText('DEF-456')).toBeInTheDocument();
        });
    });

    it('displays equipment descriptions', async () => {
        await act(async () => {
            render(<EquipmentConfig />);
        });

        await waitFor(() => {
            expect(screen.getByText('Excavadora CAT 320')).toBeInTheDocument();
            expect(screen.getByText('Retroexcavadora JD 310')).toBeInTheDocument();
        });
    });

    it('displays group chips', async () => {
        await act(async () => {
            render(<EquipmentConfig />);
        });

        await waitFor(() => {
            expect(screen.getByText('EXCAVADORAS')).toBeInTheDocument();
            expect(screen.getByText('RETROEXCAVADORAS')).toBeInTheDocument();
        });
    });

    it('opens edit dialog when edit button is clicked', async () => {
        await act(async () => {
            render(<EquipmentConfig />);
        });

        await waitFor(() => {
            expect(screen.getByText('ABC-123')).toBeInTheDocument();
        });

        const row = screen.getByText('ABC-123').closest('tr');
        const editButton = row?.querySelector('button');

        if (editButton) fireEvent.click(editButton);

        await waitFor(() => {
            expect(screen.getByText('Editar Equipo')).toBeInTheDocument();
        });
    });

    it('populates form with equipment data in edit mode', async () => {
        await act(async () => {
            render(<EquipmentConfig />);
        });

        await waitFor(() => {
            expect(screen.getByText('ABC-123')).toBeInTheDocument();
        });

        const row = screen.getByText('ABC-123').closest('tr');
        const editButton = row?.querySelector('button');
        if (editButton) fireEvent.click(editButton);

        await waitFor(() => {
            expect(screen.getByDisplayValue('ABC-123')).toBeInTheDocument();
            expect(screen.getByDisplayValue('Excavadora CAT 320')).toBeInTheDocument();
        });
    });

    it('closes edit dialog on cancel', async () => {
        await act(async () => {
            render(<EquipmentConfig />);
        });

        await waitFor(() => {
            expect(screen.getByText('ABC-123')).toBeInTheDocument();
        });

        const row = screen.getByText('ABC-123').closest('tr');
        const editButton = row?.querySelector('button');
        if (editButton) fireEvent.click(editButton);

        await waitFor(() => {
            expect(screen.getByText('Editar Equipo')).toBeInTheDocument();
        });

        const cancelButton = screen.getByText('Cancelar');
        fireEvent.click(cancelButton);

        await waitFor(() => {
            expect(screen.queryByText('Editar Equipo')).not.toBeInTheDocument();
        });
    });

    it('shows Guardar button in edit dialog', async () => {
        await act(async () => {
            render(<EquipmentConfig />);
        });

        await waitFor(() => {
            expect(screen.getByText('ABC-123')).toBeInTheDocument();
        });

        const row = screen.getByText('ABC-123').closest('tr');
        const editButton = row?.querySelector('button');
        if (editButton) fireEvent.click(editButton);

        await waitFor(() => {
            expect(screen.getByText('Guardar')).toBeInTheDocument();
        });
    });
});
