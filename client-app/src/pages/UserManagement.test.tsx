import { render, screen, waitFor, act, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import UserManagement from './UserManagement';

const mockAuthFetch = vi.fn();
vi.mock('../hooks/useAuthFetch', () => ({
    useAuthFetch: () => mockAuthFetch
}));

const mockUsuarios = [
    { id: 'usr-1', nombre: 'Juan Admin', email: 'juan@test.com', rol: 'Admin', activo: true, fechaCreacion: '2024-01-15T10:00:00Z' },
    { id: 'usr-2', nombre: 'Maria User', email: 'maria@test.com', rol: 'User', activo: false, fechaCreacion: '2024-02-20T08:00:00Z' }
];

describe('UserManagement', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => mockUsuarios });
    });

    it('renders page title "Gestión de Usuarios"', async () => {
        await act(async () => { render(<UserManagement />); });
        expect(screen.getByText('Gestión de Usuarios')).toBeInTheDocument();
    });

    it('renders Crear Usuario button', async () => {
        await act(async () => { render(<UserManagement />); });
        expect(screen.getByText('Crear Usuario')).toBeInTheDocument();
    });

    it('fetches users on mount', async () => {
        await act(async () => { render(<UserManagement />); });
        expect(mockAuthFetch).toHaveBeenCalledWith('/auth/users');
    });

    it('displays users in table', async () => {
        await act(async () => { render(<UserManagement />); });

        await waitFor(() => {
            expect(screen.getByText('Juan Admin')).toBeInTheDocument();
            expect(screen.getByText('juan@test.com')).toBeInTheDocument();
            expect(screen.getByText('Maria User')).toBeInTheDocument();
        });
    });

    it('displays rol chips', async () => {
        await act(async () => { render(<UserManagement />); });

        await waitFor(() => {
            expect(screen.getByText('Admin')).toBeInTheDocument();
            expect(screen.getByText('User')).toBeInTheDocument();
        });
    });

    it('displays activo status chips', async () => {
        await act(async () => { render(<UserManagement />); });

        await waitFor(() => {
            expect(screen.getByText('Activo')).toBeInTheDocument();
            expect(screen.getByText('Inactivo')).toBeInTheDocument();
        });
    });

    it('shows empty state when no users', async () => {
        mockAuthFetch.mockResolvedValue({ ok: true, json: async () => [] });

        await act(async () => { render(<UserManagement />); });

        await waitFor(() => {
            expect(screen.getByText(/No hay usuarios registrados/)).toBeInTheDocument();
        });
    });

    it('shows error when fetch fails', async () => {
        mockAuthFetch.mockResolvedValue({
            ok: false, status: 500, statusText: 'Server Error',
            text: async () => 'Internal error'
        });

        await act(async () => { render(<UserManagement />); });

        await waitFor(() => {
            expect(screen.getByText(/Error al cargar usuarios/)).toBeInTheDocument();
        });
    });

    it('shows connection error on network failure', async () => {
        mockAuthFetch.mockRejectedValue(new Error('Network error'));

        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

        await act(async () => { render(<UserManagement />); });

        await waitFor(() => {
            expect(screen.getByText('Error de conexión al cargar usuarios')).toBeInTheDocument();
        });

        consoleSpy.mockRestore();
    });

    it('opens create dialog', async () => {
        await act(async () => { render(<UserManagement />); });

        await waitFor(() => { expect(screen.getByText('Juan Admin')).toBeInTheDocument(); });

        fireEvent.click(screen.getByText('Crear Usuario'));

        await waitFor(() => {
            expect(screen.getByText('Crear Nuevo Usuario')).toBeInTheDocument();
        });
    });

    it('opens edit dialog with user data', async () => {
        await act(async () => { render(<UserManagement />); });

        await waitFor(() => { expect(screen.getByText('Juan Admin')).toBeInTheDocument(); });

        const editButtons = screen.getAllByTestId('EditIcon');
        fireEvent.click(editButtons[0]);

        await waitFor(() => {
            expect(screen.getByText('Editar Usuario')).toBeInTheDocument();
            expect(screen.getByDisplayValue('Juan Admin')).toBeInTheDocument();
        });
    });

    it('shows validation when nombre/email empty', async () => {
        await act(async () => { render(<UserManagement />); });

        await waitFor(() => { expect(screen.getByText('Juan Admin')).toBeInTheDocument(); });

        fireEvent.click(screen.getByText('Crear Usuario'));

        await waitFor(() => { expect(screen.getByText('Crear Nuevo Usuario')).toBeInTheDocument(); });

        // Click the save button (last "Crear Usuario" in the dialog)
        const createButtons = screen.getAllByText('Crear Usuario');
        fireEvent.click(createButtons[createButtons.length - 1]);

        await waitFor(() => {
            const alerts = screen.getAllByText('Nombre y Email son obligatorios');
            expect(alerts.length).toBeGreaterThanOrEqual(1);
        });
    });

    it('shows password validation for new user', async () => {
        await act(async () => { render(<UserManagement />); });

        await waitFor(() => { expect(screen.getByText('Juan Admin')).toBeInTheDocument(); });

        fireEvent.click(screen.getByText('Crear Usuario'));

        await waitFor(() => { expect(screen.getByText('Crear Nuevo Usuario')).toBeInTheDocument(); });

        // Fill nombre and email but not password
        fireEvent.change(screen.getByLabelText(/Nombre Completo/i), { target: { value: 'New User' } });
        fireEvent.change(screen.getByLabelText(/Correo Electrónico/i), { target: { value: 'new@test.com' } });

        const createButtons = screen.getAllByText('Crear Usuario');
        fireEvent.click(createButtons[createButtons.length - 1]);

        await waitFor(() => {
            const alerts = screen.getAllByText('La contraseña es obligatoria para nuevos usuarios');
            expect(alerts.length).toBeGreaterThanOrEqual(1);
        });
    });

    it('closes dialog on Cancelar', async () => {
        await act(async () => { render(<UserManagement />); });

        await waitFor(() => { expect(screen.getByText('Juan Admin')).toBeInTheDocument(); });

        fireEvent.click(screen.getByText('Crear Usuario'));

        await waitFor(() => { expect(screen.getByText('Crear Nuevo Usuario')).toBeInTheDocument(); });

        fireEvent.click(screen.getByText('Cancelar'));

        await waitFor(() => {
            expect(screen.queryByText('Crear Nuevo Usuario')).toBeNull();
        });
    });
});
