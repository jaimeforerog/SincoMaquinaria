import React, { useState, useEffect } from 'react';
import {
    Paper, Box, Typography, TextField, Button,
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
    Switch, Chip, Alert
} from '@mui/material';
import { Add } from '@mui/icons-material';
import { TipoFalla } from '../../types';
import { useAuthFetch } from '../../hooks/useAuthFetch';

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

export default FallasPanel;
