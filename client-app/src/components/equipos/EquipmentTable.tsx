import {
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
    Paper, IconButton, Chip
} from '@mui/material';
import { Edit } from '@mui/icons-material';

interface Equipo {
    id: string;
    placa: string;
    descripcion: string;
    marca: string;
    modelo: string;
    serie: string;
    codigo: string;
    tipoMedidorId: string;
    tipoMedidorId2: string;
    grupo: string;
    rutina: string;
}

interface EquipmentTableProps {
    equipos: Equipo[];
    onEditClick: (equipo: Equipo) => void;
}

const EquipmentTable = ({ equipos, onEditClick }: EquipmentTableProps) => (
    <TableContainer component={Paper} sx={{ mt: 3 }}>
        <Table>
            <TableHead sx={{ bgcolor: 'action.hover' }}>
                <TableRow>
                    <TableCell><strong>Placa</strong></TableCell>
                    <TableCell><strong>Descripción</strong></TableCell>
                    <TableCell><strong>Grupo</strong></TableCell>
                    <TableCell align="center"><strong>Acción</strong></TableCell>
                </TableRow>
            </TableHead>
            <TableBody>
                {equipos.map((eq) => (
                    <TableRow key={eq.id} hover>
                        <TableCell>{eq.placa}</TableCell>
                        <TableCell>{eq.descripcion}</TableCell>
                        <TableCell>
                            <Chip label={eq.grupo || 'N/A'} size="small" variant="outlined" />
                        </TableCell>
                        <TableCell align="center">
                            <IconButton onClick={() => onEditClick(eq)} color="primary">
                                <Edit />
                            </IconButton>
                        </TableCell>
                    </TableRow>
                ))}
            </TableBody>
        </Table>
    </TableContainer>
);

export type { Equipo };
export default EquipmentTable;
