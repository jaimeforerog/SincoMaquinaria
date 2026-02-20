import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Box, Typography, Container, Paper, Tabs, Tab } from '@mui/material';
import { useAuthFetch } from '../hooks/useAuthFetch';
import { OrdenDeTrabajo, Equipo, Parte, TipoFalla, CausaFalla, HistorialEvent } from '../types';
import { useNotification } from '../contexts/NotificationContext';
import OrderHeader from '../components/ordenes/OrderHeader';
import OrderInfoCards from '../components/ordenes/OrderInfoCards';
import ActivityForm from '../components/ordenes/ActivityForm';
import ActivitiesTab from '../components/ordenes/ActivitiesTab';
import AuditTab from '../components/ordenes/AuditTab';
import DeleteOrderDialog from '../components/ordenes/DeleteOrderDialog';

interface OrderDetailParams extends Record<string, string | undefined> {
    id: string;
}

const OrderDetail = () => {
    const { id } = useParams<OrderDetailParams>();
    const navigate = useNavigate();
    const authFetch = useAuthFetch();
    const { showNotification } = useNotification();
    const [order, setOrder] = useState<OrdenDeTrabajo | null>(null);
    const [history, setHistory] = useState<HistorialEvent[]>([]);
    const [equipo, setEquipo] = useState<Equipo | null>(null);
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState(0);

    // Corrective flow state
    const [rutinaParts, setRutinaParts] = useState<Parte[]>([]);
    const [tiposFalla, setTiposFalla] = useState<TipoFalla[]>([]);
    const [causasFalla, setCausasFalla] = useState<CausaFalla[]>([]);

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
                            // Fallback to Equipment Routine string
                            else if (foundEq && foundEq.rutina) {
                                console.log("Fallback to Equipment Routine string:", foundEq.rutina);
                                try {
                                    const resRutinas = await authFetch('/rutinas');
                                    if (resRutinas.ok) {
                                        const allRutinas = await resRutinas.json();
                                        const rutinasList = allRutinas.data || allRutinas;
                                        const match = rutinasList.find((r: { id: string; descripcion: string }) => r.descripcion === foundEq.rutina);

                                        if (match) {
                                            const resSingleRutina = await authFetch(`/rutinas/${match.id}`);
                                            if (resSingleRutina.ok) {
                                                const fullRoutine = await resSingleRutina.json();
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

    const handleAddActivity = async (data: { description: string; partId: string; tipoFalla: string; causaFalla: string }) => {
        if (!data.description || !id) return;
        if (rutinaParts.length > 0 && !data.partId) {
            showNotification("Debes seleccionar una parte del equipo", "warning");
            return;
        }

        let finalDescription = data.description;
        if (data.partId) {
            const part = rutinaParts.find(p => p.id === data.partId);
            if (part) {
                finalDescription = `${part.descripcion}: ${data.description}`;
            }
        }

        try {
            const res = await authFetch(`/ordenes/${id}/actividades`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    Descripcion: finalDescription,
                    FechaEstimada: new Date().toISOString(),
                    TipoFallaId: order?.tipo === 'Correctivo' ? data.tipoFalla || null : null,
                    CausaFallaId: order?.tipo === 'Correctivo' ? data.causaFalla || null : null
                })
            });
            if (res.ok) {
                fetchOrder();
            } else {
                showNotification("Error al guardar actividad", "error");
            }
        } catch (e) {
            console.error(e);
        }
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
                showNotification('Error al eliminar la orden', 'error');
            }
        } catch (error) {
            console.error(error);
            showNotification('Error de conexión', 'error');
        } finally {
            setShowDeleteConfirm(false);
        }
    };

    if (loading) return <Box sx={{ p: 4, display: 'flex', justifyContent: 'center' }}><Typography>Cargando detalle...</Typography></Box>;
    if (!order) return <Box sx={{ p: 4 }}><Typography>No se encontró la orden.</Typography></Box>;

    return (
        <Container maxWidth="xl" sx={{ mt: 4 }}>
            <OrderHeader
                order={order}
                equipo={equipo}
                history={history}
                tiposFalla={tiposFalla}
                causasFalla={causasFalla}
                onDelete={() => setShowDeleteConfirm(true)}
            />

            <OrderInfoCards order={order} equipo={equipo} />

            {/* Content Tabs */}
            <Paper elevation={0} sx={{ borderRadius: 2, border: 1, borderColor: 'divider' }}>
                <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                    <Tabs
                        value={activeTab}
                        onChange={(_, newValue) => setActiveTab(newValue)}
                        aria-label="order tabs"
                        sx={{
                            '& .MuiTab-root': { fontWeight: 600, textTransform: 'none', fontSize: '0.95rem' },
                        }}
                    >
                        <Tab label="Actividades" />
                        <Tab label="Auditoría" />
                    </Tabs>
                </Box>
                <Box sx={{ p: 3 }}>
                    {activeTab === 0 && (
                        <Box>
                            <Typography variant="h6" gutterBottom color="text.primary">Línea de Tiempo Operativa</Typography>
                            <ActivityForm
                                order={order}
                                rutinaParts={rutinaParts}
                                tiposFalla={tiposFalla}
                                causasFalla={causasFalla}
                                onSubmit={handleAddActivity}
                            />
                            <Box sx={{ mt: 3 }}>
                                <ActivitiesTab order={order} />
                            </Box>
                        </Box>
                    )}

                    {activeTab === 1 && (
                        <AuditTab history={history} equipo={equipo} order={order} />
                    )}
                </Box>
            </Paper>

            <DeleteOrderDialog
                open={showDeleteConfirm}
                orderNumber={order.numero}
                onClose={() => setShowDeleteConfirm(false)}
                onConfirm={handleDelete}
            />
        </Container>
    );
};

export default OrderDetail;
