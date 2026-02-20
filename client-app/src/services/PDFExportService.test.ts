import { describe, it, expect, vi, beforeEach } from 'vitest';
import { exportOrdenToPDF } from './PDFExportService';
import { Equipo } from '../types';

// Mock dependencies
const mockRect = vi.fn();
const mockText = vi.fn();
const mockSetFillColor = vi.fn();
const mockSetTextColor = vi.fn();
const mockSetFontSize = vi.fn();
const mockSave = vi.fn();

// Mock jsPDF constructor and methods
vi.mock('jspdf', () => {
    return {
        default: class {
            rect = mockRect;
            text = mockText;
            setFillColor = mockSetFillColor;
            setTextColor = mockSetTextColor;
            setFontSize = mockSetFontSize;
            save = mockSave;
            lastAutoTable = { finalY: 100 };
        }
    };
});

// Mock autoTable
const mockAutoTable = vi.fn();
vi.mock('jspdf-autotable', () => ({
    default: (doc: any, options: any) => mockAutoTable(doc, options)
}));

describe('PDFExportService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    const mockOrder = {
        numero: 'OT-123',
        estado: 'Abierta',
        tipo: 'Preventivo',
        fechaCreacion: new Date().toISOString(),
        detalles: [
            { descripcion: 'Activity 1', estado: 'Pendiente', avance: 0, frecuencia: 100 }
        ]
    };

    const mockEquipo = {
        placa: 'EQ-001',
        descripcion: 'Excavadora'
    } as unknown as Equipo;

    it('generates PDF for Preventive order correctly', () => {
        exportOrdenToPDF(mockOrder as any, mockEquipo, [], [], []);

        // Check Header
        expect(mockText).toHaveBeenCalledWith('Orden de Trabajo', 14, 20);
        expect(mockText).toHaveBeenCalledWith(expect.stringContaining('OT-123'), 14, 30);

        // Check AutoTable Calls (Info and Activities)
        expect(mockAutoTable).toHaveBeenCalledTimes(2);

        // Check table columns for Preventive (should include Frecuencia)
        const activitiesCall = mockAutoTable.mock.calls[1][1];
        expect(activitiesCall.head[0]).toContain('Frecuencia (h)');
        expect(activitiesCall.head[0]).not.toContain('Tipo Falla');

        // Check Save
        expect(mockSave).toHaveBeenCalledWith('ot_OT-123.pdf');
    });

    it('generates PDF for Corrective order correctly', () => {
        const correctiveOrder = {
            ...mockOrder,
            tipo: 'Correctivo',
            detalles: [
                {
                    descripcion: 'Fix Engine',
                    estado: 'En Proceso',
                    avance: 50,
                    tipoFallaId: 'TF1',
                    causaFallaId: 'CF1'
                }
            ]
        };

        const tiposFalla = [{ codigo: 'TF1', descripcion: 'Overheating', prioridad: 'Alta' as const, activo: true }];
        const causasFalla = [{ codigo: 'CF1', descripcion: 'Lack of Oil', activo: true }];

        exportOrdenToPDF(correctiveOrder as any, mockEquipo, [], tiposFalla, causasFalla);

        // Check AutoTable Columns for Corrective
        const activitiesCall = mockAutoTable.mock.calls[1][1];
        expect(activitiesCall.head[0]).toContain('Tipo Falla');
        expect(activitiesCall.head[0]).toContain('Causa Falla');
        expect(activitiesCall.head[0]).not.toContain('Frecuencia (h)');

        // Check Data Mapping (Descriptions resolved)
        const rowData = activitiesCall.body[0];
        expect(rowData).toContain('Overheating'); // Resolved description
        expect(rowData).toContain('Lack of Oil'); // Resolved description
    });

    it('handles missing activities gracefullly', () => {
        const emptyOrder = { ...mockOrder, detalles: [] };
        exportOrdenToPDF(emptyOrder as any, mockEquipo, [], [], []);

        // Should verify that "No hay actividades" text is rendered
        expect(mockText).toHaveBeenCalledWith('No hay actividades registradas.', expect.any(Number), expect.any(Number));

        // AutoTable should only be called once for Info, not for activities
        expect(mockAutoTable).toHaveBeenCalledTimes(1);
    });
});
