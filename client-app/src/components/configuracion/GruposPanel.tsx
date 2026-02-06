import React, { useState, useEffect } from 'react';
import {
    Paper, Box, Typography, TextField, Button,
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
    Switch, IconButton, Alert
} from '@mui/material';
import { Edit, Save, Cancel, Add } from '@mui/icons-material';
import { GrupoMantenimiento } from '../../types';
import { useAuthFetch } from '../../hooks/useAuthFetch';

const GruposPanel = () => {
    const authFetch = useAuthFetch();
    const [grupos, setGrupos] = useState<GrupoMantenimiento[]>([]);
    const [nuevo, setNuevo] = useState({ nombre: '', descripcion: '' });
    const [error, setError] = useState('');
    const [editingId, setEditingId] = useState<string | null>(null);
    const [editForm, setEditForm] = useState({ nombre: '', descripcion: '' });

    useEffect(() => { fetchGrupos(); }, []);

    const fetchGrupos = async () => {
        try {
            const res = await authFetch('/configuracion/grupos');
            if (res.ok) setGrupos(await res.json());
        } catch (e) { setError('Error al cargar grupos'); }
    };

    const handleCrear = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!nuevo.nombre) return setError('Nombre obligatorio');
        try {
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
        } catch (e) { setError('Error al crear grupo'); }
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

export default GruposPanel;
