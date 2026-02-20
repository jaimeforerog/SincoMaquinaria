import { useState } from 'react';
import { Box, Typography, Button, CircularProgress } from '@mui/material';
import { Add } from '@mui/icons-material';
import { useApiQuery, useApiMutation, useApiDynamicMutation } from '../hooks/useApi';
import { TipoMedidor, GrupoMantenimiento } from '../types';
import { useNotification } from '../contexts/NotificationContext';
import EquipmentTable, { Equipo } from '../components/equipos/EquipmentTable';
import EditEquipmentDialog from '../components/equipos/EditEquipmentDialog';
import CreateEquipmentDialog from '../components/equipos/CreateEquipmentDialog';

interface EquiposResponse {
    data: Equipo[];
    page: number;
    pageSize: number;
    totalCount: number;
}

const EquipmentConfig = () => {
    const { showNotification } = useNotification();

    // --- React Query: data fetching ---
    const { data: equiposResponse, isLoading: loading } = useApiQuery<EquiposResponse>(
        ['equipos'],
        '/equipos'
    );
    const equipos = equiposResponse?.data ?? [];

    const { data: medidoresRaw } = useApiQuery<TipoMedidor[]>(
        ['configuracion', 'medidores'],
        '/configuracion/medidores'
    );
    const medidores = (medidoresRaw ?? []).filter((m) => m.activo);

    const { data: rutinasResponse } = useApiQuery<{ data: { id: string; descripcion: string }[] }>(
        ['rutinas'],
        '/rutinas?pageSize=1000'
    );
    const rutinas = rutinasResponse?.data ?? [];

    const { data: gruposRaw } = useApiQuery<GrupoMantenimiento[]>(
        ['configuracion', 'grupos'],
        '/configuracion/grupos'
    );
    const grupos = (gruposRaw ?? []).filter((g) => g.activo);

    // --- React Query: mutations ---
    const createMutation = useApiMutation<unknown, Record<string, unknown>>('/equipos', {
        invalidateKeys: [['equipos'], ['dashboard']],
    });

    const updateMutation = useApiDynamicMutation({
        method: 'PUT',
        invalidateKeys: [['equipos'], ['dashboard']],
    });

    // --- Dialog state ---
    const [openEdit, setOpenEdit] = useState(false);
    const [openCreate, setOpenCreate] = useState(false);
    const [currentEquipo, setCurrentEquipo] = useState<Equipo | null>(null);
    const [newEquipo, setNewEquipo] = useState<Equipo & { lecturaInicial1: string; fechaInicial1: string; lecturaInicial2: string; fechaInicial2: string }>({
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

    const saving = createMutation.isPending || updateMutation.isPending;

    const handleEditClick = (equipo: Equipo) => {
        setCurrentEquipo({ ...equipo });
        setOpenEdit(true);
    };

    const handleSave = async () => {
        if (!currentEquipo || saving) return;

        if (!currentEquipo.grupo || !currentEquipo.rutina) {
            showNotification("Grupo de Mantenimiento y Rutina son obligatorios", "warning");
            return;
        }

        updateMutation.mutate(
            {
                url: `/equipos/${currentEquipo.id}`,
                body: {
                    Placa: currentEquipo.placa,
                    Descripcion: currentEquipo.descripcion,
                    Serie: currentEquipo.serie,
                    Codigo: currentEquipo.codigo,
                    TipoMedidorId: currentEquipo.tipoMedidorId,
                    TipoMedidorId2: currentEquipo.tipoMedidorId2,
                    Grupo: currentEquipo.grupo,
                    Rutina: currentEquipo.rutina
                }
            },
            {
                onSuccess: () => {
                    setOpenEdit(false);
                    setCurrentEquipo(null);
                },
                onError: () => showNotification("Error al guardar cambios", "error"),
            }
        );
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
            fechaInicial1: new Date().toISOString().split('T')[0],
            lecturaInicial2: '',
            fechaInicial2: new Date().toISOString().split('T')[0]
        });
        setOpenCreate(true);
    };

    const handleCreate = async () => {
        if (saving) return;

        if (!newEquipo.placa || !newEquipo.descripcion) {
            showNotification("Placa y Descripción son obligatorios", "warning");
            return;
        }

        if (!newEquipo.grupo || !newEquipo.rutina) {
            showNotification("Grupo de Mantenimiento y Rutina son obligatorios", "warning");
            return;
        }

        createMutation.mutate(
            {
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
            },
            {
                onSuccess: () => setOpenCreate(false),
                onError: (err) => showNotification("Error al crear equipo: " + err.message, "error"),
            }
        );
    };

    return (
        <Box>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h4" fontWeight="bold" color="text.primary">
                    Configuración de Equipos
                </Typography>
                <Button variant="contained" startIcon={<Add />} onClick={handleCreateClick}>
                    Nuevo Equipo
                </Button>
            </Box>

            {loading ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
                    <CircularProgress />
                </Box>
            ) : (
                <EquipmentTable equipos={equipos} onEditClick={handleEditClick} />
            )}

            <EditEquipmentDialog
                open={openEdit}
                equipo={currentEquipo}
                grupos={grupos}
                rutinas={rutinas}
                medidores={medidores}
                saving={saving}
                onClose={() => { setOpenEdit(false); setCurrentEquipo(null); }}
                onChange={(field, value) => {
                    if (currentEquipo) setCurrentEquipo({ ...currentEquipo, [field]: value });
                }}
                onSave={handleSave}
            />

            <CreateEquipmentDialog
                open={openCreate}
                equipo={newEquipo}
                grupos={grupos}
                rutinas={rutinas}
                medidores={medidores}
                saving={saving}
                onClose={() => setOpenCreate(false)}
                onChange={(field, value) => setNewEquipo({ ...newEquipo, [field]: value })}
                onCreate={handleCreate}
            />
        </Box>
    );
};

export default EquipmentConfig;
