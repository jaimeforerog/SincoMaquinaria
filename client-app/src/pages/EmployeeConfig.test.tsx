import { render, screen, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import EmployeeConfig from './EmployeeConfig';

// Mock fetch
global.fetch = vi.fn();

const mockEmployees = [
    {
        id: '1',
        nombre: 'Juan Perez',
        identificacion: '12345',
        cargo: 'Operario',
        estado: 'Activo'
    }
];

describe('EmployeeConfig Component', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        (global.fetch as any).mockResolvedValue({
            ok: true,
            json: async () => mockEmployees
        });
    });

    it('renders the component title', async () => {
        await act(async () => {
            render(<EmployeeConfig />);
        });

        expect(screen.getByText('Gestión de Empleados')).toBeInTheDocument();
    });

    it('fetches employees on mount', async () => {
        await act(async () => {
            render(<EmployeeConfig />);
        });

        expect(global.fetch).toHaveBeenCalledWith('/empleados');
    });

    it('renders Nuevo Empleado button', async () => {
        await act(async () => {
            render(<EmployeeConfig />);
        });

        expect(screen.getByText('Nuevo Empleado')).toBeInTheDocument();
    });

    it('renders table headers', async () => {
        await act(async () => {
            render(<EmployeeConfig />);
        });

        expect(screen.getByText('Nombre')).toBeInTheDocument();
        expect(screen.getByText('Identificación')).toBeInTheDocument();
        expect(screen.getByText('Cargo')).toBeInTheDocument();
    });
});
