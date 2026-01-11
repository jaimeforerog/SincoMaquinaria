import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowBack, Add } from '@mui/icons-material';

import { OrdenDeTrabajo } from '../types';
import {
    Box, Typography, Container, Paper, TableContainer, Table, TableHead, TableRow, TableCell, TableBody, Chip, IconButton, Button
} from '@mui/material';

import { useAuthFetch } from '../hooks/useAuthFetch';

const History = () => {
    const authFetch = useAuthFetch();
    const [ordenes, setOrdenes] = useState<OrdenDeTrabajo[]>([]);
    const [equiposList, setEquiposList] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    const getEquipoDetails = (id: string) => {
        const eq = equiposList.find((e: any) => e.id === id);
        return eq ? (
            <span>
                <strong>{eq.placa}</strong> - {eq.descripcion}
            </span>
        ) : id;
    };

    const formatDate = (dateString?: string) => {
        if (!dateString) return 'N/A';
        const date = new Date(dateString);
        if (isNaN(date.getTime()) || date.getFullYear() < 2024) {
            return 'N/A';
        }
        return date.toLocaleDateString();
    };

    useEffect(() => {
        const fetchData = async () => {
            try {
                const [resOrdenes, resEquipos] = await Promise.all([
                    authFetch('/ordenes'),
                    authFetch('/equipos')
                ]);

                if (resOrdenes.ok) {
                    const response = await resOrdenes.json();
                    setOrdenes(response.data || response);
                }
                if (resEquipos.ok) {
                    const response = await resEquipos.json();
                    setEquiposList(response.data || response);
                }
            } catch (error) {
                console.error("Error fetching data", error);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [authFetch]);

    return (
        <Container maxWidth="xl">
            <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
                <IconButton component={Link} to="/" color="inherit">
                    <ArrowBack />
                </IconButton>
                <Box>
                    <Typography variant="h4" component="h1" fontWeight="bold">
                        Historial de Mantenimiento
                    </Typography>
                    <Typography variant="subtitle1" color="text.secondary">
                        Registro completo de órdenes de trabajo
                    </Typography>
                </Box>
                <Box sx={{ ml: 'auto' }}>
                    <Button
                        component={Link}
                        to="/nueva-orden"
                        variant="contained"
                        startIcon={<Add />}
                    >
                        Nueva Orden
                    </Button>
                </Box>
            </Box>

            <TableContainer component={Paper} elevation={3} sx={{ borderRadius: 2 }}>
                {loading ? (
                    <Box sx={{ p: 4, textAlign: 'center' }}>
                        <Typography>Cargando historial...</Typography>
                    </Box>
                ) : ordenes.length === 0 ? (
                    <Box sx={{ p: 4, textAlign: 'center' }}>
                        <Typography color="text.secondary">No hay historial disponible.</Typography>
                    </Box>
                ) : (
                    <Table sx={{ minWidth: 650 }} aria-label="history table">
                        <TableHead sx={{ bgcolor: 'action.hover' }}>
                            <TableRow>
                                <TableCell><strong>Fecha</strong></TableCell>
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
                                    <TableCell sx={{ color: 'text.secondary', whiteSpace: 'nowrap' }}>
                                        {formatDate(orden.fechaCreacion)}
                                    </TableCell>
                                    <TableCell sx={{ fontWeight: 'bold' }}>
                                        {orden.numero}
                                    </TableCell>
                                    <TableCell>{getEquipoDetails(orden.equipoId)}</TableCell>
                                    <TableCell>{orden.tipo || 'N/A'}</TableCell>
                                    <TableCell>
                                        <Chip
                                            label={orden.estado}
                                            color={orden.estado === 'Finalizada' ? 'success' : 'secondary'}
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
        </Container>
    );
};

export default History;
