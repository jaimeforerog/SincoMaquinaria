import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import MedidoresPanel from './MedidoresPanel';
import { useAuthFetch } from '../../hooks/useAuthFetch';

// Mock useAuthFetch
vi.mock('../../hooks/useAuthFetch');
const mockAuthFetch = vi.fn();

describe('MedidoresPanel', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        (useAuthFetch as any).mockReturnValue(mockAuthFetch);
    });

    const mockMedidores = [
        { codigo: 'HORAS', nombre: 'Horas Motor', unidad: 'HR', activo: true }
    ];

    it('renders and fetches descriptors on mount', async () => {
        mockAuthFetch.mockResolvedValue({
            ok: true,
            json: async () => mockMedidores
        });

        render(<MedidoresPanel />);

        expect(screen.getByText('Nuevo Medidor')).toBeInTheDocument();
        await waitFor(() => {
            expect(screen.getByText('Horas Motor')).toBeInTheDocument();
            expect(screen.getByText('HR')).toBeInTheDocument();
        });
        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/medidores');
    });
});
