import {
    Dialog, DialogTitle, DialogContent, DialogActions,
    TextField, Button, Box, Alert, FormControl, InputLabel, Select, MenuItem
} from '@mui/material';
import { Save, Cancel } from '@mui/icons-material';
import { Actividad } from '../../types';

interface ActividadFormDialogProps {
    open: boolean;
    actividad: Actividad | null;
    medidores: any[];
    error: string | null;
    onClose: () => void;
    onSave: () => void;
    onChange: (actividad: Actividad) => void;
    onClearError: () => void;
}

const ActividadFormDialog = ({
    open, actividad, medidores, error,
    onClose, onSave, onChange, onClearError
}: ActividadFormDialogProps) => {
    const update = (field: string, value: any) => {
        if (!actividad) return;
        onChange({ ...actividad, [field]: value });
    };

    const handleMedidorChange = (field: 'nombreMedidor' | 'nombreMedidor2', value: string) => {
        if (!actividad) return;
        const selectedMedidor = medidores.find(m => m.nombre === value);
        const unidadField = field === 'nombreMedidor' ? 'unidadMedida' : 'unidadMedida2';
        onChange({
            ...actividad,
            [field]: value,
            [unidadField]: selectedMedidor?.unidad || ''
        });
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
            <DialogTitle>{actividad?.id ? 'Editar Actividad' : 'Agregar Actividad'}</DialogTitle>
            <DialogContent>
                {error && (
                    <Alert severity="error" sx={{ mb: 2 }} onClose={onClearError}>
                        {error}
                    </Alert>
                )}
                <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2, mt: 1 }}>
                    <TextField
                        label="Descripción *"
                        fullWidth
                        required
                        error={!actividad?.descripcion}
                        helperText={!actividad?.descripcion ? 'Obligatorio' : ''}
                        value={actividad?.descripcion || ''}
                        onChange={(e) => update('descripcion', e.target.value)}
                    />
                    <TextField
                        label="Clase"
                        fullWidth
                        value={actividad?.clase || ''}
                        onChange={(e) => update('clase', e.target.value)}
                    />
                    <TextField
                        label="Frecuencia"
                        type="number"
                        fullWidth
                        value={actividad?.frecuencia || 0}
                        onChange={(e) => update('frecuencia', Number(e.target.value))}
                    />
                    <FormControl fullWidth>
                        <InputLabel>Medidor I</InputLabel>
                        <Select
                            value={actividad?.nombreMedidor || ''}
                            label="Medidor I"
                            onChange={(e) => handleMedidorChange('nombreMedidor', e.target.value as string)}
                        >
                            <MenuItem value="">
                                <em>Ninguno</em>
                            </MenuItem>
                            {medidores.map((medidor) => (
                                <MenuItem key={medidor.codigo} value={medidor.nombre}>
                                    {medidor.nombre} ({medidor.unidad})
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <TextField
                        label="Alerta Faltando"
                        type="number"
                        fullWidth
                        value={actividad?.alertaFaltando || 0}
                        onChange={(e) => update('alertaFaltando', Number(e.target.value))}
                    />
                    <TextField
                        label="Frecuencia II"
                        type="number"
                        fullWidth
                        value={actividad?.frecuencia2 || 0}
                        onChange={(e) => update('frecuencia2', Number(e.target.value))}
                    />
                    <FormControl fullWidth>
                        <InputLabel>Medidor II</InputLabel>
                        <Select
                            value={actividad?.nombreMedidor2 || ''}
                            label="Medidor II"
                            onChange={(e) => handleMedidorChange('nombreMedidor2', e.target.value as string)}
                        >
                            <MenuItem value="">
                                <em>Ninguno</em>
                            </MenuItem>
                            {medidores.map((medidor) => (
                                <MenuItem key={medidor.codigo} value={medidor.nombre}>
                                    {medidor.nombre} ({medidor.unidad})
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <TextField
                        label="Alerta Faltando II"
                        type="number"
                        fullWidth
                        value={actividad?.alertaFaltando2 || 0}
                        onChange={(e) => update('alertaFaltando2', Number(e.target.value))}
                    />
                    <TextField
                        label="Insumo"
                        fullWidth
                        value={actividad?.insumo || ''}
                        onChange={(e) => update('insumo', e.target.value)}
                    />
                    <TextField
                        label="Cantidad"
                        type="number"
                        fullWidth
                        value={actividad?.cantidad || 0}
                        onChange={(e) => update('cantidad', Number(e.target.value))}
                    />
                </Box>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose} startIcon={<Cancel />}>
                    Cancelar
                </Button>
                <Button onClick={onSave} variant="contained" startIcon={<Save />}>
                    Guardar
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default ActividadFormDialog;
