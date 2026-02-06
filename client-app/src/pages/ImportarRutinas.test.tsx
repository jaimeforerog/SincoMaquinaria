import { render, screen, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import ImportarRutinas from './ImportarRutinas';

// Mock useAuthFetch
const mockAuthFetch = vi.fn();
vi.mock('../hooks/useAuthFetch', () => ({
    useAuthFetch: () => mockAuthFetch
}));

// Mock useAuth for Admin role check
vi.mock('../contexts/AuthContext', () => ({
    useAuth: () => ({ user: { rol: 'Admin' } })
}));

describe('ImportarRutinas Component', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        mockAuthFetch.mockResolvedValue({
            ok: true,
            json: async () => ({})
        });
    });

    it('renders the component title', async () => {
        await act(async () => {
            render(<ImportarRutinas />);
        });

        expect(screen.getByText('Importación de Datos')).toBeInTheDocument();
    });

    it('renders Rutinas tab', async () => {
        await act(async () => {
            render(<ImportarRutinas />);
        });

        expect(screen.getByText('Rutinas de Mantenimiento')).toBeInTheDocument();
    });

    it('renders Equipos tab', async () => {
        await act(async () => {
            render(<ImportarRutinas />);
        });

        expect(screen.getByText('Equipos')).toBeInTheDocument();
    });

    it('renders Empleados tab', async () => {
        await act(async () => {
            render(<ImportarRutinas />);
        });

        expect(screen.getByText('Empleados')).toBeInTheDocument();
    });

    it('shows file drop area text', async () => {
        await act(async () => {
            render(<ImportarRutinas />);
        });

        expect(screen.getByText(/Haz clic o arrastra/i)).toBeInTheDocument();
    });

    it('renders upload button', async () => {
        await act(async () => {
            render(<ImportarRutinas />);
        });

        expect(screen.getByText('Importar Rutinas')).toBeInTheDocument();
    });
});
