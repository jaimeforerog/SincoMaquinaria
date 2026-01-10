import { render, screen, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import OrderDetail from './OrderDetail';

// Mock fetch
global.fetch = vi.fn();

const mockOrder = {
    id: 'order-1',
    numero: 'OT-2024-001',
    equipoId: 'eq-1',
    tipo: 'Correctivo',
    origen: 'Manual',
    estado: 'Borrador',
    fechaOrden: '2024-01-15T10:00:00Z',
    fechaCreacion: '2024-01-15T10:30:00Z',
    detalles: [
        {
            id: 'det-1',
            descripcion: 'Revisar motor',
            avance: 50,
            estado: 'En Progreso'
        }
    ],
    porcentajeAvanceGeneral: 50
};

const mockEquipo = {
    id: 'eq-1',
    placa: 'ABC-123',
    descripcion: 'Excavadora CAT'
};

const renderWithRouter = (orderId: string = 'order-1') => {
    return render(
        <MemoryRouter initialEntries={[`/ordenes/${orderId}`]}>
            <Routes>
                <Route path="/ordenes/:id" element={<OrderDetail />} />
            </Routes>
        </MemoryRouter>
    );
};

describe('OrderDetail Component', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        (global.fetch as any).mockImplementation((url: string) => {
            if (url.includes('/ordenes/')) {
                return Promise.resolve({ ok: true, json: async () => mockOrder });
            }
            if (url.includes('/equipos/')) {
                return Promise.resolve({ ok: true, json: async () => mockEquipo });
            }
            if (url.includes('/fallas')) {
                return Promise.resolve({ ok: true, json: async () => [] });
            }
            if (url.includes('/causas-falla')) {
                return Promise.resolve({ ok: true, json: async () => [] });
            }
            if (url.includes('/historial')) {
                return Promise.resolve({ ok: true, json: async () => [] });
            }
            return Promise.resolve({ ok: true, json: async () => ({}) });
        });
    });

    it('fetches order on mount', async () => {
        await act(async () => {
            renderWithRouter();
        });

        expect(global.fetch).toHaveBeenCalledWith('/ordenes/order-1');
    });

    it('renders order number', async () => {
        await act(async () => {
            renderWithRouter();
        });

        expect(screen.getByText('OT-2024-001')).toBeInTheDocument();
    });

    it('renders status chip', async () => {
        await act(async () => {
            renderWithRouter();
        });

        expect(screen.getByText('Borrador')).toBeInTheDocument();
    });

    it('renders Actividades tab', async () => {
        await act(async () => {
            renderWithRouter();
        });

        expect(screen.getByText('Actividades')).toBeInTheDocument();
    });

    it('renders Auditoría tab', async () => {
        await act(async () => {
            renderWithRouter();
        });

        expect(screen.getByText('Auditoría')).toBeInTheDocument();
    });
});
