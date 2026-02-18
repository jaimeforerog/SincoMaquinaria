import {
    Dialog, DialogTitle, DialogContent, DialogActions,
    TextField, Button, Grid, FormControl, InputLabel, Select, MenuItem, CircularProgress
} from '@mui/material';

interface CreateEquipmentDialogProps {
    open: boolean;
    equipo: any;
    grupos: any[];
    rutinas: any[];
    medidores: any[];
    saving: boolean;
    onClose: () => void;
    onChange: (field: string, value: any) => void;
    onCreate: () => void;
}

const CreateEquipmentDialog = ({
    open, equipo, grupos, rutinas, medidores, saving,
    onClose, onChange, onCreate
}: CreateEquipmentDialogProps) => (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
        <DialogTitle>Crear Nuevo Equipo</DialogTitle>
        <DialogContent dividers>
            <Grid container spacing={2} sx={{ pt: 1 }}>
                <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField label="Placa" fullWidth required value={equipo.placa} onChange={(e) => onChange('placa', e.target.value)} />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField label="Descripción" fullWidth required value={equipo.descripcion} onChange={(e) => onChange('descripcion', e.target.value)} />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField label="Marca" fullWidth value={equipo.marca} onChange={(e) => onChange('marca', e.target.value)} />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField label="Modelo" fullWidth value={equipo.modelo} onChange={(e) => onChange('modelo', e.target.value)} />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField label="Serie" fullWidth value={equipo.serie} onChange={(e) => onChange('serie', e.target.value)} />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField label="Código Interno" fullWidth value={equipo.codigo} onChange={(e) => onChange('codigo', e.target.value)} />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                    <FormControl fullWidth>
                        <InputLabel id="create-grupo-label">Grupo de Mantenimiento</InputLabel>
                        <Select labelId="create-grupo-label" value={equipo.grupo || ''} label="Grupo de Mantenimiento" onChange={(e) => onChange('grupo', e.target.value as string)}>
                            <MenuItem value=""><em>Ninguno</em></MenuItem>
                            {grupos.map((g: any) => (<MenuItem key={g.codigo} value={g.nombre}>{g.nombre}</MenuItem>))}
                        </Select>
                    </FormControl>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                    <FormControl fullWidth>
                        <InputLabel id="create-rutina-label">Rutina Asignada</InputLabel>
                        <Select labelId="create-rutina-label" value={equipo.rutina || ''} label="Rutina Asignada" onChange={(e) => onChange('rutina', e.target.value as string)}>
                            <MenuItem value=""><em>Ninguna</em></MenuItem>
                            {rutinas.map((r: any) => (<MenuItem key={r.id} value={r.id}>{r.descripcion}</MenuItem>))}
                        </Select>
                    </FormControl>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                    <FormControl fullWidth>
                        <InputLabel id="create-medidor1-label">Medidor Principal</InputLabel>
                        <Select labelId="create-medidor1-label" value={equipo.tipoMedidorId || ''} label="Medidor Principal" onChange={(e) => onChange('tipoMedidorId', e.target.value as string)}>
                            <MenuItem value=""><em>Ninguno</em></MenuItem>
                            {medidores.map((m: any) => (<MenuItem key={m.codigo} value={m.codigo}>{m.nombre} ({m.unidad})</MenuItem>))}
                        </Select>
                    </FormControl>
                </Grid>
                {equipo.tipoMedidorId && (
                    <>
                        <Grid size={{ xs: 12, sm: 3 }}>
                            <TextField label="Lectura Inicial M1" type="number" fullWidth value={equipo.lecturaInicial1 || ''} onChange={(e) => onChange('lecturaInicial1', e.target.value)} />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 3 }}>
                            <TextField label="Fecha Inicial M1" type="date" fullWidth InputLabelProps={{ shrink: true }} value={equipo.fechaInicial1 || ''} onChange={(e) => onChange('fechaInicial1', e.target.value)} />
                        </Grid>
                    </>
                )}
                <Grid size={{ xs: 12, sm: 6 }}>
                    <FormControl fullWidth>
                        <InputLabel id="create-medidor2-label">Medidor Secundario</InputLabel>
                        <Select labelId="create-medidor2-label" value={equipo.tipoMedidorId2 || ''} label="Medidor Secundario" onChange={(e) => onChange('tipoMedidorId2', e.target.value as string)}>
                            <MenuItem value=""><em>Ninguno</em></MenuItem>
                            {medidores.map((m: any) => (<MenuItem key={m.codigo} value={m.codigo}>{m.nombre} ({m.unidad})</MenuItem>))}
                        </Select>
                    </FormControl>
                </Grid>
                {equipo.tipoMedidorId2 && (
                    <>
                        <Grid size={{ xs: 12, sm: 3 }}>
                            <TextField label="Lectura Inicial M2" type="number" fullWidth value={equipo.lecturaInicial2 || ''} onChange={(e) => onChange('lecturaInicial2', e.target.value)} />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 3 }}>
                            <TextField label="Fecha Inicial M2" type="date" fullWidth InputLabelProps={{ shrink: true }} value={equipo.fechaInicial2 || ''} onChange={(e) => onChange('fechaInicial2', e.target.value)} />
                        </Grid>
                    </>
                )}
            </Grid>
        </DialogContent>
        <DialogActions>
            <Button onClick={onClose} color="inherit" disabled={saving}>Cancelar</Button>
            <Button onClick={onCreate} variant="contained" color="primary" disabled={saving} startIcon={saving ? <CircularProgress size={20} /> : null}>
                {saving ? 'Creando...' : 'Crear'}
            </Button>
        </DialogActions>
    </Dialog>
);

export default CreateEquipmentDialog;
