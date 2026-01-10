import { useState, ChangeEvent, SyntheticEvent } from 'react';
import { UploadFile, DeleteForever, CloudUpload, Engineering, LocalShipping, People } from '@mui/icons-material';
import {
  Box, Typography, Container, Paper, Button, Alert, CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Tabs, Tab
} from '@mui/material';

export default function ImportarRutinas() {
  const [activeTab, setActiveTab] = useState(0);
  const [file, setFile] = useState<File | null>(null);
  const [message, setMessage] = useState('');
  const [isUploading, setIsUploading] = useState(false);
  const [openDialog, setOpenDialog] = useState(false); // Reset DB Dialog

  const handleTabChange = (event: SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
    setFile(null);
    setMessage('');
  };

  const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setFile(e.target.files[0]);
      setMessage('');
    }
  };

  const handleUpload = async () => {
    if (!file) {
      setMessage('Por favor selecciona un archivo.');
      return;
    }

    setIsUploading(true);
    setMessage('');

    const formData = new FormData();
    formData.append('file', file);

    // Determine endpoint based on tab
    let endpoint = '';
    if (activeTab === 0) endpoint = '/rutinas/importar';
    else if (activeTab === 1) endpoint = '/equipos/importar';
    else endpoint = '/empleados/importar';

    try {
      const response = await fetch(endpoint, {
        method: 'POST',
        body: formData,
      });

      if (response.ok) {
        const data = await response.json();
        setMessage(`Success: ${data.message || 'Importación exitosa'}`);
      } else {
        const text = await response.text();
        try {
          const errorData = JSON.parse(text);
          const detail = errorData.detail || errorData.title || errorData.message || errorData.error || JSON.stringify(errorData);
          setMessage(`Error: ${detail}`);
        } catch (e) {
          setMessage(`Error: ${text || response.statusText || 'Error desconocido'}`);
        }
      }
    } catch (error: any) {
      setMessage(`Error de red: ${error.message}`);
    } finally {
      setIsUploading(false);
    }
  };

  const handleResetDb = async () => {
    setIsUploading(true);
    setOpenDialog(false);
    try {
      const response = await fetch('/admin/reset-db', { method: 'POST' });
      if (response.ok) {
        setMessage("Base de datos borrada correctamente.");
      } else {
        setMessage("Error al borrar la base de datos.");
      }
    } catch (error: any) {
      setMessage(`Error de red: ${error.message}`);
    } finally {
      setIsUploading(false);
    }
  };

  const getTabLabel = () => {
    if (activeTab === 0) return 'Cargar Rutinas';
    if (activeTab === 1) return 'Cargar Equipos';
    return 'Cargar Empleados';
  };

  const getButtonLabel = () => {
    if (activeTab === 0) return 'Importar Rutinas';
    if (activeTab === 1) return 'Importar Equipos';
    return 'Importar Empleados';
  };

  return (
    <Container maxWidth="md" sx={{ mt: 4 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 4 }}>
        <Box sx={{ p: 2, bgcolor: 'action.selected', borderRadius: 2 }}>
          <CloudUpload fontSize="large" color="primary" />
        </Box>
        <Box>
          <Typography variant="h4" component="h1" fontWeight="bold">
            Importación de Datos
          </Typography>
          <Typography variant="subtitle1" color="text.secondary">
            Carga masiva desde archivos Excel
          </Typography>
        </Box>
      </Box>

      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 3 }}>
        <Tabs value={activeTab} onChange={handleTabChange} aria-label="import tabs">
          <Tab label="Rutinas de Mantenimiento" icon={<Engineering />} iconPosition="start" />
          <Tab label="Equipos" icon={<LocalShipping />} iconPosition="start" />
          <Tab label="Empleados" icon={<People />} iconPosition="start" />
        </Tabs>
      </Box>

      <Paper elevation={3} sx={{ p: 4, borderRadius: 2, bgcolor: 'background.paper' }}>

        <Box sx={{ mb: 2 }}>
          <Typography variant="h6" gutterBottom>
            {getTabLabel()}
          </Typography>
          <Typography variant="body2" color="text.secondary" paragraph>
            {activeTab === 0 && 'Sube el archivo Excel con las rutinas de mantenimiento desglosadas (Grupo, Parte, Actividad, Frecuencia).'}
            {activeTab === 1 && 'Sube el archivo Excel "fichaEq.xlsx" con el listado de equipos, placas y descripción.'}
            {activeTab === 2 && 'Sube el archivo Excel con el listado de empleados (Nombre, Documento, Cargo, Estado).'}
          </Typography>
        </Box>

        {/* Upload Section */}
        <Box sx={{ mb: 4 }}>
          <Box
            sx={{
              position: 'relative',
              overflow: 'hidden',
              borderRadius: 2,
              border: '2px dashed',
              borderColor: 'divider',
              bgcolor: 'action.hover',
              p: 4,
              textAlign: 'center',
              transition: 'all 0.3s',
              '&:hover': {
                borderColor: 'primary.main',
                bgcolor: 'action.selected'
              }
            }}
          >
            <input
              type="file"
              accept=".xlsx, .xls"
              onChange={handleFileChange}
              style={{
                position: 'absolute', top: 0, left: 0, width: '100%', height: '100%',
                opacity: 0, cursor: 'pointer', zIndex: 2
              }}
            />
            <Box sx={{ pointerEvents: 'none', position: 'relative', zIndex: 1 }}>
              <UploadFile sx={{ fontSize: 48, color: file ? 'primary.main' : 'text.disabled', mb: 2 }} />
              <Typography color={file ? 'primary.main' : 'text.secondary'}>
                {file ? file.name : "Haz clic o arrastra tu archivo aquí"}
              </Typography>
            </Box>
          </Box>
        </Box>

        <Button
          variant="contained"
          fullWidth
          size="large"
          onClick={handleUpload}
          disabled={!file || isUploading}
          startIcon={isUploading ? <CircularProgress size={20} color="inherit" /> : <CloudUpload />}
          sx={{ mb: 4, py: 1.5 }}
        >
          {isUploading ? 'Procesando...' : getButtonLabel()}
        </Button>

        {message && (
          <Alert
            severity={message.includes('Success') || message.includes('correctamente') ? 'success' : 'error'}
            sx={{ mb: 4, whiteSpace: 'pre-wrap' }}
          >
            {message}
          </Alert>
        )}

        {/* Danger Zone */}
        <Box sx={{ mt: 4, pt: 4, borderTop: 1, borderColor: 'divider' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2, color: 'error.main' }}>
            <DeleteForever />
            <Typography variant="subtitle2" fontWeight="bold">Zona de Peligro</Typography>
          </Box>

          <Button
            variant="outlined"
            color="error"
            fullWidth
            startIcon={<DeleteForever />}
            onClick={() => setOpenDialog(true)}
          >
            BORRAR BASE DE DATOS
          </Button>
        </Box>

        {/* Confirm Dialog */}
        <Dialog open={openDialog} onClose={() => setOpenDialog(false)}>
          <DialogTitle>¿Estás seguro?</DialogTitle>
          <DialogContent>
            <DialogContentText>
              Esto borrará TODOS los datos de la base de datos (Rutinas, Equipos, Ordenes). Esta acción no se puede deshacer.
            </DialogContentText>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenDialog(false)}>Cancelar</Button>
            <Button onClick={handleResetDb} color="error" autoFocus>
              Sí, Borrar Todo
            </Button>
          </DialogActions>
        </Dialog>

      </Paper>
    </Container>
  );
}
