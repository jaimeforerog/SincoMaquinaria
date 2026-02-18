import { Box, Typography, Grid, Card, CardContent } from '@mui/material';
import { CalendarToday, Person, LocalShipping } from '@mui/icons-material';
import { OrdenDeTrabajo } from '../../types';

const InfoCard = ({ icon, label, value }: { icon: React.ReactElement, label: string, value: string }) => (
    <Card elevation={2} sx={{ borderRadius: 2 }}>
        <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ p: 1.5, bgcolor: 'primary.main', borderRadius: 2, color: 'white', display: 'flex' }}>
                {icon}
            </Box>
            <Box>
                <Typography variant="caption" color="text.secondary">{label}</Typography>
                <Typography variant="h6" sx={{ fontSize: '1rem', fontWeight: 'bold' }}>{value}</Typography>
            </Box>
        </CardContent>
    </Card>
);

interface OrderInfoCardsProps {
    order: OrdenDeTrabajo;
    equipo: any | null;
}

const OrderInfoCards = ({ order, equipo }: OrderInfoCardsProps) => (
    <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{ xs: 12, md: 4 }}>
            <InfoCard icon={<LocalShipping />} label="Equipo" value={equipo ? `${equipo.placa} - ${equipo.descripcion}` : (order.equipoId || 'N/A')} />
        </Grid>
        <Grid size={{ xs: 12, md: 4 }}>
            <InfoCard icon={<Person />} label="Responsable" value="Sin Asignar" />
        </Grid>
        <Grid size={{ xs: 12, md: 4 }}>
            <InfoCard icon={<CalendarToday />} label="Fecha Creación" value={new Date().toLocaleDateString()} />
        </Grid>
    </Grid>
);

export default OrderInfoCards;
