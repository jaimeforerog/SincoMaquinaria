import { Dialog, DialogTitle, DialogContent, DialogActions, Typography, Button } from '@mui/material';

interface DeleteOrderDialogProps {
    open: boolean;
    orderNumber: string;
    onClose: () => void;
    onConfirm: () => void;
}

const DeleteOrderDialog = ({ open, orderNumber, onClose, onConfirm }: DeleteOrderDialogProps) => (
    <Dialog open={open} onClose={onClose}>
        <DialogTitle>Confirmar Eliminación</DialogTitle>
        <DialogContent>
            <Typography>
                ¿Está seguro que desea eliminar la orden {orderNumber}?
                Esta acción no se puede deshacer.
            </Typography>
        </DialogContent>
        <DialogActions>
            <Button onClick={onClose}>
                Cancelar
            </Button>
            <Button onClick={onConfirm} color="warning" variant="contained">
                Eliminar
            </Button>
        </DialogActions>
    </Dialog>
);

export default DeleteOrderDialog;
