import { useEffect, useState } from 'react';
import { Box, Typography, Button, CircularProgress, Alert } from '@mui/material';
import { Add } from '@mui/icons-material';
import { useAuthFetch } from '../hooks/useAuthFetch';
import { Rutina, Parte, Actividad } from '../types';
import RutinaAccordion from '../components/rutinas/RutinaAccordion';
import RutinaFormDialog from '../components/rutinas/RutinaFormDialog';
import ParteFormDialog from '../components/rutinas/ParteFormDialog';
import ActividadFormDialog from '../components/rutinas/ActividadFormDialog';

const EditarRutinas = () => {
    const authFetch = useAuthFetch();
    const [rutinas, setRutinas] = useState<Rutina[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [grupos, setGrupos] = useState<any[]>([]);
    const [medidores, setMedidores] = useState<any[]>([]);

    // Dialog states
    const [rutinaDialogOpen, setRutinaDialogOpen] = useState(false);
    const [rutinaDialogMode, setRutinaDialogMode] = useState<'create' | 'edit'>('create');
    const [editParteDialog, setEditParteDialog] = useState(false);
    const [editActividadDialog, setEditActividadDialog] = useState(false);

    const [currentRutina, setCurrentRutina] = useState<Rutina | null>(null);
    const [newRutina, setNewRutina] = useState({ descripcion: '', grupo: '' });
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
                setRutinaDialogOpen(false);
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
        setRutinaDialogMode('edit');
        setRutinaDialogOpen(true);
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
                setRutinaDialogOpen(false);
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
                res = await authFetch(`/rutinas/${currentRutinaId}/partes/${currentParte.id}`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ descripcion: currentParte.descripcion })
                });
            } else {
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

        if (!currentActividad.descripcion || currentActividad.descripcion.trim() === '') {
            setError('La descripción de la actividad es obligatoria');
            return;
        }

        try {
            let res;
            if (currentActividad.id) {
                res = await authFetch(
                    `/rutinas/${currentRutinaId}/partes/${currentParteId}/actividades/${currentActividad.id}`,
                    {
                        method: 'PUT',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(currentActividad)
                    }
                );
            } else {
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
                        setRutinaDialogMode('create');
                        setRutinaDialogOpen(true);
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
                    <RutinaAccordion
                        key={rutina.id}
                        rutina={rutina}
                        onEditRutina={handleEditRutina}
                        onAddParte={handleAddParte}
                        onEditParte={handleEditParte}
                        onDeleteParte={handleDeleteParte}
                        onAddActividad={handleAddActividad}
                        onEditActividad={handleEditActividad}
                        onDeleteActividad={handleDeleteActividad}
                    />
                ))
            )}

            {/* Dialogs */}
            <RutinaFormDialog
                open={rutinaDialogOpen}
                mode={rutinaDialogMode}
                rutina={rutinaDialogMode === 'create' ? newRutina : currentRutina}
                grupos={grupos}
                onClose={() => setRutinaDialogOpen(false)}
                onSave={rutinaDialogMode === 'create' ? handleCreateRutina : handleSaveRutina}
                onChange={(field, value) => {
                    if (rutinaDialogMode === 'create') {
                        setNewRutina({ ...newRutina, [field]: value });
                    } else if (currentRutina) {
                        setCurrentRutina({ ...currentRutina, [field]: value });
                    }
                }}
            />

            <ParteFormDialog
                open={editParteDialog}
                parte={currentParte}
                onClose={() => setEditParteDialog(false)}
                onSave={handleSaveParte}
                onChange={(descripcion) =>
                    setCurrentParte({ ...currentParte!, descripcion })
                }
            />

            <ActividadFormDialog
                open={editActividadDialog}
                actividad={currentActividad}
                medidores={medidores}
                error={error}
                onClose={() => setEditActividadDialog(false)}
                onSave={handleSaveActividad}
                onChange={setCurrentActividad}
                onClearError={() => setError(null)}
            />
        </Box>
    );
};

export default EditarRutinas;
