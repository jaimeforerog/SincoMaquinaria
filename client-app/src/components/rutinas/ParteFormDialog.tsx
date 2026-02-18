import {
    Dialog, DialogTitle, DialogContent, DialogActions,
    TextField, Button
} from '@mui/material';
import { Save, Cancel } from '@mui/icons-material';
import { Parte } from '../../types';

interface ParteFormDialogProps {
    open: boolean;
    parte: Parte | null;
    onClose: () => void;
    onSave: () => void;
    onChange: (descripcion: string) => void;
}

const ParteFormDialog = ({ open, parte, onClose, onSave, onChange }: ParteFormDialogProps) => (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
        <DialogTitle>{parte?.id ? 'Editar Parte' : 'Agregar Parte'}</DialogTitle>
        <DialogContent>
            <TextField
                autoFocus
                margin="dense"
                label="Descripción"
                fullWidth
                value={parte?.descripcion || ''}
                onChange={(e) => onChange(e.target.value)}
            />
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

export default ParteFormDialog;
