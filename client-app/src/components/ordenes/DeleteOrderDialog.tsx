import { Dialog, DialogTitle, DialogContent, DialogActions, Typography, Button } from '@mui/material';

interface DeleteOrderDialogProps {
    open: boolean;
    orderNumber: string;
    onClose: () => void;
    onConfirm: () => void;
}

const DeleteOrderDialog = ({ open, orderNumber, onClose, onConfirm }: DeleteOrderDialogProps) => (
    <Dialog open={open} onClose={onClose} PaperProps={{ sx: { borderRadius: 2 } }}>
        <DialogTitle sx={{ fontWeight: 'bold' }}>Confirmar Eliminación</DialogTitle>
        <DialogContent>
            <Typography color="text.primary">
                ¿Está seguro que desea eliminar la orden <strong>{orderNumber}</strong>?
            </Typography>
            <Typography variant="body2" color="error" sx={{ mt: 1 }}>
                Esta acción no se puede deshacer.
            </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
            <Button onClick={onClose} variant="outlined">
                Cancelar
            </Button>
            <Button onClick={onConfirm} color="error" variant="contained">
                Eliminar
            </Button>
        </DialogActions>
    </Dialog>
);

export default DeleteOrderDialog;
