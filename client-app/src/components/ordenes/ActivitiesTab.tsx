import {
    Box, Typography, Chip, Paper, alpha, useTheme,
    Accordion, AccordionSummary, AccordionDetails,
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow
} from '@mui/material';
import { ExpandMore } from '@mui/icons-material';
import { OrdenDeTrabajo, DetalleOrden } from '../../types';

interface ActivitiesTabProps {
    order: OrdenDeTrabajo;
}

const ActivitiesTab = ({ order }: ActivitiesTabProps) => {
    const theme = useTheme();

    if (!order.detalles || order.detalles.length === 0) {
        return (
            <Paper sx={{ p: 3, textAlign: 'center', bgcolor: 'action.hover', border: 1, borderColor: 'divider' }} elevation={0}>
                <Typography color="text.secondary">No hay actividades registradas aún.</Typography>
            </Paper>
        );
    }

    const grouped = order.detalles.reduce((acc: Record<string, (DetalleOrden & { displayDescription: string })[]>, curr: DetalleOrden) => {
        const parts = curr.descripcion.split(': ');
        const groupName = parts.length > 1 ? parts[0] : 'General';
        const activityName = parts.length > 1 ? parts.slice(1).join(': ') : curr.descripcion;

        if (!acc[groupName]) acc[groupName] = [];
        acc[groupName].push({ ...curr, displayDescription: activityName });
        return acc;
    }, {});

    return (
        <Box>
            {Object.entries(grouped).map(([group, filteredActivities]) => (
                <Accordion
                    key={group}
                    defaultExpanded
                    disableGutters
                    elevation={0}
                    sx={{
                        mb: 1.5,
                        '&:before': { display: 'none' },
                        borderRadius: 2,
                        border: 1,
                        borderColor: 'divider',
                        overflow: 'hidden',
                    }}
                >
                    <AccordionSummary
                        expandIcon={<ExpandMore sx={{ color: 'text.secondary' }} />}
                        sx={{
                            bgcolor: alpha(theme.palette.primary.main, 0.08),
                        }}
                    >
                        <Typography sx={{ display: 'flex', alignItems: 'center', width: '100%', fontWeight: 600, color: 'text.primary' }}>
                            {group}
                            <Chip
                                label={filteredActivities.length}
                                size="small"
                                color="primary"
                                sx={{ ml: 2, height: 22, minWidth: 22, fontWeight: 700, fontSize: '0.75rem' }}
                            />
                        </Typography>
                    </AccordionSummary>
                    <AccordionDetails sx={{ p: 0 }}>
                        <TableContainer component={Paper} elevation={0} sx={{ borderRadius: 0 }}>
                            <Table size="small">
                                <TableHead>
                                    <TableRow sx={{ bgcolor: alpha(theme.palette.primary.main, 0.04) }}>
                                        <TableCell sx={{ fontWeight: 'bold', color: 'text.secondary', borderBottomColor: 'divider' }}>Actividad</TableCell>
                                        {order.tipo === 'Preventivo' && <TableCell sx={{ width: 120, fontWeight: 'bold', color: 'text.secondary', borderBottomColor: 'divider' }}>Frecuencia</TableCell>}
                                        <TableCell sx={{ width: 150, fontWeight: 'bold', color: 'text.secondary', borderBottomColor: 'divider' }}>Estado</TableCell>
                                        <TableCell align="right" sx={{ width: 100, fontWeight: 'bold', color: 'text.secondary', borderBottomColor: 'divider' }}>Avance</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {filteredActivities.map((d, i) => (
                                        <TableRow key={i} hover sx={{ '&:last-child td': { borderBottom: 0 } }}>
                                            <TableCell>
                                                <Typography variant="body2" color="text.primary">{d.displayDescription}</Typography>
                                            </TableCell>
                                            {order.tipo === 'Preventivo' && (
                                                <TableCell>
                                                    {(d.frecuencia ?? 0) > 0 ? (
                                                        <Chip
                                                            label={`${d.frecuencia}h`}
                                                            size="small"
                                                            color="info"
                                                            variant="outlined"
                                                            sx={{ height: 22, fontSize: '0.7rem' }}
                                                        />
                                                    ) : (
                                                        <Typography variant="body2" color="text.secondary">-</Typography>
                                                    )}
                                                </TableCell>
                                            )}
                                            <TableCell>
                                                <Chip
                                                    label={d.estado}
                                                    size="small"
                                                    color={d.estado === 'Completada' ? 'success' : 'default'}
                                                    sx={{ fontSize: '0.7rem', height: 22 }}
                                                />
                                            </TableCell>
                                            <TableCell align="right">
                                                <Typography variant="body2" fontWeight="bold" color="text.primary">
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
            ))}
        </Box>
    );
};

export default ActivitiesTab;
