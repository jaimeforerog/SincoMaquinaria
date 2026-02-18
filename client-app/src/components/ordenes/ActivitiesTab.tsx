import {
    Box, Typography, Chip, Paper,
    Accordion, AccordionSummary, AccordionDetails,
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow
} from '@mui/material';
import { ExpandMore } from '@mui/icons-material';
import { OrdenDeTrabajo } from '../../types';

interface ActivitiesTabProps {
    order: OrdenDeTrabajo & { detalles?: any[] };
}

const ActivitiesTab = ({ order }: ActivitiesTabProps) => {
    if (!order.detalles || order.detalles.length === 0) {
        return (
            <Paper sx={{ p: 3, textAlign: 'center', bgcolor: 'grey.50' }}>
                <Typography color="text.secondary">No hay actividades registradas aún.</Typography>
            </Paper>
        );
    }

    const grouped = order.detalles.reduce((acc: any, curr: any) => {
        const parts = curr.descripcion.split(': ');
        const groupName = parts.length > 1 ? parts[0] : 'General';
        const activityName = parts.length > 1 ? parts.slice(1).join(': ') : curr.descripcion;

        if (!acc[groupName]) acc[groupName] = [];
        acc[groupName].push({ ...curr, displayDescription: activityName });
        return acc;
    }, {});

    return (
        <Box>
            {Object.entries(grouped).map(([group, filteredActivities]: [string, any]) => (
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
            ))}
        </Box>
    );
};

export default ActivitiesTab;
