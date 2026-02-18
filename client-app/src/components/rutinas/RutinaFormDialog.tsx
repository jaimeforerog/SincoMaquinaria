import {
    Dialog, DialogTitle, DialogContent, DialogActions,
    TextField, Button, FormControl, InputLabel, Select, MenuItem
} from '@mui/material';
import { Save, Cancel } from '@mui/icons-material';
import { Rutina } from '../../types';

interface RutinaFormDialogProps {
    open: boolean;
    mode: 'create' | 'edit';
    rutina: { descripcion: string; grupo: string } | Rutina | null;
    grupos: any[];
    onClose: () => void;
    onSave: () => void;
    onChange: (field: string, value: string) => void;
}

const RutinaFormDialog = ({ open, mode, rutina, grupos, onClose, onSave, onChange }: RutinaFormDialogProps) => (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
        <DialogTitle>{mode === 'create' ? 'Crear Nueva Rutina' : 'Editar Rutina'}</DialogTitle>
        <DialogContent>
            <TextField
                autoFocus
                margin="dense"
                label="Descripción"
                fullWidth
                required={mode === 'create'}
                value={rutina?.descripcion || ''}
                onChange={(e) => onChange('descripcion', e.target.value)}
                helperText={mode === 'create' ? 'Nombre de la rutina de mantenimiento' : undefined}
            />
            <FormControl fullWidth margin="dense" required={mode === 'create'}>
                <InputLabel>Grupo de Mantenimiento</InputLabel>
                <Select
                    value={rutina?.grupo || ''}
                    label="Grupo de Mantenimiento"
                    onChange={(e) => onChange('grupo', e.target.value as string)}
                >
                    {grupos.map((grupo) => (
                        <MenuItem key={grupo.codigo} value={grupo.nombre}>
                            {grupo.nombre}
                        </MenuItem>
                    ))}
                </Select>
            </FormControl>
        </DialogContent>
        <DialogActions>
            <Button onClick={onClose} startIcon={<Cancel />}>
                Cancelar
            </Button>
            <Button onClick={onSave} variant="contained" startIcon={<Save />}>
                {mode === 'create' ? 'Crear' : 'Guardar'}
            </Button>
        </DialogActions>
    </Dialog>
);

export default RutinaFormDialog;
