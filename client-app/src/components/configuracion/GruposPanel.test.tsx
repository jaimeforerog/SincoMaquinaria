import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import GruposPanel from './GruposPanel';
import { useAuthFetch } from '../../hooks/useAuthFetch';

// Mock useAuthFetch
vi.mock('../../hooks/useAuthFetch');
const mockAuthFetch = vi.fn();

describe('GruposPanel', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        (useAuthFetch as any).mockReturnValue(mockAuthFetch);
    });

    const mockGrupos = [
        { codigo: 'G1', nombre: 'Grupo 1', descripcion: 'Desc 1', activo: true }
    ];

    it('renders and fetches groups on mount', async () => {
        mockAuthFetch.mockResolvedValue({
            ok: true,
            json: async () => mockGrupos
        });

        render(<GruposPanel />);

        expect(screen.getByText('Nuevo Grupo')).toBeInTheDocument();
        await waitFor(() => {
            expect(screen.getByText('Grupo 1')).toBeInTheDocument();
        });
        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/grupos');
        expect(screen.getByText('Desc 1')).toBeInTheDocument();
    });
});
