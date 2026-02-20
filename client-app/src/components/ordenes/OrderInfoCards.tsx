import { Box, Typography, Grid, Card, CardContent, alpha, useTheme } from '@mui/material';
import { CalendarToday, Person, LocalShipping } from '@mui/icons-material';
import { OrdenDeTrabajo, Equipo } from '../../types';

const InfoCard = ({ icon, label, value }: { icon: React.ReactElement, label: string, value: string }) => {
    const theme = useTheme();
    return (
        <Card
            elevation={0}
            sx={{
                borderRadius: 2,
                border: 1,
                borderColor: 'divider',
                transition: 'border-color 0.2s',
                '&:hover': { borderColor: 'primary.main' },
            }}
        >
            <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Box
                    sx={{
                        p: 1.5,
                        bgcolor: alpha(theme.palette.primary.main, 0.15),
                        borderRadius: 2,
                        color: 'primary.main',
                        display: 'flex',
                    }}
                >
                    {icon}
                </Box>
                <Box>
                    <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: 0.5, fontSize: '0.65rem' }}>
                        {label}
                    </Typography>
                    <Typography variant="body1" sx={{ fontWeight: 600, color: 'text.primary' }}>{value}</Typography>
                </Box>
            </CardContent>
        </Card>
    );
};

interface OrderInfoCardsProps {
    order: OrdenDeTrabajo;
    equipo: Equipo | null;
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
