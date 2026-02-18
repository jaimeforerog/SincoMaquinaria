import { Box, Typography, List, ListItem, ListItemIcon, ListItemText } from '@mui/material';
import { AccessTime } from '@mui/icons-material';
import { OrdenDeTrabajo } from '../../types';

interface AuditTabProps {
    history: any[];
    equipo: any | null;
    order: OrdenDeTrabajo;
}

const AuditTab = ({ history, equipo, order }: AuditTabProps) => (
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
);

export default AuditTab;
