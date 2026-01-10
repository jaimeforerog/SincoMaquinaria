import { render, screen, fireEvent, waitFor, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import Configuracion from './Configuracion';

// Mock fetch
global.fetch = vi.fn();

const mockMedidores = [
    { codigo: 'MED1', nombre: 'Horómetro', unidad: 'HR', activo: true },
    { codigo: 'MED2', nombre: 'Odómetro', unidad: 'KM', activo: false }
];

const mockGrupos = [
    { codigo: 'GRP1', nombre: 'Excavadoras', descripcion: 'Grupo de excavadoras', activo: true }
];

const mockFallas = [
    { codigo: 'F1', descripcion: 'Desgaste', prioridad: 'Alta', activo: true }
];

const mockCausas = [
    { codigo: 'C1', descripcion: 'Falta de mantenimiento', activo: true }
];

const mockEmpleados = [
    { id: '1', nombre: 'Juan', identificacion: '123', cargo: 'Operario', estado: 'Activo' }
];

describe('Configuracion Component', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        (global.fetch as any).mockImplementation((url: string) => {
            if (url.includes('/medidores')) {
                return Promise.resolve({ ok: true, json: async () => mockMedidores });
            }
            if (url.includes('/grupos')) {
                return Promise.resolve({ ok: true, json: async () => mockGrupos });
            }
            if (url.includes('/fallas')) {
                return Promise.resolve({ ok: true, json: async () => mockFallas });
            }
            if (url.includes('/causas-falla')) {
                return Promise.resolve({ ok: true, json: async () => mockCausas });
            }
            if (url.includes('/empleados')) {
                return Promise.resolve({ ok: true, json: async () => mockEmpleados });
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });
    });

    it('renders the main title', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        expect(screen.getByText('Configuraciones')).toBeInTheDocument();
    });

    it('renders subtitle', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        expect(screen.getByText('Gestión de parámetros globales del sistema')).toBeInTheDocument();
    });

    it('renders all configuration tabs', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        expect(screen.getByText('Tipos de Medidor')).toBeInTheDocument();
        expect(screen.getByText('Grupos Mantenimiento')).toBeInTheDocument();
        expect(screen.getByText('Tipos de Falla')).toBeInTheDocument();
        expect(screen.getByText('Causas de Falla')).toBeInTheDocument();
        expect(screen.getByText('Empleados')).toBeInTheDocument();
    });

    it('shows MedidoresPanel by default', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        expect(screen.getByText('Nuevo Tipo')).toBeInTheDocument();
        expect(screen.getByText('Tipos Existentes')).toBeInTheDocument();
    });

    it('loads and displays medidores list', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        await waitFor(() => {
            expect(screen.getByText('Horómetro')).toBeInTheDocument();
        });
    });

    it('shows Crear Medidor button', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        expect(screen.getByText('Crear Medidor')).toBeInTheDocument();
    });

    it('shows medidor form fields', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        expect(screen.getByLabelText(/Nombre del Medidor/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/Unidad de Medida/i)).toBeInTheDocument();
    });

    it('switches to Grupos tab when clicked', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        const tab = screen.getByText('Grupos Mantenimiento');
        fireEvent.click(tab);

        await waitFor(() => {
            expect(screen.getByText('Nuevo Grupo')).toBeInTheDocument();
            expect(screen.getByText('Grupos Definidos')).toBeInTheDocument();
        });
    });

    it('loads grupos data when tab is clicked', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        const tab = screen.getByText('Grupos Mantenimiento');
        fireEvent.click(tab);

        await waitFor(() => {
            expect(screen.getByText('Excavadoras')).toBeInTheDocument();
        });
    });

    it('switches to Fallas tab when clicked', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        const tab = screen.getByText('Tipos de Falla');
        fireEvent.click(tab);

        await waitFor(() => {
            expect(screen.getByText('Nuevo Tipo de Falla')).toBeInTheDocument();
        });
    });

    it('loads fallas data when tab is clicked', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        const tab = screen.getByText('Tipos de Falla');
        fireEvent.click(tab);

        await waitFor(() => {
            expect(screen.getByText('Desgaste')).toBeInTheDocument();
        });
    });

    it('switches to Causas tab when clicked', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        const tab = screen.getByText('Causas de Falla');
        fireEvent.click(tab);

        await waitFor(() => {
            expect(screen.getByText('Nueva Causa')).toBeInTheDocument();
        });
    });

    it('loads causas data when tab is clicked', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        const tab = screen.getByText('Causas de Falla');
        fireEvent.click(tab);

        await waitFor(() => {
            expect(screen.getByText('Falta de mantenimiento')).toBeInTheDocument();
        });
    });

    it('shows empty state for medidores', async () => {
        (global.fetch as any).mockResolvedValue({
            ok: true,
            json: async () => []
        });

        await act(async () => {
            render(<Configuracion />);
        });

        await waitFor(() => {
            expect(screen.getByText('No hay medidores configurados.')).toBeInTheDocument();
        });
    });

    it('shows Crear Grupo button in grupos tab', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        const tab = screen.getByText('Grupos Mantenimiento');
        fireEvent.click(tab);

        await waitFor(() => {
            expect(screen.getByText('Crear Grupo')).toBeInTheDocument();
        });
    });

    it('shows Crear Tipo de Falla button', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        const tab = screen.getByText('Tipos de Falla');
        fireEvent.click(tab);

        await waitFor(() => {
            expect(screen.getByText('Crear Tipo de Falla')).toBeInTheDocument();
        });
    });

    it('shows Crear Causa button', async () => {
        await act(async () => {
            render(<Configuracion />);
        });

        const tab = screen.getByText('Causas de Falla');
        fireEvent.click(tab);

        await waitFor(() => {
            expect(screen.getByText('Crear Causa')).toBeInTheDocument();
        });
    });
});
