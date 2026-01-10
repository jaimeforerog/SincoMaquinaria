import React, { useEffect, useState } from 'react';
import { LocalShipping, Warning, CheckCircle, Add, Agriculture, Engineering, Build } from '@mui/icons-material';
import { Link } from 'react-router-dom';

import { OrdenDeTrabajo } from '../types';
import {
    Box, Grid, Card, CardContent, Typography, Button,
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Chip, CircularProgress
} from '@mui/material';

const Dashboard = () => {
    const [ordenes, setOrdenes] = useState<OrdenDeTrabajo[]>([]);
    const [equiposList, setEquiposList] = useState<any[]>([]); // Store full list
    const [equiposCount, setEquiposCount] = useState(0);
    const [rutinasCount, setRutinasCount] = useState(0);
    const [loading, setLoading] = useState(true);

    // Helper to find equipment details
    const getEquipoDetails = (id: string) => {
        // Try finding in fetched list first
        const eq = equiposList.find((e: any) => e.id === id);
        if (eq) {
            return (
                <span>
                    <strong>{eq.placa}</strong> - {eq.descripcion}
                </span>
            );
        }
        return id;
    };

    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            try {
                // Fetch Ordenes
                const resOrdenes = await fetch('/ordenes').catch(() => null);
                if (resOrdenes && resOrdenes.ok) {
                    setOrdenes(await resOrdenes.json());
                }

                // Fetch Equipos
                const resEquipos = await fetch('/equipos').catch(() => null);
                if (resEquipos && resEquipos.ok) {
                    const data = await resEquipos.json();
                    setEquiposList(data);
                    setEquiposCount(data.length);
                }

                // Fetch Rutinas
                const resRutinas = await fetch('/rutinas').catch(() => null);
                if (resRutinas && resRutinas.ok) {
                    const data = await resRutinas.json();
                    setRutinasCount(data.length);
                }

            } catch (error) {
                console.error("Error fetching dashboard data", error);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, []);

    const activeOrders = ordenes.length;
    const pendingOrders = ordenes.filter(o => o.estado === 'Borrador' || o.estado === 'Programada').length;
    const completedOrders = ordenes.filter(o => o.estado === 'Finalizada').length;

    return (
        <Box sx={{ flexGrow: 1 }}>
            {/* Header */}
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
                <Box>
                    <Typography variant="h4" component="h1" fontWeight="bold">
                        Dashboard Operativo
                    </Typography>
                    <Typography variant="subtitle1" color="text.secondary">
                        Centro de control de Maquinaria Amarilla
                    </Typography>
                </Box>
                <Button
                    component={Link}
                    to="/nueva-orden"
                    variant="contained"
                    startIcon={<Add />}
                    sx={{ fontWeight: 'bold' }}
                >
                    Crear Orden
                </Button>
            </Box>

            {/* KPIs */}
            <Grid container spacing={3} sx={{ mb: 4 }}>
                <Grid item xs={12} sm={6} md={3}>
                    <Box component={Link} to="/equipos" sx={{ textDecoration: 'none' }}>
                        <KpiCard
                            title="Equipos"
                            value={equiposCount}
                            icon={<Build fontSize="large" sx={{ color: '#2e7d32' }} />}
                            loading={loading}
                        />
                    </Box>
                </Grid>

                <Grid item xs={12} sm={6} md={2}>
                    <KpiCard
                        icon={<Engineering sx={{ fontSize: 32, color: "#78909c" }} />} // Blue Grey
                        title="Rutinas"
                        value={rutinasCount}
                        change="Definidas"
                        loading={loading}
                    />
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                    <KpiCard
                        icon={<LocalShipping sx={{ fontSize: 32, color: "#90caf9" }} />} // MUI Blue 200
                        title="Órdenes Activas"
                        value={activeOrders}
                        change="Total Registrado"
                        loading={loading}
                    />
                </Grid>
                <Grid item xs={12} sm={6} md={2}>
                    <KpiCard
                        icon={<Warning sx={{ fontSize: 32, color: "#ffb74d" }} />} // Orange
                        title="Pendientes"
                        value={pendingOrders}
                        change="Requieren Acción"
                        loading={loading}
                    />
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                    <KpiCard
                        icon={<CheckCircle sx={{ fontSize: 32, color: "#66bb6a" }} />} // Green
                        title="Finalizadas"
                        value={completedOrders}
                        change="Histórico"
                        loading={loading}
                    />
                </Grid>
            </Grid>

            {/* Listado de Órdenes */}
            <Typography variant="overline" display="block" color="text.secondary" sx={{ mb: 2, fontSize: '0.85rem' }}>
                Órdenes Recientes
            </Typography>

            <TableContainer component={Paper} elevation={2} sx={{ borderRadius: 2 }}>
                {loading ? (
                    <Box sx={{ p: 4, textAlign: 'center' }}>
                        <Typography>Cargando datos...</Typography>
                    </Box>
                ) : ordenes.length === 0 ? (
                    <Box sx={{ p: 4, textAlign: 'center' }}>
                        <Typography color="text.secondary">No hay órdenes registradas.</Typography>
                    </Box>
                ) : (
                    <Table sx={{ minWidth: 650 }} aria-label="ordenes table">
                        <TableHead sx={{ bgcolor: 'action.hover' }}>
                            <TableRow>
                                <TableCell><strong>Número</strong></TableCell>
                                <TableCell><strong>Equipo</strong></TableCell>
                                <TableCell><strong>Tipo</strong></TableCell>
                                <TableCell><strong>Estado</strong></TableCell>
                                <TableCell align="center"><strong>Acción</strong></TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {ordenes.map((orden) => (
                                <TableRow
                                    key={orden.id}
                                    sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                    hover
                                >
                                    <TableCell component="th" scope="row" sx={{ fontWeight: 'bold' }}>
                                        {orden.numero}
                                    </TableCell>
                                    <TableCell>{getEquipoDetails(orden.equipoId)}</TableCell>
                                    <TableCell>{orden.tipo || 'N/A'}</TableCell>
                                    <TableCell>
                                        <Chip
                                            label={orden.estado}
                                            color={orden.estado === 'Finalizada' ? 'success' : orden.estado === 'Cancelada' ? 'error' : 'secondary'}
                                            variant="outlined"
                                            size="small"
                                        />
                                    </TableCell>
                                    <TableCell align="center">
                                        <Button
                                            component={Link}
                                            to={`/ordenes/${orden.id}`}
                                            size="small"
                                            sx={{ textTransform: 'none' }}
                                        >
                                            Ver Detalle
                                        </Button>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                )}
            </TableContainer>
        </Box>
    );
};

interface KpiCardProps {
    icon: React.ReactElement;
    title: string;
    value: number | string;
    change?: string;
    loading?: boolean;
}

const KpiCard: React.FC<KpiCardProps> = ({ icon, title, value, change, loading = false }) => (
    <Card elevation={3} sx={{ borderRadius: 2, height: '100%' }}>
        <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                <Box sx={{
                    p: 1.5,
                    bgcolor: 'action.selected',
                    borderRadius: 3,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center'
                }}>
                    {icon}
                </Box>

                {loading ? (
                    <CircularProgress size={24} sx={{ mt: 1, mr: 1 }} />
                ) : (
                    <Typography variant="h3" fontWeight="bold">
                        {value}
                    </Typography>
                )}

            </Box>
            <Typography variant="subtitle1" component="div" color="text.secondary">
                {title}
            </Typography>
            {change && (
                <Typography variant="caption" color="text.disabled">
                    {change}
                </Typography>
            )}
        </CardContent>
    </Card>
);

export default Dashboard;
