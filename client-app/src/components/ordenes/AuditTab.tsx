import { Box, Typography, List, ListItem, ListItemIcon, ListItemText, alpha, useTheme } from '@mui/material';
import { AccessTime } from '@mui/icons-material';
import { OrdenDeTrabajo, Equipo, HistorialEvent } from '../../types';

interface AuditTabProps {
    history: HistorialEvent[];
    equipo: Equipo | null;
    order: OrdenDeTrabajo;
}

const AuditTab = ({ history, equipo, order }: AuditTabProps) => {
    const theme = useTheme();
    return (
        <Box>
            <Typography variant="h6" gutterBottom color="text.primary">Registro de Eventos (Técnico)</Typography>
            <List>
                {history.length > 0 ? (
                    history.map((event, i) => (
                        <ListItem
                            key={event.id || i}
                            sx={{
                                borderLeft: 4,
                                borderColor: 'primary.main',
                                bgcolor: alpha(theme.palette.primary.main, 0.04),
                                borderRadius: 1,
                                mb: 1,
                                '&:hover': { bgcolor: alpha(theme.palette.primary.main, 0.08) },
                            }}
                        >
                            <ListItemIcon>
                                <AccessTime fontSize="small" sx={{ color: 'primary.main' }} />
                            </ListItemIcon>
                            <ListItemText
                                primary={<Typography color="primary" variant="subtitle2" fontWeight="bold">{event.tipo}</Typography>}
                                secondary={
                                    <>
                                        <Typography variant="caption" display="block" color="text.secondary">{new Date(event.fecha).toLocaleString()}</Typography>
                                        <Typography variant="body2" color="text.primary">
                                            {event.tipo === 'OrdenDeTrabajoCreada' && equipo
                                                ? `Orden creada. Equipo: ${equipo.placa} - ${equipo.descripcion}. Tipo: ${order?.tipo}`
                                                : event.descripcion}
                                            {typeof event.datos?.usuarioNombre === 'string' && (
                                                <Box component="span" sx={{ fontWeight: 'bold', ml: 1, color: 'primary.main' }}>
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
    );
};

export default AuditTab;
