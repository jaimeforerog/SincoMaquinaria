import React, { useEffect, useState } from 'react';
import {
    Box, Typography, Button, CircularProgress, Alert,
    Accordion, AccordionSummary, AccordionDetails,
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    TextField, Dialog, DialogTitle, DialogContent, DialogActions,
    IconButton, Chip, MenuItem, FormControl, InputLabel, Select
} from '@mui/material';
import {
    ExpandMore, Edit, Delete, Add, Save, Cancel
} from '@mui/icons-material';
import { useAuthFetch } from '../hooks/useAuthFetch';
import { Rutina, Parte, Actividad } from '../types';

const EditarRutinas = () => {
    const authFetch = useAuthFetch();
    const [rutinas, setRutinas] = useState<Rutina[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [grupos, setGrupos] = useState<any[]>([]);
    const [medidores, setMedidores] = useState<any[]>([]);

    // Dialog states
    const [editRutinaDialog, setEditRutinaDialog] = useState(false);
    const [createRutinaDialog, setCreateRutinaDialog] = useState(false);
    const [editParteDialog, setEditParteDialog] = useState(false);
    const [editActividadDialog, setEditActividadDialog] = useState(false);
    const [newRutina, setNewRutina] = useState({ descripcion: '', grupo: '' });

    const [currentRutina, setCurrentRutina] = useState<Rutina | null>(null);
    const [currentParte, setCurrentParte] = useState<Parte | null>(null);
    const [currentActividad, setCurrentActividad] = useState<Actividad | null>(null);
    const [currentRutinaId, setCurrentRutinaId] = useState<string | null>(null);
    const [currentParteId, setCurrentParteId] = useState<string | null>(null);

    useEffect(() => {
        fetchRutinas();
        fetchGrupos();
        fetchMedidores();
    }, []);

    const fetchGrupos = async () => {
        try {
            const res = await authFetch('/configuracion/grupos');
            if (res.ok) {
                const data = await res.json();
                setGrupos(data.filter((g: any) => g.activo));
            }
        } catch (err) {
            console.error('Error loading grupos', err);
        }
    };

    const fetchMedidores = async () => {
        try {
            const res = await authFetch('/configuracion/medidores');
            if (res.ok) {
                const data = await res.json();
                setMedidores(data.filter((m: any) => m.activo));
            }
        } catch (err) {
            console.error('Error loading medidores', err);
        }
    };

    const fetchRutinas = async () => {
        setLoading(true);
        setError(null);
        try {
            const res = await authFetch('/rutinas?pageSize=1000');
            if (res.ok) {
                const response = await res.json();
                const rutinasData = response.data || response;

                // Fetch full details for each rutina
                const rutinasWithDetails = await Promise.all(
                    rutinasData.map(async (r: any) => {
                        const detailRes = await authFetch(`/rutinas/${r.id}`);
                        if (detailRes.ok) {
                            return await detailRes.json();
                        }
                        return r;
                    })
                );

                setRutinas(rutinasWithDetails);
            } else {
                setError('Error al cargar las rutinas');
            }
        } catch (err) {
            setError('Error de conexión');
        } finally {
            setLoading(false);
        }
    };

    // Rutina CRUD
    const handleCreateRutina = async () => {
        if (!newRutina.descripcion || !newRutina.grupo) {
            setError('Descripción y Grupo son obligatorios');
            return;
        }

        try {
            const res = await authFetch('/rutinas', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(newRutina)
            });

            if (res.ok) {
                setSuccess('Rutina creada exitosamente');
                setCreateRutinaDialog(false);
                setNewRutina({ descripcion: '', grupo: '' });
                fetchRutinas();
            } else {
                setError('Error al crear la rutina');
            }
        } catch (err) {
            setError('Error de conexión');
        }
    };

    const handleEditRutina = (rutina: Rutina) => {
        setCurrentRutina({ ...rutina });
        setEditRutinaDialog(true);
    };

    const handleSaveRutina = async () => {
        if (!currentRutina) return;

        try {
            const res = await authFetch(`/rutinas/${currentRutina.id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    descripcion: currentRutina.descripcion,
                    grupo: currentRutina.grupo
                })
            });

            if (res.ok) {
                setSuccess('Rutina actualizada exitosamente');
                setEditRutinaDialog(false);
                fetchRutinas();
            } else {
                setError('Error al actualizar la rutina');
            }
        } catch (err) {
            setError('Error de conexión');
        }
    };

    // Parte CRUD
    const handleAddParte = (rutinaId: string) => {
        setCurrentRutinaId(rutinaId);
        setCurrentParte({ id: '', descripcion: '', actividades: [] });
        setEditParteDialog(true);
    };

    const handleEditParte = (rutinaId: string, parte: Parte) => {
        setCurrentRutinaId(rutinaId);
        setCurrentParte({ ...parte });
        setEditParteDialog(true);
    };

    const handleSaveParte = async () => {
        if (!currentParte || !currentRutinaId) return;

        try {
            let res;
            if (currentParte.id) {
                // Update existing
                res = await authFetch(`/rutinas/${currentRutinaId}/partes/${currentParte.id}`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ descripcion: currentParte.descripcion })
                });
            } else {
                // Add new
                res = await authFetch(`/rutinas/${currentRutinaId}/partes`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ descripcion: currentParte.descripcion })
                });
            }

            if (res.ok) {
                setSuccess('Parte guardada exitosamente');
                setEditParteDialog(false);
                fetchRutinas();
            } else {
                setError('Error al guardar la parte');
            }
        } catch (err) {
            setError('Error de conexión');
        }
    };

    const handleDeleteParte = async (rutinaId: string, parteId: string) => {
        if (!confirm('¿Está seguro de eliminar esta parte?')) return;

        try {
            const res = await authFetch(`/rutinas/${rutinaId}/partes/${parteId}`, {
                method: 'DELETE'
            });

            if (res.ok || res.status === 204) {
                setSuccess('Parte eliminada exitosamente');
                fetchRutinas();
            } else {
                setError('Error al eliminar la parte');
            }
        } catch (err) {
            setError('Error de conexión');
        }
    };

    // Actividad CRUD
    const handleAddActividad = (rutinaId: string, parteId: string) => {
        setCurrentRutinaId(rutinaId);
        setCurrentParteId(parteId);
        setCurrentActividad({
            id: '',
            descripcion: '',
            clase: '',
            frecuencia: 0,
            unidadMedida: '',
            nombreMedidor: '',
            alertaFaltando: 0,
            frecuencia2: 0,
            unidadMedida2: '',
            nombreMedidor2: '',
            alertaFaltando2: 0,
            insumo: '',
            cantidad: 0
        });
        setEditActividadDialog(true);
    };

    const handleEditActividad = (rutinaId: string, parteId: string, actividad: Actividad) => {
        setCurrentRutinaId(rutinaId);
        setCurrentParteId(parteId);
        setCurrentActividad({ ...actividad });
        setEditActividadDialog(true);
    };

    const handleSaveActividad = async () => {
        if (!currentActividad || !currentRutinaId || !currentParteId) return;

        // Validate description is not empty
        if (!currentActividad.descripcion || currentActividad.descripcion.trim() === '') {
            setError('La descripción de la actividad es obligatoria');
            return;
        }

        try {
            let res;
            if (currentActividad.id) {
                // Update existing
                res = await authFetch(
                    `/rutinas/${currentRutinaId}/partes/${currentParteId}/actividades/${currentActividad.id}`,
                    {
                        method: 'PUT',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(currentActividad)
                    }
                );
            } else {
                // Add new
                res = await authFetch(
                    `/rutinas/${currentRutinaId}/partes/${currentParteId}/actividades`,
                    {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(currentActividad)
                    }
                );
            }

            if (res.ok) {
                setSuccess('Actividad guardada exitosamente');
                setEditActividadDialog(false);
                fetchRutinas();
            } else {
                setError('Error al guardar la actividad');
            }
        } catch (err) {
            setError('Error de conexión');
        }
    };

    const handleDeleteActividad = async (rutinaId: string, parteId: string, actividadId: string) => {
        if (!confirm('¿Está seguro de eliminar esta actividad?')) return;

        try {
            const res = await authFetch(
                `/rutinas/${rutinaId}/partes/${parteId}/actividades/${actividadId}`,
                { method: 'DELETE' }
            );

            if (res.ok || res.status === 204) {
                setSuccess('Actividad eliminada exitosamente');
                fetchRutinas();
            } else {
                setError('Error al eliminar la actividad');
            }
        } catch (err) {
            setError('Error de conexión');
        }
    };

    if (loading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
                <CircularProgress />
            </Box>
        );
    }

    return (
        <Box sx={{ p: 3 }}>
            {/* Header */}
            <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <Box>
                    <Typography variant="h4" component="h1" fontWeight="bold">
                        Editar Rutinas de Mantenimiento
                    </Typography>
                    <Typography variant="subtitle1" color="text.secondary">
                        Gestione las rutinas de mantenimiento
                    </Typography>
                </Box>
                <Button
                    variant="contained"
                    startIcon={<Add />}
                    onClick={() => {
                        setNewRutina({ descripcion: '', grupo: '' });
                        setCreateRutinaDialog(true);
                    }}
                >
                    Nueva Rutina
                </Button>
            </Box>

            {/* Alerts */}
            {error && (
                <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
                    {error}
                </Alert>
            )}
            {success && (
                <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(null)}>
                    {success}
                </Alert>
            )}

            {/* Rutinas List */}
            {rutinas.length === 0 ? (
                <Alert severity="info">No hay rutinas para editar. Importe rutinas primero.</Alert>
            ) : (
                rutinas.map((rutina) => (
                    <Accordion key={rutina.id} sx={{ mb: 2 }}>
                        <AccordionSummary expandIcon={<ExpandMore />}>
                            <Box sx={{ display: 'flex', alignItems: 'center', width: '100%' }}>
                                <Box sx={{ flexGrow: 1 }}>
                                    <Typography variant="h6">{rutina.descripcion}</Typography>
                                    <Chip label={rutina.grupo} size="small" sx={{ mt: 0.5 }} />
                                </Box>
                                <IconButton
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        handleEditRutina(rutina);
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
                                    onClick={() => handleAddParte(rutina.id)}
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
                                                    onClick={() => handleEditParte(rutina.id, parte)}
                                                >
                                                    <Edit fontSize="small" />
                                                </IconButton>
                                                <IconButton
                                                    size="small"
                                                    color="error"
                                                    onClick={() => handleDeleteParte(rutina.id, parte.id)}
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
                                                onClick={() => handleAddActividad(rutina.id, parte.id)}
                                            >
                                                Agregar Actividad
                                            </Button>
                                        </Box>

                                        {parte.actividades && parte.actividades.length > 0 && (
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
                                                                        onClick={() =>
                                                                            handleEditActividad(rutina.id, parte.id, actividad)
                                                                        }
                                                                    >
                                                                        <Edit fontSize="small" />
                                                                    </IconButton>
                                                                    <IconButton
                                                                        size="small"
                                                                        color="error"
                                                                        onClick={() =>
                                                                            handleDeleteActividad(
                                                                                rutina.id,
                                                                                parte.id,
                                                                                actividad.id
                                                                            )
                                                                        }
                                                                    >
                                                                        <Delete fontSize="small" />
                                                                    </IconButton>
                                                                </TableCell>
                                                            </TableRow>
                                                        ))}
                                                    </TableBody>
                                                </Table>
                                            </TableContainer>
                                        )}
                                    </Paper>
                                ))
                            ) : (
                                <Alert severity="info">No hay partes definidas para esta rutina.</Alert>
                            )}
                        </AccordionDetails>
                    </Accordion>
                ))
            )}

            {/* Create Rutina Dialog */}
            <Dialog open={createRutinaDialog} onClose={() => setCreateRutinaDialog(false)} maxWidth="sm" fullWidth>
                <DialogTitle>Crear Nueva Rutina</DialogTitle>
                <DialogContent>
                    <TextField
                        autoFocus
                        margin="dense"
                        label="Descripción"
                        fullWidth
                        required
                        value={newRutina.descripcion}
                        onChange={(e) =>
                            setNewRutina({ ...newRutina, descripcion: e.target.value })
                        }
                        helperText="Nombre de la rutina de mantenimiento"
                    />
                    <FormControl fullWidth margin="dense" required>
                        <InputLabel>Grupo de Mantenimiento</InputLabel>
                        <Select
                            value={newRutina.grupo}
                            label="Grupo de Mantenimiento"
                            onChange={(e) => setNewRutina({ ...newRutina, grupo: e.target.value as string })}
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
                    <Button onClick={() => setCreateRutinaDialog(false)} startIcon={<Cancel />}>
                        Cancelar
                    </Button>
                    <Button onClick={handleCreateRutina} variant="contained" startIcon={<Save />}>
                        Crear
                    </Button>
                </DialogActions>
            </Dialog>

            {/* Edit Rutina Dialog */}
            <Dialog open={editRutinaDialog} onClose={() => setEditRutinaDialog(false)} maxWidth="sm" fullWidth>
                <DialogTitle>Editar Rutina</DialogTitle>
                <DialogContent>
                    <TextField
                        autoFocus
                        margin="dense"
                        label="Descripción"
                        fullWidth
                        value={currentRutina?.descripcion || ''}
                        onChange={(e) =>
                            setCurrentRutina({ ...currentRutina!, descripcion: e.target.value })
                        }
                    />
                    <FormControl fullWidth margin="dense">
                        <InputLabel>Grupo de Mantenimiento</InputLabel>
                        <Select
                            value={currentRutina?.grupo || ''}
                            label="Grupo de Mantenimiento"
                            onChange={(e) => setCurrentRutina({ ...currentRutina!, grupo: e.target.value as string })}
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
                    <Button onClick={() => setEditRutinaDialog(false)} startIcon={<Cancel />}>
                        Cancelar
                    </Button>
                    <Button onClick={handleSaveRutina} variant="contained" startIcon={<Save />}>
                        Guardar
                    </Button>
                </DialogActions>
            </Dialog>

            {/* Edit Parte Dialog */}
            <Dialog open={editParteDialog} onClose={() => setEditParteDialog(false)} maxWidth="sm" fullWidth>
                <DialogTitle>{currentParte?.id ? 'Editar Parte' : 'Agregar Parte'}</DialogTitle>
                <DialogContent>
                    <TextField
                        autoFocus
                        margin="dense"
                        label="Descripción"
                        fullWidth
                        value={currentParte?.descripcion || ''}
                        onChange={(e) =>
                            setCurrentParte({ ...currentParte!, descripcion: e.target.value })
                        }
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setEditParteDialog(false)} startIcon={<Cancel />}>
                        Cancelar
                    </Button>
                    <Button onClick={handleSaveParte} variant="contained" startIcon={<Save />}>
                        Guardar
                    </Button>
                </DialogActions>
            </Dialog>

            {/* Edit Actividad Dialog */}
            <Dialog open={editActividadDialog} onClose={() => setEditActividadDialog(false)} maxWidth="md" fullWidth>
                <DialogTitle>{currentActividad?.id ? 'Editar Actividad' : 'Agregar Actividad'}</DialogTitle>
                <DialogContent>
                    {error && (
                        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
                            {error}
                        </Alert>
                    )}
                    <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2, mt: 1 }}>
                        <TextField
                            label="Descripción *"
                            fullWidth
                            required
                            error={!currentActividad?.descripcion}
                            helperText={!currentActividad?.descripcion ? 'Obligatorio' : ''}
                            value={currentActividad?.descripcion || ''}
                            onChange={(e) =>
                                setCurrentActividad({ ...currentActividad!, descripcion: e.target.value })
                            }
                        />
                        <TextField
                            label="Clase"
                            fullWidth
                            value={currentActividad?.clase || ''}
                            onChange={(e) =>
                                setCurrentActividad({ ...currentActividad!, clase: e.target.value })
                            }
                        />
                        <TextField
                            label="Frecuencia"
                            type="number"
                            fullWidth
                            value={currentActividad?.frecuencia || 0}
                            onChange={(e) =>
                                setCurrentActividad({ ...currentActividad!, frecuencia: Number(e.target.value) })
                            }
                        />
                        <FormControl fullWidth>
                            <InputLabel>Medidor I</InputLabel>
                            <Select
                                value={currentActividad?.nombreMedidor || ''}
                                label="Medidor I"
                                onChange={(e) => {
                                    const selectedMedidor = medidores.find(m => m.nombre === e.target.value);
                                    setCurrentActividad({
                                        ...currentActividad!,
                                        nombreMedidor: e.target.value as string,
                                        unidadMedida: selectedMedidor?.unidad || ''
                                    });
                                }}
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
                            value={currentActividad?.alertaFaltando || 0}
                            onChange={(e) =>
                                setCurrentActividad({ ...currentActividad!, alertaFaltando: Number(e.target.value) })
                            }
                        />
                        <TextField
                            label="Frecuencia II"
                            type="number"
                            fullWidth
                            value={currentActividad?.frecuencia2 || 0}
                            onChange={(e) =>
                                setCurrentActividad({ ...currentActividad!, frecuencia2: Number(e.target.value) })
                            }
                        />
                        <FormControl fullWidth>
                            <InputLabel>Medidor II</InputLabel>
                            <Select
                                value={currentActividad?.nombreMedidor2 || ''}
                                label="Medidor II"
                                onChange={(e) => {
                                    const selectedMedidor = medidores.find(m => m.nombre === e.target.value);
                                    setCurrentActividad({
                                        ...currentActividad!,
                                        nombreMedidor2: e.target.value as string,
                                        unidadMedida2: selectedMedidor?.unidad || ''
                                    });
                                }}
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
                            value={currentActividad?.alertaFaltando2 || 0}
                            onChange={(e) =>
                                setCurrentActividad({ ...currentActividad!, alertaFaltando2: Number(e.target.value) })
                            }
                        />
                        <TextField
                            label="Insumo"
                            fullWidth
                            value={currentActividad?.insumo || ''}
                            onChange={(e) =>
                                setCurrentActividad({ ...currentActividad!, insumo: e.target.value })
                            }
                        />
                        <TextField
                            label="Cantidad"
                            type="number"
                            fullWidth
                            value={currentActividad?.cantidad || 0}
                            onChange={(e) =>
                                setCurrentActividad({ ...currentActividad!, cantidad: Number(e.target.value) })
                            }
                        />
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setEditActividadDialog(false)} startIcon={<Cancel />}>
                        Cancelar
                    </Button>
                    <Button onClick={handleSaveActividad} variant="contained" startIcon={<Save />}>
                        Guardar
                    </Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
};

export default EditarRutinas;
