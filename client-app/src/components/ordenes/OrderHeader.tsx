import { Box, Typography, IconButton, Chip, Button, Paper } from '@mui/material';
import { ArrowBack, AccessTime, PictureAsPdf, Delete } from '@mui/icons-material';
import { Link } from 'react-router-dom';
import { OrdenDeTrabajo, Equipo, HistorialEvent, TipoFalla, CausaFalla } from '../../types';
import { exportOrdenToPDF } from '../../services/PDFExportService';

interface OrderHeaderProps {
    order: OrdenDeTrabajo;
    equipo: Equipo | null;
    history: HistorialEvent[];
    tiposFalla: TipoFalla[];
    causasFalla: CausaFalla[];
    onDelete: () => void;
}

const estadoColor = (estado: string): 'success' | 'warning' | 'info' | 'error' | 'default' => {
    const lower = estado?.toLowerCase() || '';
    if (lower.includes('complet') || lower.includes('cerrad')) return 'success';
    if (lower.includes('progreso') || lower.includes('ejecuc')) return 'warning';
    if (lower.includes('pendiente') || lower.includes('abiert')) return 'info';
    if (lower.includes('cancel')) return 'error';
    return 'default';
};

const OrderHeader = ({ order, equipo, history, tiposFalla, causasFalla, onDelete }: OrderHeaderProps) => (
    <Paper
        elevation={0}
        sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            mb: 4,
            p: 2.5,
            borderRadius: 3,
            bgcolor: 'background.paper',
            border: 1,
            borderColor: 'divider',
        }}
    >
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <IconButton
                component={Link}
                to="/"
                sx={{
                    bgcolor: 'action.hover',
                    '&:hover': { bgcolor: 'action.selected' },
                }}
            >
                <ArrowBack sx={{ color: 'primary.main' }} />
            </IconButton>
            <Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <Typography variant="h5" component="h1" fontWeight="bold" color="text.primary">
                        {order.numero}
                    </Typography>
                    <Chip
                        label={order.tipo}
                        size="small"
                        color={order.tipo === 'Correctivo' ? 'error' : 'info'}
                        sx={{ fontWeight: 600, letterSpacing: 0.5 }}
                    />
                </Box>
                <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25 }}>
                    Orden de Mantenimiento
                </Typography>
            </Box>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
            <Chip
                icon={<AccessTime />}
                label={order.estado}
                color={estadoColor(order.estado)}
                sx={{ px: 1, fontWeight: 'bold' }}
            />
            <Button
                variant="outlined"
                color="primary"
                startIcon={<PictureAsPdf />}
                onClick={() => exportOrdenToPDF(order, equipo, history, tiposFalla, causasFalla)}
                size="small"
            >
                Exportar PDF
            </Button>
            <Button
                variant="outlined"
                color="error"
                startIcon={<Delete />}
                onClick={onDelete}
                size="small"
            >
                Eliminar
            </Button>
        </Box>
    </Paper>
);

export default OrderHeader;
