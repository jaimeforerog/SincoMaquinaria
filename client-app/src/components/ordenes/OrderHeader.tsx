import { Box, Typography, IconButton, Chip, Button } from '@mui/material';
import { ArrowBack, AccessTime, PictureAsPdf, Delete } from '@mui/icons-material';
import { Link } from 'react-router-dom';
import { OrdenDeTrabajo } from '../../types';
import { exportOrdenToPDF } from '../../services/PDFExportService';

interface OrderHeaderProps {
    order: OrdenDeTrabajo;
    equipo: any | null;
    history: any[];
    tiposFalla: any[];
    causasFalla: any[];
    onDelete: () => void;
}

const OrderHeader = ({ order, equipo, history, tiposFalla, causasFalla, onDelete }: OrderHeaderProps) => (
    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 4 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <IconButton component={Link} to="/" color="inherit">
                <ArrowBack />
            </IconButton>
            <Box>
                <Typography variant="h4" component="h1" fontWeight="bold">
                    {order.numero} - {order.tipo}
                </Typography>
                <Typography variant="subtitle1" color="text.secondary">
                    Orden de Mantenimiento
                </Typography>
            </Box>
        </Box>
        <Chip
            icon={<AccessTime />}
            label={order.estado}
            color="primary"
            variant="outlined"
            sx={{ px: 1, fontWeight: 'bold' }}
        />
        <Button
            variant="contained"
            color="primary"
            startIcon={<PictureAsPdf />}
            onClick={() => exportOrdenToPDF(order, equipo, history, tiposFalla, causasFalla)}
            sx={{ ml: 2 }}
        >
            Exportar PDF
        </Button>
        <Button
            variant="contained"
            color="warning"
            startIcon={<Delete />}
            onClick={onDelete}
            sx={{ ml: 2 }}
        >
            Eliminar OT
        </Button>
    </Box>
);

export default OrderHeader;
