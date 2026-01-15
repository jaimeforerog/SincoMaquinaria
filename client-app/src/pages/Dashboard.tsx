import React, { useEffect, useState } from 'react';
import { Engineering, Agriculture, Assignment } from '@mui/icons-material';
import { Link } from 'react-router-dom';

import { useAuthFetch } from '../hooks/useAuthFetch';
import {
    Box, Grid, Card, CardContent, Typography, CircularProgress
} from '@mui/material';

import { useDashboardSocket } from '../hooks/useDashboardSocket';

// ... imports ...

const Dashboard = () => {
    const authFetch = useAuthFetch();
    const [activeOrders, setActiveOrders] = useState(0);
    const [equiposCount, setEquiposCount] = useState(0);
    const [rutinasCount, setRutinasCount] = useState(0);
    const [loading, setLoading] = useState(true);

    const fetchData = React.useCallback(async () => {
        try {
            const res = await authFetch('/dashboard/stats');
            if (res.ok) {
                const stats = await res.json();
                setEquiposCount(stats.equiposCount);
                setRutinasCount(stats.rutinasCount);
                setActiveOrders(stats.ordenesActivasCount);
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
