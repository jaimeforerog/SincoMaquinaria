import { useState, ChangeEvent, SyntheticEvent } from 'react';
import { UploadFile, DeleteForever, CloudUpload, Engineering, LocalShipping, People, Download } from '@mui/icons-material';
import {
  Box, Typography, Container, Paper, Button, Alert, CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Tabs, Tab
} from '@mui/material';

import { useAuthFetch } from '../hooks/useAuthFetch';
import { useAuth } from '../contexts/AuthContext';

export default function ImportarRutinas() {
  const authFetch = useAuthFetch();
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState(0);
  const [file, setFile] = useState<File | null>(null);
  const [message, setMessage] = useState('');
  const [isUploading, setIsUploading] = useState(false);
  const [openDialog, setOpenDialog] = useState(false); // Reset DB Dialog

  const handleTabChange = (_event: SyntheticEvent, newValue: number) => {
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
      const response = await authFetch(endpoint, {
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
    } catch (error) {
      setMessage(`Error de red: ${error instanceof Error ? error.message : 'Error desconocido'}`);
    } finally {
      setIsUploading(false);
    }
  };

  const handleResetDb = async () => {
    setIsUploading(true);
    setOpenDialog(false);
    try {
      const response = await authFetch('/admin/reset-db', { method: 'POST' });
      if (response.ok) {
        setMessage("Base de datos borrada correctamente.");
      } else {
        setMessage("Error al borrar la base de datos.");
      }
    } catch (error) {
      setMessage(`Error de red: ${error instanceof Error ? error.message : 'Error desconocido'}`);
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

  const getTabDescription = () => {
    if (activeTab === 0) return 'Sube el archivo Excel con las rutinas de mantenimiento desglosadas (Grupo, Parte, Actividad, Frecuencia).';
    if (activeTab === 1) return 'Sube el archivo Excel con el listado de equipos. Todos los campos son obligatorios.';
    return 'Sube el archivo Excel con el listado de empleados (Nombre, Documento, Cargo, Estado).';
  };

  const handleDownloadTemplate = async () => {
    try {
      let endpoint = '';
      let defaultFileName = '';

      if (activeTab === 0) {
        endpoint = '/rutinas/plantilla';
        defaultFileName = `plantillaRutinas_${new Date().toISOString().slice(0, 10)}.xlsx`;
      } else if (activeTab === 1) {
        endpoint = '/equipos/plantilla';
        defaultFileName = `plantillaEquipos_${new Date().toISOString().slice(0, 10)}.xlsx`;
      } else {
        endpoint = '/empleados/plantilla';
        defaultFileName = `plantillaEmpleados_${new Date().toISOString().slice(0, 10)}.xlsx`;
      }

      const response = await authFetch(endpoint, {
        method: 'GET',
      });

      if (response.ok) {
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;

        // Extraer el nombre del archivo del header Content-Disposition o usar uno por defecto
        const contentDisposition = response.headers.get('Content-Disposition');
        // Regex ajustado para capturar solo el valor de filename, deteniéndose ante un punto y coma
        const fileNameMatch = contentDisposition?.match(/filename="?([^";]+)"?/);
        const fileName = fileNameMatch ? fileNameMatch[1] : defaultFileName;

        link.setAttribute('download', fileName);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
      } else {
        setMessage('Error al descargar la plantilla');
      }
    } catch (error) {
      setMessage(`Error al descargar: ${error instanceof Error ? error.message : 'Error desconocido'}`);
    }
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

        <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <Box>
            <Typography variant="h6" gutterBottom>
              {getTabLabel()}
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              {getTabDescription()}
            </Typography>
          </Box>

          <Button
            variant="outlined"
            startIcon={<Download />}
            onClick={handleDownloadTemplate}
            sx={{ flexShrink: 0 }}
          >
            Descargar Plantilla
          </Button>

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

        {/* Danger Zone - Solamente visible para administradores */}
        {user?.rol === 'Admin' && (
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
        )}

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
