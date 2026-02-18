import { useEffect, useState } from 'react';
import { Box, Typography, Button, CircularProgress } from '@mui/material';
import { Add } from '@mui/icons-material';
import { useAuthFetch } from '../hooks/useAuthFetch';
import EquipmentTable, { Equipo } from '../components/equipos/EquipmentTable';
import EditEquipmentDialog from '../components/equipos/EditEquipmentDialog';
import CreateEquipmentDialog from '../components/equipos/CreateEquipmentDialog';

const EquipmentConfig = () => {
    const authFetch = useAuthFetch();
    const [equipos, setEquipos] = useState<Equipo[]>([]);
    const [loading, setLoading] = useState(true);
    const [openEdit, setOpenEdit] = useState(false);
    const [openCreate, setOpenCreate] = useState(false);
    const [saving, setSaving] = useState(false);
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
        setCurrentEquipo({ ...equipo });
        setOpenEdit(true);
    };

    const handleSave = async () => {
        if (!currentEquipo || saving) return;

        if (!currentEquipo.grupo || !currentEquipo.rutina) {
            alert("Grupo de Mantenimiento y Rutina son obligatorios");
            return;
        }

        setSaving(true);
        try {
            const res = await authFetch(`/equipos/${currentEquipo.id}`, {
                method: 'PUT',
                body: JSON.stringify({
                    Placa: currentEquipo.placa,
                    Descripcion: currentEquipo.descripcion,
                    Serie: currentEquipo.serie,
                    Codigo: currentEquipo.codigo,
                    TipoMedidorId: currentEquipo.tipoMedidorId,
                    TipoMedidorId2: currentEquipo.tipoMedidorId2,
                    Grupo: currentEquipo.grupo,
                    Rutina: currentEquipo.rutina
                })
            });

            if (res.ok) {
                fetchEquipos();
                setOpenEdit(false);
                setCurrentEquipo(null);
            } else {
                alert("Error al guardar cambios");
            }
        } catch (error) {
            console.error("Error saving equipment", error);
            alert("Error de conexión");
        } finally {
            setSaving(false);
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
            fechaInicial1: new Date().toISOString().split('T')[0],
            lecturaInicial2: '',
            fechaInicial2: new Date().toISOString().split('T')[0]
        });
        setOpenCreate(true);
    };

    const handleCreate = async () => {
        if (saving) return;

        if (!newEquipo.placa || !newEquipo.descripcion) {
            alert("Placa y Descripción son obligatorios");
            return;
        }

        if (!newEquipo.grupo || !newEquipo.rutina) {
            alert("Grupo de Mantenimiento y Rutina son obligatorios");
            return;
        }

        setSaving(true);
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
                setOpenCreate(false);
            } else {
                const err = await res.text();
                alert("Error al crear equipo: " + err);
            }
        } catch (error) {
            console.error("Error creating equipment", error);
            alert("Error de conexión");
        } finally {
            setSaving(false);
        }
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
