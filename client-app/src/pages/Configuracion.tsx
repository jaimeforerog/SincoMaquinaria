import React, { useState, useEffect } from 'react';
import {
    Container, Paper, Tabs, Tab, Box, Typography, TextField, Button,
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
    Switch, IconButton, Chip, Alert
} from '@mui/material';
import { Edit, Save, Cancel, Add } from '@mui/icons-material';
import { TipoMedidor, GrupoMantenimiento, TipoFalla, CausaFalla } from '../types';
import { useAuthFetch } from '../hooks/useAuthFetch';
import EmployeeConfig from './EmployeeConfig';

const MedidoresPanel = () => {
    const authFetch = useAuthFetch();
    const [tipos, setTipos] = useState<TipoMedidor[]>([]);
    const [nuevoTipo, setNuevoTipo] = useState({ nombre: '', unidad: '' });
    const [error, setError] = useState('');
    const [editingId, setEditingId] = useState<string | null>(null);
    const [editForm, setEditForm] = useState({ nombre: '', unidad: '' });

    useEffect(() => { fetchTipos(); }, []);

    const fetchTipos = async () => {
        try {
            const res = await authFetch('/configuracion/medidores');
            if (res.ok) setTipos(await res.json());
        } catch (err: any) { setError(err.message); }
    };

    const handleCrear = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!nuevoTipo.nombre || !nuevoTipo.unidad) return setError('Campos obligatorios');
        try {
            const res = await authFetch('/configuracion/medidores', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(nuevoTipo)
            });
            if (res.ok) {
                setNuevoTipo({ nombre: '', unidad: '' });
                fetchTipos();
            }
        } catch (err) { setError('Error al crear'); }
    };

    const startEditing = (t: TipoMedidor) => {
        setEditingId(t.codigo);
        setEditForm({ nombre: t.nombre, unidad: t.unidad });
    };

    const cancelEditing = () => {
        setEditingId(null);
        setEditForm({ nombre: '', unidad: '' });
    };

    const saveEdit = async (codigo: string) => {
        try {
            const res = await authFetch(`/configuracion/medidores/${codigo}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(editForm)
            });
            if (res.ok) {
                fetchTipos();
                setEditingId(null);
            }
        } catch (e) { setError('Error al guardar cambios'); }
    };

    const toggleEstado = async (codigo: string, estadoActual: boolean) => {
        await authFetch(`/configuracion/medidores/${codigo}/estado`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ activo: !estadoActual })
        });
        fetchTipos();
    };

    return (
        <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 2fr', gap: 3 }}>
            <Paper elevation={3} sx={{ p: 3, borderRadius: 2, bgcolor: 'background.paper' }}>
                <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Add color="primary" /> Nuevo Tipo
                </Typography>
                {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
                <form onSubmit={handleCrear} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                    <TextField label="Nombre del Medidor" fullWidth variant="outlined" size="small"
                        placeholder="Ej: Horas de Motor"
                        value={nuevoTipo.nombre} onChange={e => setNuevoTipo({ ...nuevoTipo, nombre: e.target.value })} />
                    <TextField label="Unidad de Medida" fullWidth variant="outlined" size="small"
                        placeholder="Ej: hr"
                        value={nuevoTipo.unidad} onChange={e => setNuevoTipo({ ...nuevoTipo, unidad: e.target.value.toUpperCase() })} />
                    <Button type="submit" variant="contained" color="primary" startIcon={<Add />}>
                        Crear Medidor
                    </Button>
                </form>
            </Paper>

            <Paper elevation={3} sx={{ p: 3, borderRadius: 2, bgcolor: 'background.paper' }}>
                <Typography variant="h6" gutterBottom>Tipos Existentes</Typography>
                <TableContainer>
                    <Table size="small">
                        <TableHead>
                            <TableRow>
                                <TableCell><strong>Nombre</strong></TableCell>
                                <TableCell><strong>Unidad</strong></TableCell>
                                <TableCell align="center"><strong>Estado</strong></TableCell>
                                <TableCell align="center"><strong>Acciones</strong></TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {tipos.map(t => (
                                <TableRow key={t.codigo} hover>
                                    <TableCell>
                                        {editingId === t.codigo ? (
                                            <TextField size="small" value={editForm.nombre} onChange={e => setEditForm({ ...editForm, nombre: e.target.value })} />
                                        ) : t.nombre}
                                    </TableCell>
                                    <TableCell>
                                        {editingId === t.codigo ? (
                                            <TextField size="small" sx={{ width: 80 }} value={editForm.unidad} onChange={e => setEditForm({ ...editForm, unidad: e.target.value.toUpperCase() })} />
                                        ) : (
                                            <Chip label={t.unidad} size="small" color="primary" variant="outlined" />
                                        )}
                                    </TableCell>
                                    <TableCell align="center">
                                        <Switch
                                            checked={t.activo}
                                            onChange={() => toggleEstado(t.codigo, t.activo)}
                                            color="success"
                                        />
                                    </TableCell>
                                    <TableCell align="center">
                                        {editingId === t.codigo ? (
                                            <Box>
                                                <IconButton onClick={() => saveEdit(t.codigo)} color="success"><Save /></IconButton>
                                                <IconButton onClick={cancelEditing} color="error"><Cancel /></IconButton>
                                            </Box>
                                        ) : (
                                            <IconButton onClick={() => startEditing(t)}><Edit /></IconButton>
                                        )}
                                    </TableCell>
                                </TableRow>
                            ))}
                            {tipos.length === 0 && (
                                <TableRow>
                                    <TableCell colSpan={4} align="center">No hay medidores configurados.</TableCell>
                                </TableRow>
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Paper>
        </Box>
    );
};

const GruposPanel = () => {
    const authFetch = useAuthFetch();
    const [grupos, setGrupos] = useState<GrupoMantenimiento[]>([]);
    const [nuevo, setNuevo] = useState({ nombre: '', descripcion: '' });
    const [error, setError] = useState('');
    const [editingId, setEditingId] = useState<string | null>(null);
    const [editForm, setEditForm] = useState({ nombre: '', descripcion: '' });

    useEffect(() => { fetchGrupos(); }, []);

    const fetchGrupos = async () => {
        const res = await authFetch('/configuracion/grupos');
        if (res.ok) setGrupos(await res.json());
    };

    const handleCrear = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!nuevo.nombre) return setError('Nombre obligatorio');
        const res = await authFetch('/configuracion/grupos', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(nuevo)
        });
        if (res.ok) {
            setNuevo({ nombre: '', descripcion: '' });
            fetchGrupos();
        } else {
            setError(await res.text());
        }
    };

    const startEditing = (g: GrupoMantenimiento) => {
        setEditingId(g.codigo);
        setEditForm({ nombre: g.nombre, descripcion: g.descripcion });
    };

    const cancelEditing = () => {
        setEditingId(null);
        setEditForm({ nombre: '', descripcion: '' });
    };

    const saveEdit = async (codigo: string) => {
        try {
            const res = await authFetch(`/configuracion/grupos/${codigo}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(editForm)
            });
            if (res.ok) {
                fetchGrupos();
                setEditingId(null);
            }
        } catch (e) { setError('Error al guardar cambios'); }
    };

    const toggleEstado = async (codigo: string, estadoActual: boolean) => {
        await authFetch(`/configuracion/grupos/${codigo}/estado`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ activo: !estadoActual })
        });
        fetchGrupos();
    };

    return (
        <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 2fr', gap: 3 }}>
            <Paper elevation={3} sx={{ p: 3, borderRadius: 2, bgcolor: 'background.paper' }}>
                <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Add color="primary" /> Nuevo Grupo
                </Typography>
                {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
                <form onSubmit={handleCrear} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                    <TextField label="Nombre del Grupo" fullWidth variant="outlined" size="small"
                        placeholder="Ej: Preventivo Motor"
                        value={nuevo.nombre} onChange={e => setNuevo({ ...nuevo, nombre: e.target.value })} />
                    <TextField label="Descripción" fullWidth variant="outlined" size="small" multiline rows={3}
                        placeholder="Descripción opcional..."
                        value={nuevo.descripcion} onChange={e => setNuevo({ ...nuevo, descripcion: e.target.value })} />
                    <Button type="submit" variant="contained" color="primary" startIcon={<Add />}>
                        Crear Grupo
                    </Button>
                </form>
            </Paper>

            <Paper elevation={3} sx={{ p: 3, borderRadius: 2, bgcolor: 'background.paper' }}>
                <Typography variant="h6" gutterBottom>Grupos Definidos</Typography>
                <TableContainer>
                    <Table size="small">
                        <TableHead>
                            <TableRow>
                                <TableCell><strong>Nombre</strong></TableCell>
                                <TableCell><strong>Descripción</strong></TableCell>
                                <TableCell align="center"><strong>Estado</strong></TableCell>
                                <TableCell align="center"><strong>Acciones</strong></TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {grupos.map(g => (
                                <TableRow key={g.codigo} hover>
                                    <TableCell>
                                        {editingId === g.codigo ? (
                                            <TextField size="small" value={editForm.nombre} onChange={e => setEditForm({ ...editForm, nombre: e.target.value })} />
                                        ) : g.nombre}
                                    </TableCell>
                                    <TableCell>
                                        {editingId === g.codigo ? (
                                            <TextField size="small" fullWidth value={editForm.descripcion} onChange={e => setEditForm({ ...editForm, descripcion: e.target.value })} />
                                        ) : (g.descripcion || 'Sin descripción')}
                                    </TableCell>
                                    <TableCell align="center">
                                        <Switch
                                            checked={g.activo}
                                            onChange={() => toggleEstado(g.codigo, g.activo)}
                                            color="primary"
                                        />
                                    </TableCell>
                                    <TableCell align="center">
                                        {editingId === g.codigo ? (
                                            <Box>
                                                <IconButton onClick={() => saveEdit(g.codigo)} color="success"><Save /></IconButton>
                                                <IconButton onClick={cancelEditing} color="error"><Cancel /></IconButton>
                                            </Box>
                                        ) : (
                                            <IconButton onClick={() => startEditing(g)} color="primary"><Edit /></IconButton>
                                        )}
                                    </TableCell>
                                </TableRow>
                            ))}
                            {grupos.length === 0 && (
                                <TableRow>
                                    <TableCell colSpan={4} align="center">No hay grupos creados aún.</TableCell>
                                </TableRow>
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Paper>
        </Box>
    );
};

const FallasPanel = () => {
    const authFetch = useAuthFetch();
    const [fallas, setFallas] = useState<TipoFalla[]>([]);
    const [nuevo, setNuevo] = useState({ descripcion: '', prioridad: 'Media' });
    const [error, setError] = useState('');

    useEffect(() => { fetchFallas(); }, []);

    const fetchFallas = async () => {
        try {
            const res = await authFetch('/configuracion/fallas');
            if (res.ok) setFallas(await res.json());
        } catch (e) { setError('Error al cargar fallas'); }
    };

    const handleCrear = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!nuevo.descripcion) return setError('Descripción obligatoria');
        try {
            const res = await authFetch('/configuracion/fallas', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(nuevo)
            });
            if (res.ok) {
                setNuevo({ descripcion: '', prioridad: 'Media' });
                fetchFallas();
            } else {
                setError(await res.text());
            }
        } catch (e) { setError('Error al crear falla'); }
    };

    return (
        <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 2fr', gap: 3 }}>
            <Paper elevation={3} sx={{ p: 3, borderRadius: 2, bgcolor: 'background.paper' }}>
                <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Add color="primary" /> Nuevo Tipo de Falla
                </Typography>
                {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
                <form onSubmit={handleCrear} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                    <TextField label="Descripción de Falla" fullWidth variant="outlined" size="small"
                        placeholder="Ej: Desgaste"
                        value={nuevo.descripcion} onChange={e => setNuevo({ ...nuevo, descripcion: e.target.value })} />

                    <TextField
                        select
                        label="Prioridad"
                        value={nuevo.prioridad}
                        onChange={(e) => setNuevo({ ...nuevo, prioridad: e.target.value })}
                        fullWidth
                        size="small"
                        SelectProps={{ native: true }}
                    >
                        <option value="Alta">Alta</option>
                        <option value="Media">Media</option>
                        <option value="Baja">Baja</option>
                    </TextField>

                    <Button type="submit" variant="contained" color="primary" startIcon={<Add />}>
                        Crear Tipo de Falla
                    </Button>
                </form>
            </Paper>

            <Paper elevation={3} sx={{ p: 3, borderRadius: 2, bgcolor: 'background.paper' }}>
                <Typography variant="h6" gutterBottom>Tipos de Falla</Typography>
                <TableContainer>
                    <Table size="small">
                        <TableHead>
                            <TableRow>
                                <TableCell><strong>Descripción</strong></TableCell>
                                <TableCell><strong>Prioridad</strong></TableCell>
                                <TableCell align="center"><strong>Estado</strong></TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {fallas.map(f => (
                                <TableRow key={f.codigo} hover>
                                    <TableCell>{f.descripcion}</TableCell>
                                    <TableCell>
                                        <Chip
                                            label={f.prioridad}
                                            size="small"
                                            color={f.prioridad === 'Alta' ? 'error' : f.prioridad === 'Media' ? 'warning' : 'success'}
                                            variant="outlined"
                                        />
                                    </TableCell>
                                    <TableCell align="center">
                                        <Switch checked={f.activo} disabled color="success" />
                                    </TableCell>
                                </TableRow>
                            ))}
                            {fallas.length === 0 && (
                                <TableRow>
                                    <TableCell colSpan={3} align="center">No hay tipos de falla configurados.</TableCell>
                                </TableRow>
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Paper>
        </Box>
    );
};

const CausasFallaPanel = () => {
    const authFetch = useAuthFetch();
    const [causas, setCausas] = useState<CausaFalla[]>([]);
    const [nuevo, setNuevo] = useState({ descripcion: '' });
    const [error, setError] = useState('');
    const [editingId, setEditingId] = useState<string | null>(null);
    const [editForm, setEditForm] = useState({ descripcion: '' });

    useEffect(() => { fetchCausas(); }, []);

    const fetchCausas = async () => {
        try {
            const res = await authFetch('/configuracion/causas-falla');
            if (res.ok) setCausas(await res.json());
        } catch (e) { setError('Error al cargar causas de falla'); }
    };

    const handleCrear = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!nuevo.descripcion) return setError('Descripción obligatoria');
        try {
            const res = await authFetch('/configuracion/causas-falla', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(nuevo)
            });
            if (res.ok) {
                setNuevo({ descripcion: '' });
                fetchCausas();
            } else {
                setError(await res.text());
            }
        } catch (e) { setError('Error al crear causa de falla'); }
    };

    const startEditing = (c: CausaFalla) => {
        setEditingId(c.codigo);
        setEditForm({ descripcion: c.descripcion });
    };

    const cancelEditing = () => {
        setEditingId(null);
        setEditForm({ descripcion: '' });
    };

    const saveEdit = async (codigo: string) => {
        try {
            const res = await authFetch(`/configuracion/causas-falla/${codigo}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(editForm)
            });
            if (res.ok) {
                fetchCausas();
                setEditingId(null);
            }
        } catch (e) { setError('Error al guardar cambios'); }
    };

    const toggleEstado = async (codigo: string, estadoActual: boolean) => {
        await authFetch(`/configuracion/causas-falla/${codigo}/estado`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ activo: !estadoActual })
        });
        fetchCausas();
    };

    return (
        <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 2fr', gap: 3 }}>
            <Paper elevation={3} sx={{ p: 3, borderRadius: 2, bgcolor: 'background.paper' }}>
                <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Add color="primary" /> Nueva Causa
                </Typography>
                {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
                <form onSubmit={handleCrear} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                    <TextField label="Descripción de Causa" fullWidth variant="outlined" size="small"
                        placeholder="Ej: Falta de mantenimiento preventivo"
                        value={nuevo.descripcion} onChange={e => setNuevo({ ...nuevo, descripcion: e.target.value })} />
                    <Button type="submit" variant="contained" color="primary" startIcon={<Add />}>
                        Crear Causa
                    </Button>
                </form>
            </Paper>

            <Paper elevation={3} sx={{ p: 3, borderRadius: 2, bgcolor: 'background.paper' }}>
                <Typography variant="h6" gutterBottom>Causas de Falla</Typography>
                <TableContainer>
                    <Table size="small">
                        <TableHead>
                            <TableRow>
                                <TableCell><strong>Descripción</strong></TableCell>
                                <TableCell align="center"><strong>Estado</strong></TableCell>
                                <TableCell align="center"><strong>Acciones</strong></TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {causas.map(c => (
                                <TableRow key={c.codigo} hover>
                                    <TableCell>
                                        {editingId === c.codigo ? (
                                            <TextField size="small" fullWidth value={editForm.descripcion} onChange={e => setEditForm({ ...editForm, descripcion: e.target.value })} />
                                        ) : c.descripcion}
                                    </TableCell>
                                    <TableCell align="center">
                                        <Switch
                                            checked={c.activo}
                                            onChange={() => toggleEstado(c.codigo, c.activo)}
                                            color="success"
                                        />
                                    </TableCell>
                                    <TableCell align="center">
                                        {editingId === c.codigo ? (
                                            <Box>
                                                <IconButton onClick={() => saveEdit(c.codigo)} color="success"><Save /></IconButton>
                                                <IconButton onClick={cancelEditing} color="error"><Cancel /></IconButton>
                                            </Box>
                                        ) : (
                                            <IconButton onClick={() => startEditing(c)} color="primary"><Edit /></IconButton>
                                        )}
                                    </TableCell>
                                </TableRow>
                            ))}
                            {causas.length === 0 && (
                                <TableRow>
                                    <TableCell colSpan={3} align="center">No hay causas de falla configuradas.</TableCell>
                                </TableRow>
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Paper>
        </Box>
    );
};

const Configuracion = () => {
    const [activeTab, setActiveTab] = useState(0);

    const handleChange = (_: React.SyntheticEvent, newValue: number) => {
        setActiveTab(newValue);
    };

    return (
        <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
            <Box sx={{ pb: 3 }}>
                <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 'bold' }}>
                    Configuraciones
                </Typography>
                <Typography variant="subtitle1" color="text.secondary">
                    Gestión de parámetros globales del sistema
                </Typography>
            </Box>

            <Box sx={{ width: '100%' }}>
                <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                    <Tabs value={activeTab} onChange={handleChange} aria-label="config tabs">
                        <Tab label="Tipos de Medidor" />
                        <Tab label="Grupos Mantenimiento" />
                        <Tab label="Tipos de Falla" />
                        <Tab label="Causas de Falla" />
                        <Tab label="Empleados" />
                    </Tabs>
                </Box>
                <Box sx={{ pt: 3 }}>
                    {activeTab === 0 && <MedidoresPanel />}
                    {activeTab === 1 && <GruposPanel />}
                    {activeTab === 2 && <FallasPanel />}
                    {activeTab === 3 && <CausasFallaPanel />}
                    {activeTab === 4 && <EmployeeConfig />}
                </Box>
            </Box>
        </Container>
    );
};

export default Configuracion;
