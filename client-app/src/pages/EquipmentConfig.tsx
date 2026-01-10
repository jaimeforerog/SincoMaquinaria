import { useEffect, useState } from 'react';
import {
    Box, Typography, Button, Table, TableBody, TableCell, TableContainer,
    TableHead, TableRow, Paper, IconButton, Dialog, DialogTitle,
    DialogContent, DialogActions, TextField, Grid, Chip, CircularProgress
} from '@mui/material';
import { Edit } from '@mui/icons-material';

interface Equipo {
    id: string; // Keep id for API calls
    placa: string;
    descripcion: string;
    // marca: string; // Removed from edit
    // modelo: string; // Removed from edit
    serie: string;
    codigo: string;
    tipoMedidorId: string;
    tipoMedidorId2: string;
    grupo: string;
    rutina: string;
    // Add other fields as necessary based on your backend model
}

const EquipmentConfig = () => {
    const [equipos, setEquipos] = useState<Equipo[]>([]);
    const [loading, setLoading] = useState(true);
    const [openEdit, setOpenEdit] = useState(false);
    const [currentEquipo, setCurrentEquipo] = useState<Equipo | null>(null);

    // Filter/Search states could be added here

    useEffect(() => {
        fetchEquipos();
    }, []);

    const fetchEquipos = async () => {
        setLoading(true);
        try {
            const res = await fetch('/equipos');
            if (res.ok) {
                const data = await res.json();
                setEquipos(data);
            }
        } catch (error) {
            console.error("Error fetching equipments", error);
        } finally {
            setLoading(false);
        }
    };

    const handleEditClick = (equipo: Equipo) => {
        setCurrentEquipo({ ...equipo }); // Clone to avoid direct mutation
        setOpenEdit(true);
    };

    const handleCloseEdit = () => {
        setOpenEdit(false);
        setCurrentEquipo(null);
    };

    const handleSave = async () => {
        if (!currentEquipo) return;

        try {
            // Note: Sending 'Marca' and 'Modelo' as empty or existing values if needed by backend, 
            // but for now ignoring them in the UI as requested.
            const res = await fetch(`/equipos/${currentEquipo.id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    Placa: currentEquipo.placa, // Add placa to the body
                    Descripcion: currentEquipo.descripcion,
                    // Marca: currentEquipo.marca, // Removed
                    // Modelo: currentEquipo.modelo, // Removed
                    Serie: currentEquipo.serie,
                    Codigo: currentEquipo.codigo,
                    TipoMedidorId: currentEquipo.tipoMedidorId,
                    TipoMedidorId2: currentEquipo.tipoMedidorId2,
                    Grupo: currentEquipo.grupo,
                    Rutina: currentEquipo.rutina
                })
            });

            if (res.ok) {
                // Refresh list or update local state
                fetchEquipos();
                handleCloseEdit();
            } else {
                alert("Error al guardar cambios");
            }
        } catch (error) {
            console.error("Error saving equipment", error);
            alert("Error de conexión");
        }
    };

    const handleChange = (field: keyof Equipo, value: string) => {
        if (currentEquipo) {
            setCurrentEquipo({ ...currentEquipo, [field]: value });
        }
    };

    return (
        <Box>
            <Typography variant="h4" gutterBottom fontWeight="bold" color="text.primary">
                Configuración de Equipos
            </Typography>

            {loading ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
                    <CircularProgress />
                </Box>
            ) : (
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
                                        <IconButton onClick={() => handleEditClick(eq)} color="primary">
                                            <Edit />
                                        </IconButton>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            )}

            {/* Edit Dialog */}
            <Dialog open={openEdit} onClose={handleCloseEdit} maxWidth="md" fullWidth>
                <DialogTitle>Editar Equipo</DialogTitle>
                <DialogContent dividers>
                    {currentEquipo && (
                        <Grid container spacing={2} sx={{ pt: 1 }}>
                            <Grid item xs={12} sm={6}>
                                <TextField
                                    label="Placa"
                                    fullWidth
                                    value={currentEquipo.placa}
                                    InputProps={{
                                        readOnly: true,
                                    }}
                                    disabled
                                />
                            </Grid>
                            <Grid item xs={12} sm={12}>
                                <TextField
                                    label="Descripción"
                                    fullWidth
                                    value={currentEquipo.descripcion}
                                    onChange={(e) => handleChange('descripcion', e.target.value)}
                                />
                            </Grid>
                            {/* Marca and Modelo removed as requested */}
                            <Grid item xs={12} sm={6}>
                                <TextField
                                    label="Serie"
                                    fullWidth
                                    value={currentEquipo.serie}
                                    onChange={(e) => handleChange('serie', e.target.value)}
                                />
                            </Grid>
                            <Grid item xs={12} sm={6}>
                                <TextField
                                    label="Grupo"
                                    fullWidth
                                    value={currentEquipo.grupo}
                                    onChange={(e) => handleChange('grupo', e.target.value)}
                                    helperText="Grupo de Mantenimiento"
                                />
                            </Grid>
                            {/* Add more fields as needed */}
                        </Grid>
                    )}
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCloseEdit} color="inherit">Cancelar</Button>
                    <Button onClick={handleSave} variant="contained" color="primary">Guardar</Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
};

export default EquipmentConfig;
