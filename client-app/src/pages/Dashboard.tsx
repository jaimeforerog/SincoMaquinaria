import React, { useEffect, useState } from 'react';
import { LocalShipping, Warning, CheckCircle, Add, Engineering, Agriculture, Assignment } from '@mui/icons-material';
import { Link, useNavigate } from 'react-router-dom';

import { OrdenDeTrabajo } from '../types';
import { useAuthFetch } from '../hooks/useAuthFetch';
import {
    Box, Grid, Card, CardContent, Typography, Button,
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Chip, CircularProgress
} from '@mui/material';

import { useDashboardSocket } from '../hooks/useDashboardSocket';

// ... imports ...

const Dashboard = () => {
    const authFetch = useAuthFetch();
    const navigate = useNavigate();
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

    const fetchData = React.useCallback(async () => {
        // Don't set loading to true here to avoid flickering on updates
        // or set it if you want to show progress
        try {
            // Fetch Ordenes
            const resOrdenes = await authFetch('/ordenes').catch(() => null);
            if (resOrdenes && resOrdenes.ok) {
                const response = await resOrdenes.json();
                // Handle both paginated and non-paginated responses
                setOrdenes(response.data || response);
            }

            // Fetch Equipos
            const resEquipos = await authFetch('/equipos').catch(() => null);
            if (resEquipos && resEquipos.ok) {
                const response = await resEquipos.json();
                const data = response.data || response;
                setEquiposList(data);
                setEquiposCount(response.totalCount ?? data.length);
            }

            // Fetch Rutinas
            const resRutinas = await authFetch('/rutinas').catch(() => null);
            if (resRutinas && resRutinas.ok) {
                const response = await resRutinas.json();
                const data = response.data || response;
                setRutinasCount(response.totalCount ?? data.length);
            }

        } catch (error) {
            console.error("Error fetching dashboard data", error);
        } finally {
            setLoading(false);
        }
    }, [authFetch]);

    useEffect(() => {
        setLoading(true);
        fetchData();
    }, [fetchData]);

    // Listen for real-time updates
    useDashboardSocket(() => {
        console.log("Recibida actualización en tiempo real. Recargando datos...");
        fetchData();
    });

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
                    <Box component={Link} to="/gestion-equipos" sx={{ textDecoration: 'none' }}>
                        <KpiCard
                            title="Equipos"
                            value={equiposCount}
                            icon={<Agriculture fontSize="large" sx={{ color: '#2e7d32' }} />}
                            loading={loading}
                        />
                    </Box>
                </Grid>

                <Grid item xs={12} sm={6} md={2}>
                    <Box component={Link} to="/editar-rutinas" sx={{ textDecoration: 'none' }}>
                        <KpiCard
                            icon={<Engineering sx={{ fontSize: 32, color: "#78909c" }} />} // Blue Grey
                            title="Rutinas"
                            value={rutinasCount}
                            change="Definidas"
                            loading={loading}
                        />
                    </Box>
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                    <Box component={Link} to="/historial" sx={{ textDecoration: 'none' }}>
                        <KpiCard
                            icon={<Assignment sx={{ fontSize: 32, color: "#90caf9" }} />} // MUI Blue 200
                            title="Órdenes Activas"
                            value={activeOrders}
                            change="Total Registrado"
                            loading={loading}
                        />
                    </Box>
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
