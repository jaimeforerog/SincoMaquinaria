import React, { useState, useEffect } from 'react';
import {
    Paper, Box, Typography, TextField, Button,
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
    Switch, IconButton, Chip, Alert
} from '@mui/material';
import { Edit, Save, Cancel, Add } from '@mui/icons-material';
import { TipoMedidor } from '../../types';
import { useAuthFetch } from '../../hooks/useAuthFetch';

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
                    <Add color="primary" /> Nuevo Medidor
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

export default MedidoresPanel;
