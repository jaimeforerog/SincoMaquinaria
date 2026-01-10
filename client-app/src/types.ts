export interface OrdenDeTrabajo {
    id: string;
    numero: string;
    equipoId: string;
    origen: string;
    tipo: string;
    estado: string;
    fechaCreacion: string;
    fechaOrden?: string;
    rutinaId?: string | null;
    detalles?: DetalleOrden[];
}

export interface DetalleOrden {
    id: string;
    descripcion: string;
    avance: number;
    estado: string;
    observaciones: string;
    frecuencia?: number;
    tipoFallaId?: string;
    causaFallaId?: string;
}

export interface Equipo {
    id: string;
    codigo: string;
    nombre: string;
    modelo?: string;
    serie?: string;
    ubicacion?: string;
    rutina?: string;
}

export interface Rutina {
    id: string;
    descripcion: string;
    grupo: string;
    partes?: Parte[];
}

export interface Parte {
    id: string;
    descripcion: string;
    actividades: Actividad[];
}

export interface Actividad {
    id: string;
    descripcion: string;
    clase: string;
    frecuencia: number;
    unidadMedida: string;
    alertaFaltando: number;
    insumo?: string;
    cantidad: number;
}

export interface ErrorLog {
    id: string;
    message: string;
    stackTrace: string;
    path: string;
    fecha: string;
}

export interface TipoMedidor {
    codigo: string;
    nombre: string;
    unidad: string;
    activo: boolean;
}

export interface GrupoMantenimiento {
    codigo: string;
    nombre: string;
    descripcion: string;
    activo: boolean;
}

export interface TipoFalla {
    codigo: string;
    descripcion: string;
    prioridad: 'Alta' | 'Media' | 'Baja';
    activo: boolean;
}

export interface CausaFalla {
    codigo: string;
    descripcion: string;
    activo: boolean;
}

export interface ConfiguracionGlobal {
    id: string;
    tiposMedidor: TipoMedidor[];
    gruposMantenimiento: GrupoMantenimiento[];
}
