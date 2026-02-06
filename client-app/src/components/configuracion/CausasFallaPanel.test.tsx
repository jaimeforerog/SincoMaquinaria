import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import CausasFallaPanel from './CausasFallaPanel';
import { useAuthFetch } from '../../hooks/useAuthFetch';

// Mock useAuthFetch
vi.mock('../../hooks/useAuthFetch');
const mockAuthFetch = vi.fn();

describe('CausasFallaPanel', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        (useAuthFetch as any).mockReturnValue(mockAuthFetch);
    });

    const mockCausas = [
        { codigo: 'C1', descripcion: 'Causa 1', activo: true }
    ];

    it('renders and fetches causes on mount', async () => {
        mockAuthFetch.mockResolvedValue({
            ok: true,
            json: async () => mockCausas
        });

        render(<CausasFallaPanel />);

        expect(screen.getByText('Nueva Causa')).toBeInTheDocument();
        await waitFor(() => {
            expect(screen.getByText('Causa 1')).toBeInTheDocument();
        });
        expect(mockAuthFetch).toHaveBeenCalledWith('/configuracion/causas-falla');
    });
});
