import React, { useState, useEffect } from 'react';
import { LocalShipping, Save } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { Rutina } from '../types';
import {
    Box,
    Button,
    Container,
    Typography,
    Paper,
    TextField,
    Autocomplete,
    MenuItem,
    Grid,
    CircularProgress,
    InputAdornment
} from '@mui/material';

const CreateOrder = () => {
    const navigate = useNavigate();
    const [loading, setLoading] = useState(false);
    const [formData, setFormData] = useState({
        equipoId: '',
        numero: `OT-${new Date().getFullYear()}-${Math.floor(Math.random() * 1000)}`,
        origen: 'Manual',
        tipo: 'Correctivo',
        rutinaId: '',
        fechaOrden: new Date().toISOString().split('T')[0]
    });
    const [rutinas, setRutinas] = useState<Rutina[]>([]);
    const [equiposList, setEquiposList] = useState<any[]>([]);
    const [equipoValue, setEquipoValue] = useState<any | null>(null);
    const [availableFrequencies, setAvailableFrequencies] = useState<number[]>([]);
    const [frecuenciaPreventiva, setFrecuenciaPreventiva] = useState<string>('');

    useEffect(() => {
        // Cargar rutinas al montar
        fetch('/rutinas')
            .then(res => res.json())
            .then(data => setRutinas(data))
            .catch(err => console.error("Error cargando rutinas", err));

        // Cargar equipos al montar (MIGRACIÓN)
        fetch('/equipos')
            .then(res => res.json())
            .then(data => setEquiposList(data))
            .catch(err => console.error("Error cargando equipos", err));
    }, []);

    // Effect to fetch routine details and extract frequencies
    useEffect(() => {
        if (formData.rutinaId && formData.tipo === 'Preventivo') {
            fetch(`/rutinas/${formData.rutinaId}`)
                .then(res => res.json())
                .then((data: Rutina) => {
                    const freqs = new Set<number>();
                    data.partes?.forEach(p => {
                        p.actividades.forEach(a => {
                            if (a.frecuencia > 0) freqs.add(a.frecuencia);
                        });
                    });
                    const sortedFreqs = Array.from(freqs).sort((a, b) => a - b);
                    setAvailableFrequencies(sortedFreqs);
                    setFrecuenciaPreventiva(''); // Reset selection
                })
                .catch(err => console.error("Error loading routine details", err));
        } else {
            setAvailableFrequencies([]);
            setFrecuenciaPreventiva('');
        }
    }, [formData.rutinaId, formData.tipo]);

    const handleSubmit = async () => {
        setLoading(true);
        try {
            const payload = {
                numero: formData.numero,
                equipoId: formData.equipoId,
                origen: formData.origen,
                tipo: formData.tipo,
                fechaOrden: formData.fechaOrden ? new Date(formData.fechaOrden).toISOString() : null,
                rutinaId: formData.rutinaId || null,
                frecuenciaPreventiva: frecuenciaPreventiva ? parseInt(frecuenciaPreventiva) : null
            };

            const response = await fetch('/ordenes', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });

            if (response.ok) {
                const data = await response.json();
                const newId = data.id || data.Id;
                if (newId) {
                    navigate(`/ordenes/${newId}`);
                } else {
                    console.error("No ID returned", data);
                    alert("Orden creada, pero no se pudo redirigir. Revisa el historial.");
                    navigate('/historial');
                }
            } else {
                alert("Error al crear la orden");
            }
        } catch (error) {
            console.error(error);
            alert("Error de conexión");
        } finally {
            setLoading(false);
        }
    };

    return (
        <Container maxWidth="md">
            <Box sx={{ mb: 4, textAlign: 'center' }}>
                <Typography variant="h4" component="h1" fontWeight="bold">
                    Nueva Orden de Trabajo
                </Typography>
                <Typography variant="subtitle1" color="text.secondary">
                    Diligencia los datos para crear una nueva orden
                </Typography>
            </Box>

            <Paper elevation={3} sx={{ p: 4, borderRadius: 2 }}>
                <Grid container spacing={4}>
                    {/* SECCIÓN 1: Equipo */}
                    <Grid item xs={12}>
                        <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <LocalShipping sx={{ fontSize: 20 }} /> 1. Selecciona el Equipo
                        </Typography>
                        <Autocomplete
                            options={equiposList}
                            getOptionLabel={(option) => `${option.placa} - ${option.descripcion}`}
                            value={equipoValue}
                            onChange={(event, newValue: any) => {
                                setEquipoValue(newValue);
                                setFormData({ ...formData, equipoId: newValue ? newValue.id : '' });
                            }}
                            renderInput={(params) => (
                                <TextField
                                    {...params}
                                    label="Buscar equipo por placa o descripción..."
                                    variant="outlined"
                                    fullWidth
                                />
                            )}
                            renderOption={(props, option) => (
                                <li {...props} key={option.id}>
                                    <Box>
                                        <Typography variant="body1" fontWeight="bold">{option.placa}</Typography>
                                        <Typography variant="body2" color="text.secondary">{option.descripcion}</Typography>
                                    </Box>
                                </li>
                            )}
                        />
                    </Grid>

                    {/* SECCIÓN 2: Detalles */}
                    <Grid item xs={12}>
                        <Typography variant="h6" gutterBottom>
                            2. Detalles de la Orden
                        </Typography>
                        <Grid container spacing={2}>
                            <Grid item xs={12} md={6}>
                                <TextField
                                    select
                                    label="Tipo de Orden"
                                    fullWidth
                                    value={formData.tipo}
                                    onChange={(e) => {
                                        const newTipo = e.target.value;
                                        const newOrigen = newTipo === 'Preventivo' ? 'Planificacion' : 'Manual';
                                        setFormData({ ...formData, tipo: newTipo, origen: newOrigen });
                                        if (newTipo !== 'Preventivo') {
                                            setFormData(prev => ({ ...prev, tipo: newTipo, origen: newOrigen, rutinaId: '' }));
                                        }
                                    }}
                                >
                                    <MenuItem value="Correctivo">Correctivo</MenuItem>
                                    <MenuItem value="Preventivo">Preventivo</MenuItem>
                                    <MenuItem value="Predictivo">Predictivo</MenuItem>
                                </TextField>
                            </Grid>

                            <Grid item xs={12} md={6}>
                                <TextField
                                    type="date"
                                    label="Fecha de la OT"
                                    fullWidth
                                    InputLabelProps={{ shrink: true }}
                                    value={formData.fechaOrden}
                                    onChange={(e) => setFormData({ ...formData, fechaOrden: e.target.value })}
                                />
                            </Grid>

                            {formData.tipo === 'Preventivo' && (
                                <>
                                    <Grid item xs={12} md={6}>
                                        <TextField
                                            select
                                            label="Rutina Sugerida"
                                            fullWidth
                                            value={formData.rutinaId}
                                            onChange={(e) => setFormData({ ...formData, rutinaId: e.target.value })}
                                            helperText="Selecciona la rutina de mantenimiento asociada"
                                        >
                                            <MenuItem value="">
                                                <em>-- Seleccionar Rutina --</em>
                                            </MenuItem>
                                            {rutinas.map(r => (
                                                <MenuItem key={r.id} value={r.id}>
                                                    {r.grupo} - {r.descripcion} ({r.id.substring(0, 8)})
                                                </MenuItem>
                                            ))}
                                        </TextField>
                                    </Grid>

                                    <Grid item xs={12} md={6}>
                                        <TextField
                                            select
                                            label="Frecuencia Mantenimiento"
                                            fullWidth
                                            value={frecuenciaPreventiva}
                                            onChange={(e) => setFrecuenciaPreventiva(e.target.value)}
                                            helperText="Selecciona la frecuencia para filtrar actividades"
                                            disabled={availableFrequencies.length === 0}
                                        >
                                            <MenuItem value="">
                                                <em>-- Todas --</em>
                                            </MenuItem>
                                            {availableFrequencies.map(f => (
                                                <MenuItem key={f} value={f}>
                                                    {f} horas (o unidades)
                                                </MenuItem>
                                            ))}
                                        </TextField>
                                    </Grid>
                                </>
                            )}
                        </Grid>
                    </Grid>

                    {/* Botón Guardar */}
                    <Grid item xs={12}>
                        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 2 }}>
                            <Button
                                variant="contained"
                                size="large"
                                onClick={handleSubmit}
                                disabled={loading || !formData.equipoId}
                                startIcon={loading ? <CircularProgress size={20} color="inherit" /> : <Save />}
                                sx={{ fontWeight: 'bold', px: 4 }}
                            >
                                {loading ? 'Guardando...' : 'Crear Orden'}
                            </Button>
                        </Box>
                    </Grid>
                </Grid>
            </Paper>
        </Container>
    );
};

export default CreateOrder;
