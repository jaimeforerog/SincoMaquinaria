import { render, screen, fireEvent, waitFor, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { BrowserRouter } from 'react-router-dom';
import Login from './Login';

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
    const actual = await vi.importActual('react-router-dom');
    return { ...actual, useNavigate: () => mockNavigate };
});

const mockLogin = vi.fn();
vi.mock('../contexts/AuthContext', () => ({
    useAuth: () => ({ login: mockLogin })
}));

const renderLogin = () => render(<BrowserRouter><Login /></BrowserRouter>);

describe('Login', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders the login form with title', () => {
        renderLogin();
        expect(screen.getByText('SincoMaquinaria')).toBeInTheDocument();
        expect(screen.getByText('Sistema de Gestión de Mantenimiento')).toBeInTheDocument();
    });

    it('renders email and password fields', () => {
        renderLogin();
        expect(screen.getByLabelText(/Correo Electrónico o Usuario/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/Contraseña/i)).toBeInTheDocument();
    });

    it('renders submit button', () => {
        renderLogin();
        expect(screen.getByText('Iniciar Sesión')).toBeInTheDocument();
    });

    it('toggles password visibility', () => {
        renderLogin();
        const passwordInput = screen.getByLabelText(/Contraseña/i);
        expect(passwordInput).toHaveAttribute('type', 'password');

        const toggleBtn = screen.getByLabelText('toggle password visibility');
        fireEvent.click(toggleBtn);
        expect(passwordInput).toHaveAttribute('type', 'text');

        fireEvent.click(toggleBtn);
        expect(passwordInput).toHaveAttribute('type', 'password');
    });

    it('calls login and navigates on success', async () => {
        mockLogin.mockResolvedValue(undefined);
        renderLogin();

        fireEvent.change(screen.getByLabelText(/Correo Electrónico o Usuario/i), { target: { value: 'user@test.com' } });
        fireEvent.change(screen.getByLabelText(/Contraseña/i), { target: { value: 'password123' } });
        fireEvent.click(screen.getByText('Iniciar Sesión'));

        await waitFor(() => {
            expect(mockLogin).toHaveBeenCalledWith('user@test.com', 'password123');
            expect(mockNavigate).toHaveBeenCalledWith('/');
        });
    });

    it('displays error when login fails with message', async () => {
        mockLogin.mockRejectedValue(new Error('Credenciales inválidas'));
        renderLogin();

        fireEvent.change(screen.getByLabelText(/Correo Electrónico o Usuario/i), { target: { value: 'bad@test.com' } });
        fireEvent.change(screen.getByLabelText(/Contraseña/i), { target: { value: 'wrong' } });
        fireEvent.click(screen.getByText('Iniciar Sesión'));

        await waitFor(() => {
            expect(screen.getByText('Credenciales inválidas')).toBeInTheDocument();
        });
    });

    it('displays default error when login fails without message', async () => {
        mockLogin.mockRejectedValue(new Error(''));
        renderLogin();

        fireEvent.change(screen.getByLabelText(/Correo Electrónico o Usuario/i), { target: { value: 'bad@test.com' } });
        fireEvent.change(screen.getByLabelText(/Contraseña/i), { target: { value: 'wrong' } });
        fireEvent.click(screen.getByText('Iniciar Sesión'));

        await waitFor(() => {
            expect(screen.getByText('Error al iniciar sesión. Verifica tus credenciales.')).toBeInTheDocument();
        });
    });

    it('shows loading spinner during submission', async () => {
        let resolveLogin: () => void;
        mockLogin.mockReturnValue(new Promise<void>((resolve) => { resolveLogin = resolve; }));
        renderLogin();

        fireEvent.change(screen.getByLabelText(/Correo Electrónico o Usuario/i), { target: { value: 'user@test.com' } });
        fireEvent.change(screen.getByLabelText(/Contraseña/i), { target: { value: 'pass' } });
        fireEvent.click(screen.getByText('Iniciar Sesión'));

        await waitFor(() => {
            expect(screen.getByRole('progressbar')).toBeInTheDocument();
        });

        await act(async () => { resolveLogin!(); });
    });
});
