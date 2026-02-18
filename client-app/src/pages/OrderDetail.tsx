import { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { ArrowBack, CalendarToday, Person, LocalShipping, AccessTime, Add, ExpandMore, PictureAsPdf, Delete } from '@mui/icons-material';
import { exportOrdenToPDF } from '../services/PDFExportService';
import { OrdenDeTrabajo } from '../types';
import {
    Box, Typography, Container, Paper, Grid, Card, CardContent, Chip, IconButton, Button, Tabs, Tab, TextField, List, ListItem, ListItemText, ListItemIcon, MenuItem,
    Accordion, AccordionSummary, AccordionDetails, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Dialog, DialogTitle, DialogContent, DialogActions
} from '@mui/material';

interface OrderDetailParams extends Record<string, string | undefined> {
    id: string;
}

const InfoCard = ({ icon, label, value }: { icon: React.ReactElement, label: string, value: string }) => (
    <Card elevation={2} sx={{ borderRadius: 2 }}>
        <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ p: 1.5, bgcolor: 'primary.main', borderRadius: 2, color: 'white', display: 'flex' }}>
                {icon}
            </Box>
            <Box>
                <Typography variant="caption" color="text.secondary">{label}</Typography>
                <Typography variant="h6" sx={{ fontSize: '1rem', fontWeight: 'bold' }}>{value}</Typography>
            </Box>
        </CardContent>
    </Card>
);

import { useAuthFetch } from '../hooks/useAuthFetch';

const OrderDetail = () => {
    const { id } = useParams<OrderDetailParams>();
    const navigate = useNavigate();
    const authFetch = useAuthFetch();
    const [order, setOrder] = useState<OrdenDeTrabajo & { detalles?: any[] } | null>(null);
    const [history, setHistory] = useState<any[]>([]);
    const [equipo, setEquipo] = useState<any | null>(null);
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState(0);
    const [newActivity, setNewActivity] = useState('');

    // New state for Corrective flow
    const [rutinaParts, setRutinaParts] = useState<any[]>([]);
    const [selectedPartId, setSelectedPartId] = useState('');

    // Failure info for corrective orders
    const [tiposFalla, setTiposFalla] = useState<any[]>([]);
    const [causasFalla, setCausasFalla] = useState<any[]>([]);
    const [selectedTipoFalla, setSelectedTipoFalla] = useState('');
    const [selectedCausaFalla, setSelectedCausaFalla] = useState('');

    // Delete confirmation state
    const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

    const fetchOrder = async () => {
        try {
            const resOrder = await authFetch(`/ordenes/${id}`);
            if (resOrder.ok) {
                const data = await resOrder.json();
                console.log("Order loaded:", data);
                setOrder(data);

                // Fetch Equipment details
                if (data.equipoId) {
                    try {
                        const resEq = await authFetch(`/equipos/${data.equipoId}`);
                        if (resEq.ok) {
                            const foundEq = await resEq.json();
                            console.log("Equipment loaded:", foundEq);
                            setEquipo(foundEq);

                            // Load Routine
                            // Priority 1: Use Routine ID from Order (Preventive/Corrective with specific routine)
                            console.log("Checking Routine ID:", data.rutinaId);
                            if (data.rutinaId) {
                                try {
                                    const resSingleRutina = await authFetch(`/rutinas/${data.rutinaId}`);
                                    if (resSingleRutina.ok) {
                                        const fullRoutine = await resSingleRutina.json();
                                        console.log("Full Routine loaded from ID:", fullRoutine);
                                        if (fullRoutine && fullRoutine.partes) {
                                            setRutinaParts(fullRoutine.partes);
                                        }
                                    } else {
                                        console.warn("Failed to load routine from ID", resSingleRutina.status);
                                    }
                                } catch (e) {
                                    console.error("Error loading routine from ID", e);
                                }
                            }
                            // Priority 2: Fallback to Equipment Routine string (Legacy/Implicit)
                            else if (foundEq && foundEq.rutina) {
                                console.log("Fallback to Equipment Routine string:", foundEq.rutina);
                                try {
                                    const resRutinas = await authFetch('/rutinas');
                                    if (resRutinas.ok) {
                                        const allRutinas = await resRutinas.json();
                                        console.log("All Rutinas loaded:", allRutinas);
                                        // Handle paginated response structure if needed (though endpoint returns list in .data usually)
                                        const rutinasList = allRutinas.data || allRutinas;

                                        const match = rutinasList.find((r: any) => r.descripcion === foundEq.rutina);
                                        console.log("Match found:", match);

                                        if (match) {
                                            const resSingleRutina = await authFetch(`/rutinas/${match.id}`);
                                            if (resSingleRutina.ok) {
                                                const fullRoutine = await resSingleRutina.json();
                                                console.log("Full Routine loaded from Description match:", fullRoutine);
                                                if (fullRoutine && fullRoutine.partes) {
                                                    setRutinaParts(fullRoutine.partes);
                                                }
                                            }
                                        }
                                    }
                                } catch (e) {
                                    console.error("Error loading routine parts from description", e);
                                }
                            } else {
                                console.log("No routine ID in order and no routine string in equipment.");
                            }
                        }
                    } catch (e) {
                        console.error("Error loading equipment details", e);
                    }
                }
            }

            const resHistory = await authFetch(`/ordenes/${id}/historial`);
            if (resHistory.ok) {
                const data = await resHistory.json();
                setHistory(data);
            }
        } catch (error) {
            console.error("Error fetching data", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (id) fetchOrder();
        // Fetch failure types and causes for corrective orders
        fetchFailureData();
    }, [id, authFetch]);

    const fetchFailureData = async () => {
        try {
            const [resTipos, resCausas] = await Promise.all([
                authFetch('/configuracion/fallas'),
                authFetch('/configuracion/causas-falla')
            ]);
            if (resTipos.ok) setTiposFalla(await resTipos.json());
            if (resCausas.ok) setCausasFalla(await resCausas.json());
        } catch (e) {
            console.error("Error loading failure data", e);
        }
    };

    const handleAddActivity = async () => {
        if (!newActivity || !id) return;
        if (rutinaParts.length > 0 && !selectedPartId) {
            alert("Debes seleccionar una parte del equipo");
            return;
        }

        let finalDescription = newActivity;
        if (selectedPartId) {
            const part = rutinaParts.find(p => p.id === selectedPartId);
            if (part) {
                finalDescription = `${part.descripcion}: ${newActivity}`;
            }
        }

        try {
            const res = await authFetch(`/ordenes/${id}/actividades`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    Descripcion: finalDescription,
                    FechaEstimada: new Date().toISOString(),
                    TipoFallaId: order?.tipo === 'Correctivo' ? selectedTipoFalla || null : null,
                    CausaFallaId: order?.tipo === 'Correctivo' ? selectedCausaFalla || null : null
                })
            });
            if (res.ok) {
                setNewActivity('');
                setSelectedPartId('');
                setSelectedTipoFalla('');
                setSelectedCausaFalla('');
                fetchOrder();
            } else {
                alert("Error al guardar actividad");
            }
        } catch (e) {
            console.error(e);
        }
    };

    const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
        setActiveTab(newValue);
    };

    const handleDelete = async () => {
        if (!id) return;

        try {
            const res = await authFetch(`/ordenes/${id}`, {
                method: 'DELETE'
            });

            if (res.ok) {
                navigate('/historial');
            } else {
                alert('Error al eliminar la orden');
            }
        } catch (error) {
            console.error(error);
            alert('Error de conexión');
        } finally {
            setShowDeleteConfirm(false);
        }
    };

    if (loading) return <Box sx={{ p: 4, display: 'flex', justifyContent: 'center' }}><Typography>Cargando detalle...</Typography></Box>;
    if (!order) return <Box sx={{ p: 4 }}><Typography>No se encontró la orden.</Typography></Box>;

    return (
        <Container maxWidth="xl" sx={{ mt: 4 }}>
            {/* Header */}
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 4 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <IconButton component={Link} to="/" color="inherit">
                        <ArrowBack />
                    </IconButton>
                    <Box>
                        <Typography variant="h4" component="h1" fontWeight="bold">
                            {order.numero} - {order.tipo}
                        </Typography>
                        <Typography variant="subtitle1" color="text.secondary">
                            Orden de Mantenimiento
                        </Typography>
                    </Box>
                </Box>
                <Chip
                    icon={<AccessTime />}
                    label={order.estado}
                    color="primary"
                    variant="outlined"
                    sx={{ px: 1, fontWeight: 'bold' }}
                />
                <Button
                    variant="contained"
                    color="primary"
                    startIcon={<PictureAsPdf />}
                    onClick={() => order && exportOrdenToPDF(order, equipo, history, tiposFalla, causasFalla)}
                    sx={{ ml: 2 }}
                >
                    Exportar PDF
                </Button>
                <Button
                    variant="contained"
                    color="warning"
                    startIcon={<Delete />}
                    onClick={() => setShowDeleteConfirm(true)}
                    sx={{ ml: 2 }}
                >
                    Eliminar OT
                </Button>
            </Box>

            {/* Info Cards */}
            <Grid container spacing={3} sx={{ mb: 4 }}>
                <Grid size={{ xs: 12, md: 4 }}>
                    <InfoCard icon={<LocalShipping />} label="Equipo" value={equipo ? `${equipo.placa} - ${equipo.descripcion}` : (order.equipoId || 'N/A')} />
                </Grid>
                <Grid size={{ xs: 12, md: 4 }}>
                    <InfoCard icon={<Person />} label="Responsable" value="Sin Asignar" />
                </Grid>
                <Grid size={{ xs: 12, md: 4 }}>
                    <InfoCard icon={<CalendarToday />} label="Fecha Creación" value={new Date().toLocaleDateString()} />
                </Grid>
            </Grid>

            {/* Content Tabs */}
            <Paper elevation={3} sx={{ borderRadius: 2 }}>
                <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                    <Tabs value={activeTab} onChange={handleTabChange} aria-label="order tabs">
                        <Tab label="Actividades" />
                        <Tab label="Auditoría" />
                    </Tabs>
                </Box>
                <Box sx={{ p: 3 }}>
                    {activeTab === 0 && (
                        <Box>
                            <Typography variant="h6" gutterBottom>Línea de Tiempo Operativa</Typography>

                            {/* Formulario Agregar Actividad */}
                            <Box sx={{ mb: 4, p: 2, bgcolor: 'background.paper', borderRadius: 2, border: 1, borderColor: 'divider' }}>
                                <Typography variant="subtitle2" gutterBottom sx={{ fontWeight: 'bold' }}>Nueva Actividad</Typography>
                                <Box sx={{ display: 'flex', gap: 2, flexDirection: 'column' }}>
                                    {rutinaParts.length > 0 && (
                                        <TextField
                                            select
                                            label="Parte del Equipo"
                                            fullWidth
                                            size="small"
                                            value={selectedPartId}
                                            onChange={(e) => setSelectedPartId(e.target.value)}
                                            helperText="Selecciona la parte afectada (Requerido)"
                                            required
                                        >
                                            {rutinaParts.map((p: any) => (
                                                <MenuItem key={p.id} value={p.id}>{p.descripcion}</MenuItem>
                                            ))}
                                        </TextField>
                                    )}

                                    {/* Failure info for corrective orders - BEFORE the add button */}
                                    {order?.tipo === 'Correctivo' && (
                                        <Box sx={{ display: 'flex', gap: 2 }}>
                                            <TextField
                                                select
                                                label="Tipo de Falla *"
                                                fullWidth
                                                size="small"
                                                value={selectedTipoFalla}
                                                onChange={(e) => setSelectedTipoFalla(e.target.value)}
                                                helperText="Requerido para OT Correctiva"
                                                required
                                                error={!selectedTipoFalla}
                                            >
                                                <MenuItem value=""><em>-- Seleccionar --</em></MenuItem>
                                                {tiposFalla.filter(t => t.activo).map((tipo: any) => (
                                                    <MenuItem key={tipo.codigo} value={tipo.codigo}>{tipo.descripcion}</MenuItem>
                                                ))}
                                            </TextField>
                                            <TextField
                                                select
                                                label="Causa de Falla *"
                                                fullWidth
                                                size="small"
                                                value={selectedCausaFalla}
                                                onChange={(e) => setSelectedCausaFalla(e.target.value)}
                                                helperText="Requerido para OT Correctiva"
                                                required
                                                error={!selectedCausaFalla}
                                            >
                                                <MenuItem value=""><em>-- Seleccionar --</em></MenuItem>
                                                {causasFalla.filter(c => c.activo).map((causa: any) => (
                                                    <MenuItem key={causa.codigo} value={causa.codigo}>{causa.descripcion}</MenuItem>
                                                ))}
                                            </TextField>
                                        </Box>
                                    )}

                                    <Box sx={{ display: 'flex', gap: 2 }}>
                                        <TextField
                                            fullWidth
                                            placeholder="Descripción de la actividad (ej. Cambio de Aceite)"
                                            value={newActivity}
                                            onChange={(e) => setNewActivity(e.target.value)}
                                            size="small"
                                        />
                                        <Button
                                            variant="contained"
                                            onClick={handleAddActivity}
                                            disabled={
                                                !newActivity ||
                                                (rutinaParts.length > 0 && !selectedPartId) ||
                                                (order?.tipo === 'Correctivo' && (!selectedTipoFalla || !selectedCausaFalla))
                                            }
                                            startIcon={<Add />}
                                            sx={{ minWidth: 120 }}
                                        >
                                            Agregar
                                        </Button>
                                    </Box>
                                </Box>
                            </Box>

                            <Box sx={{ mt: 3 }}>
                                {order.detalles && order.detalles.length > 0 ? (
                                    Object.entries(
                                        order.detalles.reduce((acc: any, curr: any) => {
                                            const parts = curr.descripcion.split(': ');
                                            const groupName = parts.length > 1 ? parts[0] : 'General';
                                            const activityName = parts.length > 1 ? parts.slice(1).join(': ') : curr.descripcion;

                                            if (!acc[groupName]) acc[groupName] = [];
                                            acc[groupName].push({ ...curr, displayDescription: activityName });
                                            return acc;
                                        }, {})
                                    ).map(([group, filteredActivities]: [string, any]) => (
                                        <Accordion key={group} defaultExpanded disableGutters elevation={1} sx={{ mb: 1, '&:before': { display: 'none' }, borderRadius: 1 }}>
                                            <AccordionSummary expandIcon={<ExpandMore />} sx={{ bgcolor: 'action.hover', fontWeight: 'bold' }}>
                                                <Typography sx={{ display: 'flex', alignItems: 'center', width: '100%' }}>
                                                    {group}
                                                    <Chip
                                                        label={filteredActivities.length}
                                                        size="small"
                                                        sx={{ ml: 2, height: 20, minWidth: 20 }}
                                                    />
                                                </Typography>
                                            </AccordionSummary>
                                            <AccordionDetails sx={{ p: 0 }}>
                                                <TableContainer component={Paper} elevation={0} sx={{ borderRadius: 0 }}>
                                                    <Table size="small">
                                                        <TableHead>
                                                            <TableRow sx={{ backgroundColor: '#e0e0e0' }}>
                                                                <TableCell sx={{ fontWeight: 'bold', color: 'black !important' }}>Actividad</TableCell>
                                                                {order.tipo === 'Preventivo' && <TableCell sx={{ width: 120, fontWeight: 'bold', color: 'black !important' }}>Frecuencia</TableCell>}
                                                                <TableCell sx={{ width: 150, fontWeight: 'bold', color: 'black !important' }}>Estado</TableCell>
                                                                <TableCell align="right" sx={{ width: 100, fontWeight: 'bold', color: 'black !important' }}>Avance</TableCell>
                                                            </TableRow>
                                                        </TableHead>
                                                        <TableBody>
                                                            {filteredActivities.map((d: any, i: number) => (
                                                                <TableRow key={i} hover>
                                                                    <TableCell>
                                                                        <Typography variant="body2" color="text.primary">{d.displayDescription}</Typography>
                                                                    </TableCell>
                                                                    {order.tipo === 'Preventivo' && (
                                                                        <TableCell>
                                                                            {d.frecuencia > 0 ? (
                                                                                <Chip
                                                                                    label={`${d.frecuencia}h`}
                                                                                    size="small"
                                                                                    color="info"
                                                                                    variant="outlined"
                                                                                    sx={{ height: 20, fontSize: '0.7rem' }}
                                                                                />
                                                                            ) : '-'}
                                                                        </TableCell>
                                                                    )}
                                                                    <TableCell>
                                                                        <Chip
                                                                            label={d.estado}
                                                                            size="small"
                                                                            color={d.estado === 'Completada' ? 'success' : 'default'}
                                                                            variant="outlined"
                                                                            sx={{ fontSize: '0.7rem', height: 20 }}
                                                                        />
                                                                    </TableCell>
                                                                    <TableCell align="right">
                                                                        <Typography variant="body2" fontWeight="bold" color="text.secondary">
                                                                            {d.avance}%
                                                                        </Typography>
                                                                    </TableCell>
                                                                </TableRow>
                                                            ))}
                                                        </TableBody>
                                                    </Table>
                                                </TableContainer>
                                            </AccordionDetails>
                                        </Accordion>
                                    ))
                                ) : (
                                    <Paper sx={{ p: 3, textAlign: 'center', bgcolor: 'grey.50' }}>
                                        <Typography color="text.secondary">No hay actividades registradas aún.</Typography>
                                    </Paper>
                                )}
                            </Box>
                        </Box>
                    )}

                    {activeTab === 1 && (
                        <Box>
                            <Typography variant="h6" gutterBottom>Registro de Eventos (Técnico)</Typography>
                            <List>
                                {history.length > 0 ? (
                                    history.map((event, i) => (
                                        <ListItem key={event.id || i} sx={{ borderLeft: 4, borderColor: 'grey.500', bgcolor: 'action.hover', borderRadius: 1, mb: 1 }}>
                                            <ListItemIcon>
                                                <AccessTime fontSize="small" />
                                            </ListItemIcon>
                                            <ListItemText
                                                primary={<Typography color="primary" variant="subtitle2" fontWeight="bold">{event.tipo}</Typography>}
                                                secondary={
                                                    <>
                                                        <Typography variant="caption" display="block" color="text.secondary">{new Date(event.fecha).toLocaleString()}</Typography>
                                                        <Typography variant="body2">
                                                            {event.tipo === 'OrdenDeTrabajoCreada' && equipo
                                                                ? `Orden creada. Equipo: ${equipo.placa} - ${equipo.descripcion}. Tipo: ${order?.tipo}`
                                                                : event.descripcion}
                                                            {event.datos?.usuarioNombre && (
                                                                <Box component="span" sx={{ fontWeight: 'bold', ml: 1 }}>
                                                                    - Por: {event.datos.usuarioNombre}
                                                                </Box>
                                                            )}
                                                        </Typography>
                                                    </>
                                                }
                                            />
                                        </ListItem>
                                    ))
                                ) : (
                                    <Typography color="text.secondary">No hay historial disponible.</Typography>
                                )}
                            </List>
                        </Box>
                    )}
                </Box>
            </Paper>

            {/* Delete Confirmation Dialog */}
            <Dialog open={showDeleteConfirm} onClose={() => setShowDeleteConfirm(false)}>
                <DialogTitle>Confirmar Eliminación</DialogTitle>
                <DialogContent>
                    <Typography>
                        ¿Está seguro que desea eliminar la orden {order.numero}?
                        Esta acción no se puede deshacer.
                    </Typography>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setShowDeleteConfirm(false)}>
                        Cancelar
                    </Button>
                    <Button onClick={handleDelete} color="warning" variant="contained">
                        Eliminar
                    </Button>
                </DialogActions>
            </Dialog>
        </Container>
    );
};

export default OrderDetail;
