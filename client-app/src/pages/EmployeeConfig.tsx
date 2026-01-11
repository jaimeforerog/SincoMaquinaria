import { useEffect, useState } from 'react';
import {
    Box, Typography, Button, Table, TableBody, TableCell, TableContainer,
    TableHead, TableRow, Paper, Dialog, DialogTitle,
    DialogContent, DialogActions, TextField, Grid, CircularProgress,
    FormControl, InputLabel, Select, MenuItem, SelectChangeEvent,
    Switch, FormControlLabel, IconButton
} from '@mui/material';
import { Add, Edit } from '@mui/icons-material';
import { useAuthFetch } from '../hooks/useAuthFetch';

export interface Empleado {
    id: string;
    nombre: string;
    identificacion: string;
    cargo: string;
    estado: string;
}

const EmployeeConfig = () => {
    const authFetch = useAuthFetch();
    const [empleados, setEmpleados] = useState<Empleado[]>([]);
    const [loading, setLoading] = useState(true);
    const [openDialog, setOpenDialog] = useState(false);
    const [editingId, setEditingId] = useState<string | null>(null);

    // Form State
    const [nombre, setNombre] = useState('');
    const [identificacion, setIdentificacion] = useState('');
    const [cargo, setCargo] = useState('');
    const [estado, setEstado] = useState('Activo');

    useEffect(() => {
        fetchEmpleados();
    }, []);

    const fetchEmpleados = async () => {
        setLoading(true);
        try {
            const res = await authFetch('/empleados');
            if (res.ok) {
                const data = await res.json();
                setEmpleados(data);
            }
        } catch (error) {
            console.error("Error fetching employees", error);
        } finally {
            setLoading(false);
        }
    };

    const handleOpenDialog = () => {
        setNombre('');
        setIdentificacion('');
        setCargo('');
        setEstado('Activo');
        setEditingId(null);
        setOpenDialog(true);
    };

    const startEditing = (emp: Empleado) => {
        setNombre(emp.nombre);
        setIdentificacion(emp.identificacion);
        setCargo(emp.cargo);
        setEstado(emp.estado);
        setEditingId(emp.id);
        setOpenDialog(true);
    };

    const handleCloseDialog = () => {
        setOpenDialog(false);
    };

    const handleSave = async () => {
        if (!nombre || !identificacion || !cargo) return;

        try {
            const method = editingId ? 'PUT' : 'POST';
            const url = editingId ? `/empleados/${editingId}` : '/empleados';

            const res = await authFetch(url, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    Nombre: nombre,
                    Identificacion: identificacion,
                    Cargo: cargo,
                    Estado: estado
                })
            });

            if (res.ok) {
                fetchEmpleados();
                handleCloseDialog();
            } else {
                alert("Error al guardar empleado");
            }
        } catch (error) {
            console.error("Error saving employee", error);
            alert("Error de conexión");
        }
    };

    const handleEstadoChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setEstado(event.target.checked ? 'Activo' : 'Inactivo');
    };

    return (
        <Box>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                <Typography variant="h4" fontWeight="bold" color="text.primary">
                    Gestión de Empleados
                </Typography>
                <Button variant="contained" startIcon={<Add />} onClick={handleOpenDialog}>
                    Nuevo Empleado
                </Button>
            </Box>

            {loading ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
                    <CircularProgress />
                </Box>
            ) : (
                <TableContainer component={Paper}>
                    <Table>
                        <TableHead sx={{ bgcolor: 'action.hover' }}>
                            <TableRow>
                                <TableCell><strong>Nombre</strong></TableCell>
                                <TableCell><strong>Identificación</strong></TableCell>
                                <TableCell><strong>Cargo</strong></TableCell>
                                <TableCell><strong>Estado</strong></TableCell>
                                <TableCell align="center"><strong>Acciones</strong></TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {empleados.map((emp) => (
                                <TableRow key={emp.id} hover>
                                    <TableCell>{emp.nombre}</TableCell>
                                    <TableCell>{emp.identificacion}</TableCell>
                                    <TableCell>{emp.cargo}</TableCell>
                                    <TableCell>{emp.estado}</TableCell>
                                    <TableCell align="center">
                                        <IconButton onClick={() => startEditing(emp)} color="primary" size="small">
                                            <Edit />
                                        </IconButton>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            )}

            <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
                <DialogTitle>{editingId ? 'Editar Empleado' : 'Nuevo Empleado'}</DialogTitle>
                <DialogContent dividers>
                    <Grid container spacing={2} sx={{ pt: 1 }}>
                        <Grid item xs={12}>
                            <TextField
                                label="Nombre Completo"
                                fullWidth
                                value={nombre}
                                onChange={(e) => setNombre(e.target.value)}
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                label="Identificación (Cédula)"
                                fullWidth
                                value={identificacion}
                                onChange={(e) => setIdentificacion(e.target.value)}
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <FormControl fullWidth>
                                <InputLabel>Cargo</InputLabel>
                                <Select
                                    value={cargo}
                                    label="Cargo"
                                    onChange={(e) => setCargo(e.target.value)}
                                >
                                    <MenuItem value="Operario">Operario</MenuItem>
                                    <MenuItem value="Conductor">Conductor</MenuItem>
                                    <MenuItem value="Mecanico">Mecánico</MenuItem>
                                </Select>
                            </FormControl>
                        </Grid>
                        <Grid item xs={12}>
                            {editingId ? (
                                <FormControlLabel
                                    control={
                                        <Switch
                                            checked={estado === 'Activo'}
                                            onChange={handleEstadoChange}
                                            color="primary"
                                        />
                                    }
                                    label={estado}
                                />
                            ) : null}
                        </Grid>
                    </Grid>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCloseDialog} color="inherit">Cancelar</Button>
                    <Button onClick={handleSave} variant="contained" color="primary" disabled={!nombre || !identificacion}>
                        Guardar
                    </Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
};

export default EmployeeConfig;
