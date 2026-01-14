import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { OrdenDeTrabajo } from '../types';

export const exportOrdenToPDF = (
    order: OrdenDeTrabajo & { detalles?: any[] },
    equipo: any,
    history: any[],
    tiposFalla?: any[],
    causasFalla?: any[]
) => {
    const doc = new jsPDF();
    const isCorrective = order.tipo === 'Correctivo';
    const isPreventive = order.tipo === 'Preventivo';

    // --- Header ---
    doc.setFillColor(16, 42, 67); // Dark Blue (Header bg)
    doc.rect(0, 0, 210, 40, 'F');

    doc.setTextColor(255, 255, 255);
    doc.setFontSize(22);
    doc.text('Orden de Trabajo', 14, 20);

    doc.setFontSize(12);
    doc.text(`N°: ${order.numero}`, 14, 30);
    doc.text(`Fecha: ${new Date().toLocaleDateString()}`, 150, 20);
    doc.text(`Estado: ${order.estado}`, 150, 30);

    // --- General Information ---
    doc.setTextColor(0, 0, 0);
    doc.setFontSize(12);
    doc.text('Información del Equipo', 14, 50);

    const infoData = [
        ['Equipo/Placa', equipo ? `${equipo.placa} - ${equipo.descripcion}` : 'N/A'],
        ['Tipo de Mantenimiento', order.tipo],
        ['Fecha de Creación', new Date(order.fechaCreacion as any).toLocaleString()],
        ['Responsable', "Sin Asignar (Por definir)"]
    ];

    autoTable(doc, {
        startY: 55,
        head: [['Campo', 'Valor']],
        body: infoData,
        theme: 'striped',
        headStyles: { fillColor: [79, 195, 247] }, // Light Blue
        columnStyles: { 0: { fontStyle: 'bold', width: 60 } }
    });

    // --- Activities Table ---
    const lastY = (doc as any).lastAutoTable.finalY + 15;
    doc.text('Detalle de Actividades', 14, lastY);

    // Define columns based on Order Type
    let columns = ['Actividad', 'Estado', 'Avance %'];

    if (isPreventive) {
        // Insert Frequency after Actividad
        columns.splice(1, 0, 'Frecuencia (h)');
    }

    if (isCorrective) {
        // Append Failure fields
        columns.push('Tipo Falla', 'Causa Falla');
    }

    // Helper to get description from code
    const getTipoFallaDesc = (codigo: string) => {
        if (!codigo || !tiposFalla) return 'N/A';
        const tipo = tiposFalla.find(t => t.codigo === codigo);
        return tipo?.descripcion || codigo;
    };

    const getCausaFallaDesc = (codigo: string) => {
        if (!codigo || !causasFalla) return 'N/A';
        const causa = causasFalla.find(c => c.codigo === codigo);
        return causa?.descripcion || codigo;
    };

    // Map data
    const activitiesData = (order.detalles || []).map((detalle: any) => {
        const row = [
            detalle.descripcion,
            detalle.estado,
            `${detalle.avance}%`
        ];

        if (isPreventive) {
            row.splice(1, 0, detalle.frecuencia > 0 ? `${detalle.frecuencia}` : '-');
        }

        if (isCorrective) {
            row.push(getTipoFallaDesc(detalle.tipoFallaId));
            row.push(getCausaFallaDesc(detalle.causaFallaId));
        }

        return row;
    });

    if (activitiesData.length > 0) {
        autoTable(doc, {
            startY: lastY + 5,
            head: [columns],
            body: activitiesData,
            theme: 'grid',
            headStyles: { fillColor: [21, 101, 192] }, // Darker Blue
            styles: { fontSize: 9 }
        });
    } else {
        doc.setFontSize(10);
        doc.setTextColor(100);
        doc.text('No hay actividades registradas.', 14, lastY + 10);
    }

    // --- Footer ---
    const footerY = 280;
    doc.setFontSize(8);
    doc.setTextColor(150);
    doc.text('SincoMaquinaria - Generado automáticamente', 14, footerY);
    doc.text(`Página 1`, 190, footerY);

    doc.save(`ot_${order.numero}.pdf`);
};
