import { useState, useEffect, Fragment } from 'react';
import {
    Container, Typography, Paper, Table, TableBody, TableCell, TableContainer,
    TableHead, TableRow, TablePagination, Box, Chip, TextField, MenuItem,
    Card, CardContent, IconButton, Collapse, CircularProgress, Button
} from '@mui/material';
import { History, Person, ExpandMore, ExpandLess, FilterList, CalendarMonth, Search } from '@mui/icons-material';
import { useAuthFetch } from '../hooks/useAuthFetch';

interface AuditEvent {
    id: string;
    streamId: string;
    tipo: string;
    modulo: string;
    fecha: string;
    version: number;
    datos?: {
        usuarioId?: string;
        usuarioNombre?: string;
        fechaAccion?: string;
        detalles?: Record<string, string>;
    };
}

interface PagedResponse {
    data: AuditEvent[];
    page: number;
    pageSize: number;
    totalCount: number;
}

interface EventoInfo {
    tipo: string;
    modulo: string;
}

interface UsuarioInfo {
    id: string;
    nombre: string;
}

const eventTypeLabels: Record<string, string> = {
    'OrdenDeTrabajoCreada': 'Orden Creada',
    'OrdenDeTrabajoEliminada': 'Orden Eliminada',
    'OrdenProgramada': 'Orden Programada',
    'OrdenFinalizada': 'Orden Finalizada',
    'ActividadAgregada': 'Actividad Agregada',
    'AvanceDeActividadRegistrado': 'Avance Registrado',
    'EquipoMigrado': 'Equipo Importado',
    'EquipoActualizado': 'Equipo Actualizado',
    'MedicionRegistrada': 'Medición Registrada',
    'EmpleadoCreado': 'Empleado Creado',
    'EmpleadoActualizado': 'Empleado Actualizado',
    'TipoMedidorCreado': 'Tipo Medidor Creado',
    'TipoMedidorActualizado': 'Tipo Medidor Actualizado',
    'EstadoTipoMedidorCambiado': 'Estado Medidor Cambiado',
    'GrupoMantenimientoCreado': 'Grupo Creado',
    'GrupoMantenimientoActualizado': 'Grupo Actualizado',
    'EstadoGrupoMantenimientoCambiado': 'Estado Grupo Cambiado',
    'TipoFallaCreado': 'Tipo Falla Creado',
    'CausaFallaCreada': 'Causa Falla Creada',
    'CausaFallaActualizada': 'Causa Falla Actualizada',
    'EstadoCausaFallaCambiado': 'Estado Causa Cambiado',
    'UsuarioCreado': 'Usuario Creado',
    'UsuarioActualizado': 'Usuario Actualizado',
    'UsuarioDesactivado': 'Usuario Desactivado',
};

const moduloColors: Record<string, 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info'> = {
    'Órdenes': 'primary',
    'Equipos': 'info',
    'Empleados': 'success',
    'Configuración': 'warning',
    'Usuarios': 'secondary',
    'Otros': 'default' as any,
};

// Helper para formatear fecha a YYYY-MM-DD para inputs tipo date
const formatDateForInput = (date: Date): string => {
    return date.toISOString().split('T')[0];
};

const Auditoria = () => {
    const authFetch = useAuthFetch();
    const [events, setEvents] = useState<AuditEvent[]>([]);
    const [loading, setLoading] = useState(false);
    const [hasSearched, setHasSearched] = useState(false);
    const [page, setPage] = useState(0);
    const [rowsPerPage, setRowsPerPage] = useState(25);
    const [totalCount, setTotalCount] = useState(0);

    // Filtros
    const [modulos, setModulos] = useState<string[]>([]);
    const [eventos, setEventos] = useState<EventoInfo[]>([]);
    const [usuarios, setUsuarios] = useState<UsuarioInfo[]>([]);

    const [filterModulo, setFilterModulo] = useState<string>('');
    const [filterTipo, setFilterTipo] = useState<string>('');
    const [filterUsuario, setFilterUsuario] = useState<string>('');

    // Fechas: hoy y un mes atrás
    const today = new Date();
    const oneMonthAgo = new Date(today);
    oneMonthAgo.setMonth(oneMonthAgo.getMonth() - 1);

    const [fechaInicio, setFechaInicio] = useState<string>(formatDateForInput(oneMonthAgo));
    const [fechaFin, setFechaFin] = useState<string>(formatDateForInput(today));

    const [expandedRow, setExpandedRow] = useState<string | null>(null);

    useEffect(() => {
        fetchModulos();
        fetchUsuarios();
    }, []);

    useEffect(() => {
        if (filterModulo) {
            fetchEventosPorModulo(filterModulo);
        } else {
            setEventos([]);
            setFilterTipo('');
        }
    }, [filterModulo]);

    useEffect(() => {
        if (hasSearched) {
            fetchEvents();
        }
    }, [page, rowsPerPage]);

    const fetchModulos = async () => {
        try {
            const res = await authFetch('/api/auditoria/modulos');
            if (res.ok) {
                const data = await res.json();
                setModulos(data);
            }
        } catch (error) {
            console.error('Error fetching modulos:', error);
        }
    };

    const fetchEventosPorModulo = async (modulo: string) => {
        try {
            const res = await authFetch(`/api/auditoria/eventos?modulo=${encodeURIComponent(modulo)}`);
            if (res.ok) {
                const data = await res.json();
                setEventos(data);
            }
        } catch (error) {
            console.error('Error fetching eventos:', error);
        }
    };

    const fetchUsuarios = async () => {
        try {
            const res = await authFetch('/api/auditoria/usuarios');
            if (res.ok) {
                const data = await res.json();
                setUsuarios(data);
            }
        } catch (error) {
            console.error('Error fetching usuarios:', error);
        }
    };

    const fetchEvents = async () => {
        setLoading(true);
        try {
            const params = new URLSearchParams({
                page: String(page + 1),
                pageSize: String(rowsPerPage),
            });
            if (filterModulo) params.append('modulo', filterModulo);
            if (filterTipo) params.append('tipo', filterTipo);
            if (filterUsuario) params.append('usuario', filterUsuario);
            if (fechaInicio) params.append('fechaInicio', fechaInicio);
            if (fechaFin) params.append('fechaFin', fechaFin);

            const res = await authFetch(`/api/auditoria?${params}`);
            if (res.ok) {
                const data: PagedResponse = await res.json();
                setEvents(data.data);
                setTotalCount(data.totalCount);
            }
        } catch (error) {
            console.error('Error fetching audit events:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleChangePage = (_: unknown, newPage: number) => {
        setPage(newPage);
    };

    const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
        setRowsPerPage(parseInt(event.target.value, 10));
        setPage(0);
    };

    const formatDate = (dateStr: string) => {
        return new Date(dateStr).toLocaleString('es-CO', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        });
    };

    return (
        <Container maxWidth="xl" sx={{ mt: 4 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 4, gap: 2 }}>
                <History sx={{ fontSize: 40, color: 'primary.main' }} />
                <Box>
                    <Typography variant="h4" fontWeight="bold">
                        Auditoría del Sistema
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                        Registro de todos los cambios realizados en la aplicación
                    </Typography>
                </Box>
            </Box>

            {/* Estadísticas rápidas */}
            <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
                <Card sx={{ flex: 1 }}>
                    <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2, py: 1.5 }}>
                        <History color="primary" />
                        <Box>
                            <Typography variant="h5" fontWeight="bold">{totalCount}</Typography>
                            <Typography variant="caption" color="text.secondary">Eventos Encontrados</Typography>
                        </Box>
                    </CardContent>
                </Card>
                <Card sx={{ flex: 1 }}>
                    <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2, py: 1.5 }}>
                        <CalendarMonth color="secondary" />
                        <Box>
                            <Typography variant="body2" fontWeight="bold">
                                {new Date(fechaInicio).toLocaleDateString('es-CO')} - {new Date(fechaFin).toLocaleDateString('es-CO')}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">Rango de Fechas</Typography>
                        </Box>
                    </CardContent>
                </Card>
            </Box>

            {/* Filtros */}
            <Paper sx={{ p: 3, mb: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
                    <FilterList color="primary" />
                    <Typography variant="subtitle1" fontWeight="bold">Filtros</Typography>
                </Box>

                {/* Fila 1: Módulo y Evento */}
                <Box sx={{ display: 'flex', gap: 2, mb: 2, flexWrap: 'wrap' }}>
                    <TextField
                        select
                        label="Módulo"
                        value={filterModulo}
                        onChange={(e) => {
                            setFilterModulo(e.target.value);
                            setFilterTipo('');
                        }}
                        sx={{ flex: 1, minWidth: 250 }}
                        SelectProps={{
                            MenuProps: {
                                PaperProps: {
                                    style: { maxHeight: 300 }
                                }
                            }
                        }}
                    >
                        <MenuItem value="">Todos los módulos</MenuItem>
                        {modulos.map((mod) => (
                            <MenuItem key={mod} value={mod}>{mod}</MenuItem>
                        ))}
                    </TextField>
                    <TextField
                        select
                        label="Tipo de Evento"
                        value={filterTipo}
                        onChange={(e) => setFilterTipo(e.target.value)}
                        disabled={!filterModulo}
                        sx={{ flex: 1, minWidth: 250 }}
                        SelectProps={{
                            MenuProps: {
                                PaperProps: {
                                    style: { maxHeight: 300 }
                                }
                            }
                        }}
                    >
                        <MenuItem value="">Todos los eventos</MenuItem>
                        {eventos.map((ev) => (
                            <MenuItem key={ev.tipo} value={ev.tipo}>
                                {eventTypeLabels[ev.tipo] || ev.tipo}
                            </MenuItem>
                        ))}
                    </TextField>
                </Box>

                {/* Fila 2: Usuario, Fechas y Botón */}
                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'flex-end' }}>
                    <TextField
                        select
                        label="Usuario"
                        value={filterUsuario}
                        onChange={(e) => setFilterUsuario(e.target.value)}
                        sx={{ flex: 1, minWidth: 200 }}
                        SelectProps={{
                            MenuProps: {
                                PaperProps: {
                                    style: { maxHeight: 300 }
                                }
                            }
                        }}
                    >
                        <MenuItem value="">Todos los usuarios</MenuItem>
                        {usuarios.map((usr) => (
                            <MenuItem key={usr.id} value={usr.nombre}>{usr.nombre}</MenuItem>
                        ))}
                    </TextField>
                    <TextField
                        type="date"
                        label="Fecha Inicio"
                        value={fechaInicio}
                        onChange={(e) => setFechaInicio(e.target.value)}
                        sx={{ flex: 1, minWidth: 180 }}
                        InputLabelProps={{ shrink: true }}
                    />
                    <TextField
                        type="date"
                        label="Fecha Fin"
                        value={fechaFin}
                        onChange={(e) => setFechaFin(e.target.value)}
                        sx={{ flex: 1, minWidth: 180 }}
                        InputLabelProps={{ shrink: true }}
                    />
                    <Button
                        variant="contained"
                        size="large"
                        startIcon={<Search />}
                        onClick={() => { setPage(0); setHasSearched(true); fetchEvents(); }}
                        sx={{ height: '56px', minWidth: 140 }}
                    >
                        Consultar
                    </Button>
                </Box>
            </Paper>

            {/* Tabla de Eventos */}
            <Paper elevation={3} sx={{ borderRadius: 2 }}>
                <TableContainer>
                    <Table>
                        <TableHead>
                            <TableRow sx={{ bgcolor: 'action.hover' }}>
                                <TableCell width={50}></TableCell>
                                <TableCell sx={{ fontWeight: 'bold' }}>Fecha</TableCell>
                                <TableCell sx={{ fontWeight: 'bold' }}>Módulo</TableCell>
                                <TableCell sx={{ fontWeight: 'bold' }}>Evento</TableCell>
                                <TableCell sx={{ fontWeight: 'bold' }}>Usuario</TableCell>
                                <TableCell sx={{ fontWeight: 'bold' }}>Versión</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {loading ? (
                                <TableRow>
                                    <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                                        <CircularProgress />
                                    </TableCell>
                                </TableRow>
                            ) : !hasSearched ? (
                                <TableRow>
                                    <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                                        <Typography color="text.secondary">
                                            Seleccione los filtros y presione "Consultar" para buscar eventos
                                        </Typography>
                                    </TableCell>
                                </TableRow>
                            ) : events.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                                        <Typography color="text.secondary">
                                            No hay eventos registrados en el rango de fechas seleccionado
                                        </Typography>
                                    </TableCell>
                                </TableRow>
                            ) : (
                                events.map((event) => {
                                    const isExpanded = expandedRow === event.id;
                                    const moduloColor = moduloColors[event.modulo] || 'default';

                                    return (
                                        <Fragment key={event.id}>
                                            <TableRow
                                                hover
                                                onClick={() => setExpandedRow(isExpanded ? null : event.id)}
                                                sx={{ cursor: 'pointer' }}
                                            >
                                                <TableCell>
                                                    <IconButton size="small">
                                                        {isExpanded ? <ExpandLess /> : <ExpandMore />}
                                                    </IconButton>
                                                </TableCell>
                                                <TableCell>
                                                    <Typography variant="body2">
                                                        {formatDate(event.fecha)}
                                                    </Typography>
                                                </TableCell>
                                                <TableCell>
                                                    <Chip
                                                        label={event.modulo}
                                                        color={moduloColor}
                                                        size="small"
                                                    />
                                                </TableCell>
                                                <TableCell>
                                                    <Typography variant="body2">
                                                        {eventTypeLabels[event.tipo] || event.tipo}
                                                    </Typography>
                                                </TableCell>
                                                <TableCell>
                                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                        <Person fontSize="small" color="action" />
                                                        <Typography variant="body2">
                                                            {event.datos?.usuarioNombre || 'Sistema'}
                                                        </Typography>
                                                    </Box>
                                                </TableCell>
                                                <TableCell>
                                                    <Chip label={`v${event.version}`} size="small" variant="outlined" />
                                                </TableCell>
                                            </TableRow>
                                            <TableRow>
                                                <TableCell colSpan={6} sx={{ p: 0, borderBottom: isExpanded ? undefined : 'none' }}>
                                                    <Collapse in={isExpanded}>
                                                        <Box sx={{ p: 2, bgcolor: 'action.hover' }}>
                                                            <Typography variant="subtitle2" gutterBottom>
                                                                Detalles del Evento
                                                            </Typography>
                                                            <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap' }}>
                                                                <Box>
                                                                    <Typography variant="caption" color="text.secondary">Stream ID</Typography>
                                                                    <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.75rem' }}>
                                                                        {event.streamId}
                                                                    </Typography>
                                                                </Box>
                                                                {event.datos?.detalles && Object.entries(event.datos.detalles).map(([key, value]) => (
                                                                    <Box key={key}>
                                                                        <Typography variant="caption" color="text.secondary">{key}</Typography>
                                                                        <Typography variant="body2">{value || '-'}</Typography>
                                                                    </Box>
                                                                ))}
                                                            </Box>
                                                        </Box>
                                                    </Collapse>
                                                </TableCell>
                                            </TableRow>
                                        </Fragment>
                                    );
                                })
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>
                <TablePagination
                    component="div"
                    count={totalCount}
                    page={page}
                    onPageChange={handleChangePage}
                    rowsPerPage={rowsPerPage}
                    onRowsPerPageChange={handleChangeRowsPerPage}
                    rowsPerPageOptions={[10, 25, 50, 100]}
                    labelRowsPerPage="Filas por página:"
                    labelDisplayedRows={({ from, to, count }) => `${from}-${to} de ${count}`}
                />
            </Paper>
        </Container>
    );
};

export default Auditoria;
