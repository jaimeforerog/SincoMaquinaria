import { render, screen, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { NotificationProvider } from '../contexts/NotificationContext';
import EmployeeConfig from './EmployeeConfig';

// Mock useAuthFetch
const mockAuthFetch = vi.fn();
vi.mock('../hooks/useAuthFetch', () => ({
    useAuthFetch: () => mockAuthFetch
}));

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
        mockAuthFetch.mockResolvedValue({
            ok: true,
            json: async () => mockEmployees
        });
    });

    it('renders the component title', async () => {
        await act(async () => {
            render(<NotificationProvider><EmployeeConfig /></NotificationProvider>);
        });

        expect(screen.getByText('Gestión de Empleados')).toBeInTheDocument();
    });

    it('fetches employees on mount', async () => {
        await act(async () => {
            render(<NotificationProvider><EmployeeConfig /></NotificationProvider>);
        });

        expect(mockAuthFetch).toHaveBeenCalledWith('/empleados');
    });

    it('renders Nuevo Empleado button', async () => {
        await act(async () => {
            render(<NotificationProvider><EmployeeConfig /></NotificationProvider>);
        });

        expect(screen.getByText('Nuevo Empleado')).toBeInTheDocument();
    });

    it('renders table headers', async () => {
        await act(async () => {
            render(<NotificationProvider><EmployeeConfig /></NotificationProvider>);
        });

        expect(screen.getByText('Nombre')).toBeInTheDocument();
        expect(screen.getByText('Identificación')).toBeInTheDocument();
        expect(screen.getByText('Cargo')).toBeInTheDocument();
    });
});
