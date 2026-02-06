import React, { useState, useEffect } from 'react';
import {
    Paper, Box, Typography, TextField, Button,
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
    Switch, IconButton, Alert
} from '@mui/material';
import { Edit, Save, Cancel, Add } from '@mui/icons-material';
import { CausaFalla } from '../../types';
import { useAuthFetch } from '../../hooks/useAuthFetch';

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

export default CausasFallaPanel;
