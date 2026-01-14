import { useEffect, useState } from 'react';
import {
    Box, Typography, Button, Table, TableBody, TableCell, TableContainer,
    TableHead, TableRow, Paper, IconButton, Dialog, DialogTitle,
    DialogContent, DialogActions, TextField, Grid, Chip, CircularProgress,
    MenuItem, FormControl, InputLabel, Select
} from '@mui/material';
import { Edit, Add } from '@mui/icons-material';
import { useAuthFetch } from '../hooks/useAuthFetch';

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

const EquipmentConfig = () => {
    const authFetch = useAuthFetch();
    const [equipos, setEquipos] = useState<Equipo[]>([]);
    const [loading, setLoading] = useState(true);
    const [openEdit, setOpenEdit] = useState(false);
    const [openCreate, setOpenCreate] = useState(false);
    const [currentEquipo, setCurrentEquipo] = useState<Equipo | null>(null);
    const [newEquipo, setNewEquipo] = useState<any>({
        id: '',
        placa: '',
        descripcion: '',
        marca: '',
        modelo: '',
        serie: '',
        codigo: '',
        tipoMedidorId: '',
        tipoMedidorId2: '',
        grupo: '',
        rutina: '',
        lecturaInicial1: '',
        fechaInicial1: '',
        lecturaInicial2: '',
        fechaInicial2: ''
    });

    const [medidores, setMedidores] = useState<any[]>([]);
    const [rutinas, setRutinas] = useState<any[]>([]);
    const [grupos, setGrupos] = useState<any[]>([]);

    // Filter/Search states could be added here

    useEffect(() => {
        fetchEquipos();
        fetchAuxDATA();
    }, [authFetch]);

    const fetchAuxDATA = async () => {
        try {
            const [resMed, resRut, resGrup] = await Promise.all([
                authFetch('/configuracion/medidores'),
                authFetch('/rutinas?pageSize=1000'),
                authFetch('/configuracion/grupos')
            ]);

            if (resMed.ok) {
                const data = await resMed.json();
                setMedidores(data.filter((m: any) => m.activo));
            }
            if (resRut.ok) {
                const data = await resRut.json();
                // Handle different response structures if needed
                const items = data.data || data;
                setRutinas(items);
            }
            if (resGrup.ok) {
                const data = await resGrup.json();
                setGrupos(data.filter((g: any) => g.activo));
            }
        } catch (err) {
            console.error('Error loading aux data', err);
        }
    };

    const fetchEquipos = async () => {
        setLoading(true);
        try {
            const res = await authFetch('/equipos');
            if (res.ok) {
                const response = await res.json();
                // Handle paginated response
                const data = response.data || response;
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

        if (!currentEquipo.grupo || !currentEquipo.rutina) {
            alert("Grupo de Mantenimiento y Rutina son obligatorios");
            return;
        }

        try {
            // Note: Sending 'Marca' and 'Modelo' as empty or existing values if needed by backend, 
            // but for now ignoring them in the UI as requested.
            const res = await authFetch(`/equipos/${currentEquipo.id}`, {
                method: 'PUT',
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

    const handleCreateClick = () => {
        setNewEquipo({
            id: '',
            placa: '',
            descripcion: '',
            marca: '',
            modelo: '',
            serie: '',
            codigo: '',
            tipoMedidorId: '',
            tipoMedidorId2: '',
            grupo: '',
            rutina: '',
            lecturaInicial1: '',
            fechaInicial1: new Date().toISOString().split('T')[0], // Default today
            lecturaInicial2: '',
            fechaInicial2: new Date().toISOString().split('T')[0]
        });
        setOpenCreate(true);
    };

    const handleCloseCreate = () => {
        setOpenCreate(false);
    };

    const handleCreate = async () => {
        if (!newEquipo.placa || !newEquipo.descripcion) {
            alert("Placa y Descripción son obligatorios");
            return;
        }

        if (!newEquipo.grupo || !newEquipo.rutina) {
            alert("Grupo de Mantenimiento y Rutina son obligatorios");
            return;
        }

        try {
            const res = await authFetch('/equipos', {
                method: 'POST',
                body: JSON.stringify({
                    Placa: newEquipo.placa,
                    Descripcion: newEquipo.descripcion,
                    Marca: newEquipo.marca || "GENERICO",
                    Modelo: newEquipo.modelo || "GENERICO",
                    Serie: newEquipo.serie,
                    Codigo: newEquipo.codigo,
                    TipoMedidorId: newEquipo.tipoMedidorId,
                    TipoMedidorId2: newEquipo.tipoMedidorId2,
                    Grupo: newEquipo.grupo,
                    Rutina: newEquipo.rutina,
                    LecturaInicial1: newEquipo.tipoMedidorId && newEquipo.lecturaInicial1 ? Number(newEquipo.lecturaInicial1) : null,
                    FechaInicial1: newEquipo.tipoMedidorId && newEquipo.fechaInicial1 ? newEquipo.fechaInicial1 : null,
                    LecturaInicial2: newEquipo.tipoMedidorId2 && newEquipo.lecturaInicial2 ? Number(newEquipo.lecturaInicial2) : null,
                    FechaInicial2: newEquipo.tipoMedidorId2 && newEquipo.fechaInicial2 ? newEquipo.fechaInicial2 : null
                })
            });

            if (res.ok) {
                fetchEquipos();
                handleCloseCreate();
            } else {
                const err = await res.text();
                alert("Error al crear equipo: " + err);
            }
        } catch (error) {
            console.error("Error creating equipment", error);
            alert("Error de conexión");
        }
    };

    const handleChange = (field: keyof Equipo, value: string) => {
        if (currentEquipo) {
            setCurrentEquipo({ ...currentEquipo, [field]: value });
        }
    };

    const handleNewChange = (field: string, value: any) => {
        setNewEquipo({ ...newEquipo, [field]: value });
    };

    return (
        <Box>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h4" fontWeight="bold" color="text.primary">
                    Configuración de Equipos
                </Typography>
                <Button
                    variant="contained"
                    startIcon={<Add />}
                    onClick={handleCreateClick}
                >
                    Nuevo Equipo
                </Button>
            </Box>

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
                            <Grid size={{ xs: 12, sm: 6 }}>
                                <TextField
                                    label="Placa"
                                    fullWidth
                                    value={currentEquipo.placa}
                                    InputProps={{ readOnly: true }}
                                    disabled
                                />
                            </Grid>
                            <Grid size={{ xs: 12, sm: 6 }}>
                                <TextField
                                    label="Descripción"
                                    fullWidth
                                    value={currentEquipo.descripcion}
                                    onChange={(e) => handleChange('descripcion', e.target.value)}
                                />
                            </Grid>
                            <Grid size={{ xs: 12, sm: 6 }}>
                                <TextField
                                    label="Marca"
                                    fullWidth
                                    value={currentEquipo.marca || ''}
                                    onChange={(e) => handleChange('marca', e.target.value)}
                                />
                            </Grid>
                            <Grid size={{ xs: 12, sm: 6 }}>
                                <TextField
                                    label="Modelo"
                                    fullWidth
                                    value={currentEquipo.modelo || ''}
                                    onChange={(e) => handleChange('modelo', e.target.value)}
                                />
                            </Grid>
                            <Grid size={{ xs: 12, sm: 6 }}>
                                <TextField
                                    label="Serie"
                                    fullWidth
                                    value={currentEquipo.serie}
                                    onChange={(e) => handleChange('serie', e.target.value)}
                                />
                            </Grid>
                            <Grid size={{ xs: 12, sm: 6 }}>
                                <TextField
                                    label="Código Interno"
                                    fullWidth
                                    value={currentEquipo.codigo}
                                    onChange={(e) => handleChange('codigo', e.target.value)}
                                />
                            </Grid>
                            <Grid size={{ xs: 12, sm: 6 }}>
                                <FormControl fullWidth>
                                    <InputLabel id="edit-grupo-label">Grupo de Mantenimiento</InputLabel>
                                    <Select
                                        labelId="edit-grupo-label"
                                        value={currentEquipo.grupo || ''}
                                        label="Grupo de Mantenimiento"
                                        onChange={(e) => handleChange('grupo', e.target.value as string)}
                                    >
                                        <MenuItem value=""><em>Ninguno</em></MenuItem>
                                        {grupos.map((g: any) => (
                                            <MenuItem key={g.codigo} value={g.nombre}>{g.nombre}</MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                            </Grid>
                            <Grid size={{ xs: 12, sm: 6 }}>
                                <FormControl fullWidth>
                                    <InputLabel id="edit-rutina-label">Rutina Asignada</InputLabel>
                                    <Select
                                        labelId="edit-rutina-label"
                                        value={currentEquipo.rutina || ''}
                                        label="Rutina Asignada"
                                        onChange={(e) => handleChange('rutina', e.target.value as string)}
                                    >
                                        <MenuItem value=""><em>Ninguna</em></MenuItem>
                                        {rutinas.map((r: any) => (
                                            <MenuItem key={r.id} value={r.id}>{r.descripcion}</MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                            </Grid>
                            <Grid size={{ xs: 12, sm: 6 }}>
                                <FormControl fullWidth>
                                    <InputLabel id="edit-medidor1-label">Medidor Principal</InputLabel>
                                    <Select
                                        labelId="edit-medidor1-label"
                                        value={currentEquipo.tipoMedidorId || ''}
                                        label="Medidor Principal"
                                        onChange={(e) => handleChange('tipoMedidorId', e.target.value as string)}
                                    >
                                        <MenuItem value=""><em>Ninguno</em></MenuItem>
                                        {medidores.map((m: any) => (
                                            <MenuItem key={m.codigo} value={m.codigo}>{m.nombre} ({m.unidad})</MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                            </Grid>
                            <Grid size={{ xs: 12, sm: 6 }}>
                                <FormControl fullWidth>
                                    <InputLabel id="edit-medidor2-label">Medidor Secundario</InputLabel>
                                    <Select
                                        labelId="edit-medidor2-label"
                                        value={currentEquipo.tipoMedidorId2 || ''}
                                        label="Medidor Secundario"
                                        onChange={(e) => handleChange('tipoMedidorId2', e.target.value as string)}
                                    >
                                        <MenuItem value=""><em>Ninguno</em></MenuItem>
                                        {medidores.map((m: any) => (
                                            <MenuItem key={m.codigo} value={m.codigo}>{m.nombre} ({m.unidad})</MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                            </Grid>
                        </Grid>
                    )}
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCloseEdit} color="inherit">Cancelar</Button>
                    <Button onClick={handleSave} variant="contained" color="primary">Guardar</Button>
                </DialogActions>
            </Dialog>

            {/* Create Dialog */}
            <Dialog open={openCreate} onClose={handleCloseCreate} maxWidth="md" fullWidth>
                <DialogTitle>Crear Nuevo Equipo</DialogTitle>
                <DialogContent dividers>
                    <Grid container spacing={2} sx={{ pt: 1 }}>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField
                                label="Placa"
                                fullWidth
                                required
                                value={newEquipo.placa}
                                onChange={(e) => handleNewChange('placa', e.target.value)}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField
                                label="Descripción"
                                fullWidth
                                required
                                value={newEquipo.descripcion}
                                onChange={(e) => handleNewChange('descripcion', e.target.value)}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField
                                label="Marca"
                                fullWidth
                                value={newEquipo.marca}
                                onChange={(e) => handleNewChange('marca', e.target.value)}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField
                                label="Modelo"
                                fullWidth
                                value={newEquipo.modelo}
                                onChange={(e) => handleNewChange('modelo', e.target.value)}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField
                                label="Serie"
                                fullWidth
                                value={newEquipo.serie}
                                onChange={(e) => handleNewChange('serie', e.target.value)}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField
                                label="Código Interno"
                                fullWidth
                                value={newEquipo.codigo}
                                onChange={(e) => handleNewChange('codigo', e.target.value)}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <FormControl fullWidth>
                                <InputLabel id="create-grupo-label">Grupo de Mantenimiento</InputLabel>
                                <Select
                                    labelId="create-grupo-label"
                                    value={newEquipo.grupo || ''}
                                    label="Grupo de Mantenimiento"
                                    onChange={(e) => handleNewChange('grupo', e.target.value as string)}
                                >
                                    <MenuItem value=""><em>Ninguno</em></MenuItem>
                                    {grupos.map((g: any) => (
                                        <MenuItem key={g.codigo} value={g.nombre}>{g.nombre}</MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <FormControl fullWidth>
                                <InputLabel id="create-rutina-label">Rutina Asignada</InputLabel>
                                <Select
                                    labelId="create-rutina-label"
                                    value={newEquipo.rutina || ''}
                                    label="Rutina Asignada"
                                    onChange={(e) => handleNewChange('rutina', e.target.value as string)}
                                >
                                    <MenuItem value=""><em>Ninguna</em></MenuItem>
                                    {rutinas.map((r: any) => (
                                        <MenuItem key={r.id} value={r.id}>{r.descripcion}</MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <FormControl fullWidth>
                                <InputLabel id="create-medidor1-label">Medidor Principal</InputLabel>
                                <Select
                                    labelId="create-medidor1-label"
                                    value={newEquipo.tipoMedidorId || ''}
                                    label="Medidor Principal"
                                    onChange={(e) => handleNewChange('tipoMedidorId', e.target.value as string)}
                                >
                                    <MenuItem value=""><em>Ninguno</em></MenuItem>
                                    {medidores.map((m: any) => (
                                        <MenuItem key={m.codigo} value={m.codigo}>{m.nombre} ({m.unidad})</MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Grid>
                        {newEquipo.tipoMedidorId && (
                            <>
                                <Grid size={{ xs: 12, sm: 3 }}>
                                    <TextField
                                        label="Lectura Inicial M1"
                                        type="number"
                                        fullWidth
                                        value={newEquipo.lecturaInicial1 || ''}
                                        onChange={(e) => handleNewChange('lecturaInicial1', e.target.value)}
                                    />
                                </Grid>
                                <Grid size={{ xs: 12, sm: 3 }}>
                                    <TextField
                                        label="Fecha Inicial M1"
                                        type="date"
                                        fullWidth
                                        InputLabelProps={{ shrink: true }}
                                        value={newEquipo.fechaInicial1 || ''}
                                        onChange={(e) => handleNewChange('fechaInicial1', e.target.value)}
                                    />
                                </Grid>
                            </>
                        )}
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <FormControl fullWidth>
                                <InputLabel id="create-medidor2-label">Medidor Secundario</InputLabel>
                                <Select
                                    labelId="create-medidor2-label"
                                    value={newEquipo.tipoMedidorId2 || ''}
                                    label="Medidor Secundario"
                                    onChange={(e) => handleNewChange('tipoMedidorId2', e.target.value as string)}
                                >
                                    <MenuItem value=""><em>Ninguno</em></MenuItem>
                                    {medidores.map((m: any) => (
                                        <MenuItem key={m.codigo} value={m.codigo}>{m.nombre} ({m.unidad})</MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Grid>
                        {newEquipo.tipoMedidorId2 && (
                            <>
                                <Grid size={{ xs: 12, sm: 3 }}>
                                    <TextField
                                        label="Lectura Inicial M2"
                                        type="number"
                                        fullWidth
                                        value={newEquipo.lecturaInicial2 || ''}
                                        onChange={(e) => handleNewChange('lecturaInicial2', e.target.value)}
                                    />
                                </Grid>
                                <Grid size={{ xs: 12, sm: 3 }}>
                                    <TextField
                                        label="Fecha Inicial M2"
                                        type="date"
                                        fullWidth
                                        InputLabelProps={{ shrink: true }}
                                        value={newEquipo.fechaInicial2 || ''}
                                        onChange={(e) => handleNewChange('fechaInicial2', e.target.value)}
                                    />
                                </Grid>
                            </>
                        )}
                    </Grid>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCloseCreate} color="inherit">Cancelar</Button>
                    <Button onClick={handleCreate} variant="contained" color="primary">Crear</Button>
                </DialogActions>
            </Dialog>

        </Box>
    );
};

export default EquipmentConfig;
