import { useState } from 'react';
import { Box, Typography, TextField, Button, MenuItem } from '@mui/material';
import { Add } from '@mui/icons-material';
import { OrdenDeTrabajo, Parte, TipoFalla, CausaFalla } from '../../types';

interface ActivityFormProps {
    order: OrdenDeTrabajo;
    rutinaParts: Parte[];
    tiposFalla: TipoFalla[];
    causasFalla: CausaFalla[];
    onSubmit: (data: {
        description: string;
        partId: string;
        tipoFalla: string;
        causaFalla: string;
    }) => Promise<void>;
}

const ActivityForm = ({ order, rutinaParts, tiposFalla, causasFalla, onSubmit }: ActivityFormProps) => {
    const [newActivity, setNewActivity] = useState('');
    const [selectedPartId, setSelectedPartId] = useState('');
    const [selectedTipoFalla, setSelectedTipoFalla] = useState('');
    const [selectedCausaFalla, setSelectedCausaFalla] = useState('');

    const handleSubmit = async () => {
        await onSubmit({
            description: newActivity,
            partId: selectedPartId,
            tipoFalla: selectedTipoFalla,
            causaFalla: selectedCausaFalla
        });
        setNewActivity('');
        setSelectedPartId('');
        setSelectedTipoFalla('');
        setSelectedCausaFalla('');
    };

    return (
        <Box sx={{ mb: 4, p: 2.5, bgcolor: 'background.paper', borderRadius: 2, border: 1, borderColor: 'divider' }}>
            <Typography variant="subtitle2" gutterBottom sx={{ fontWeight: 'bold', color: 'primary.main' }}>Nueva Actividad</Typography>
            <Box sx={{ display: 'flex', gap: 2, flexDirection: 'column' }}>
                {rutinaParts.length > 0 && (
                    <TextField
                        select
                        label="Parte del Equipo"
                        fullWidth
                        size="small"
                        value={selectedPartId}
                        onChange={(e) => setSelectedPartId(e.target.value)}
                        helperText="Selecciona la parte afectada (Requerido)"
                        required
                    >
                        {rutinaParts.map((p) => (
                            <MenuItem key={p.id} value={p.id}>{p.descripcion}</MenuItem>
                        ))}
                    </TextField>
                )}

                {order?.tipo === 'Correctivo' && (
                    <Box sx={{ display: 'flex', gap: 2 }}>
                        <TextField
                            select
                            label="Tipo de Falla *"
                            fullWidth
                            size="small"
                            value={selectedTipoFalla}
                            onChange={(e) => setSelectedTipoFalla(e.target.value)}
                            helperText="Requerido para OT Correctiva"
                            required
                            error={!selectedTipoFalla}
                        >
                            <MenuItem value=""><em>-- Seleccionar --</em></MenuItem>
                            {tiposFalla.filter(t => t.activo).map((tipo) => (
                                <MenuItem key={tipo.codigo} value={tipo.codigo}>{tipo.descripcion}</MenuItem>
                            ))}
                        </TextField>
                        <TextField
                            select
                            label="Causa de Falla *"
                            fullWidth
                            size="small"
                            value={selectedCausaFalla}
                            onChange={(e) => setSelectedCausaFalla(e.target.value)}
                            helperText="Requerido para OT Correctiva"
                            required
                            error={!selectedCausaFalla}
                        >
                            <MenuItem value=""><em>-- Seleccionar --</em></MenuItem>
                            {causasFalla.filter(c => c.activo).map((causa) => (
                                <MenuItem key={causa.codigo} value={causa.codigo}>{causa.descripcion}</MenuItem>
                            ))}
                        </TextField>
                    </Box>
                )}

                <Box sx={{ display: 'flex', gap: 2 }}>
                    <TextField
                        fullWidth
                        placeholder="Descripción de la actividad (ej. Cambio de Aceite)"
                        value={newActivity}
                        onChange={(e) => setNewActivity(e.target.value)}
                        size="small"
                    />
                    <Button
                        variant="contained"
                        onClick={handleSubmit}
                        disabled={
                            !newActivity ||
                            (rutinaParts.length > 0 && !selectedPartId) ||
                            (order?.tipo === 'Correctivo' && (!selectedTipoFalla || !selectedCausaFalla))
                        }
                        startIcon={<Add />}
                        sx={{ minWidth: 120 }}
                    >
                        Agregar
                    </Button>
                </Box>
            </Box>
        </Box>
    );
};

export default ActivityForm;
