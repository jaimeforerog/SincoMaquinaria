import {
    Box, Typography, Chip, IconButton, Button, Paper,
    Accordion, AccordionSummary, AccordionDetails,
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Alert
} from '@mui/material';
import { ExpandMore, Edit, Delete, Add } from '@mui/icons-material';
import { Rutina, Parte, Actividad } from '../../types';

interface RutinaAccordionProps {
    rutina: Rutina;
    onEditRutina: (rutina: Rutina) => void;
    onAddParte: (rutinaId: string) => void;
    onEditParte: (rutinaId: string, parte: Parte) => void;
    onDeleteParte: (rutinaId: string, parteId: string) => void;
    onAddActividad: (rutinaId: string, parteId: string) => void;
    onEditActividad: (rutinaId: string, parteId: string, actividad: Actividad) => void;
    onDeleteActividad: (rutinaId: string, parteId: string, actividadId: string) => void;
}

const ActividadesTable = ({ rutina, parte, onEditActividad, onDeleteActividad }: {
    rutina: Rutina;
    parte: Parte;
    onEditActividad: (rutinaId: string, parteId: string, actividad: Actividad) => void;
    onDeleteActividad: (rutinaId: string, parteId: string, actividadId: string) => void;
}) => (
    <TableContainer>
        <Table size="small">
            <TableHead>
                <TableRow>
                    <TableCell><strong>Actividad</strong></TableCell>
                    <TableCell><strong>Clase</strong></TableCell>
                    <TableCell><strong>Frecuencia</strong></TableCell>
                    <TableCell><strong>Insumo</strong></TableCell>
                    <TableCell align="right"><strong>Acciones</strong></TableCell>
                </TableRow>
            </TableHead>
            <TableBody>
                {parte.actividades.map((actividad) => (
                    <TableRow key={actividad.id}>
                        <TableCell>{actividad.descripcion}</TableCell>
                        <TableCell>{actividad.clase}</TableCell>
                        <TableCell>
                            {actividad.frecuencia} {actividad.unidadMedida}
                        </TableCell>
                        <TableCell>
                            {actividad.insumo || 'N/A'} ({actividad.cantidad})
                        </TableCell>
                        <TableCell align="right">
                            <IconButton
                                size="small"
                                onClick={() => onEditActividad(rutina.id, parte.id, actividad)}
                            >
                                <Edit fontSize="small" />
                            </IconButton>
                            <IconButton
                                size="small"
                                color="error"
                                onClick={() => onDeleteActividad(rutina.id, parte.id, actividad.id)}
                            >
                                <Delete fontSize="small" />
                            </IconButton>
                        </TableCell>
                    </TableRow>
                ))}
            </TableBody>
        </Table>
    </TableContainer>
);

const RutinaAccordion = ({
    rutina,
    onEditRutina,
    onAddParte,
    onEditParte,
    onDeleteParte,
    onAddActividad,
    onEditActividad,
    onDeleteActividad
}: RutinaAccordionProps) => (
    <Accordion sx={{ mb: 2 }}>
        <AccordionSummary expandIcon={<ExpandMore />}>
            <Box sx={{ display: 'flex', alignItems: 'center', width: '100%' }}>
                <Box sx={{ flexGrow: 1 }}>
                    <Typography variant="h6">{rutina.descripcion}</Typography>
                    <Chip label={rutina.grupo} size="small" sx={{ mt: 0.5 }} />
                </Box>
                <IconButton
                    onClick={(e) => {
                        e.stopPropagation();
                        onEditRutina(rutina);
                    }}
                    size="small"
                >
                    <Edit />
                </IconButton>
            </Box>
        </AccordionSummary>
        <AccordionDetails>
            <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between' }}>
                <Typography variant="subtitle2" color="text.secondary">
                    Partes del Equipo
                </Typography>
                <Button
                    startIcon={<Add />}
                    size="small"
                    onClick={() => onAddParte(rutina.id)}
                >
                    Agregar Parte
                </Button>
            </Box>

            {rutina.partes && rutina.partes.length > 0 ? (
                rutina.partes.map((parte) => (
                    <Paper key={parte.id} sx={{ p: 2, mb: 2 }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                            <Typography variant="subtitle1" fontWeight="bold">
                                {parte.descripcion}
                            </Typography>
                            <Box>
                                <IconButton
                                    size="small"
                                    onClick={() => onEditParte(rutina.id, parte)}
                                >
                                    <Edit fontSize="small" />
                                </IconButton>
                                <IconButton
                                    size="small"
                                    color="error"
                                    onClick={() => onDeleteParte(rutina.id, parte.id)}
                                >
                                    <Delete fontSize="small" />
                                </IconButton>
                            </Box>
                        </Box>

                        <Box sx={{ mb: 1 }}>
                            <Button
                                startIcon={<Add />}
                                size="small"
                                variant="outlined"
                                onClick={() => onAddActividad(rutina.id, parte.id)}
                            >
                                Agregar Actividad
                            </Button>
                        </Box>

                        {parte.actividades && parte.actividades.length > 0 && (
                            <ActividadesTable
                                rutina={rutina}
                                parte={parte}
                                onEditActividad={onEditActividad}
                                onDeleteActividad={onDeleteActividad}
                            />
                        )}
                    </Paper>
                ))
            ) : (
                <Alert severity="info">No hay partes definidas para esta rutina.</Alert>
            )}
        </AccordionDetails>
    </Accordion>
);

export default RutinaAccordion;
