import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Alert,
  CircularProgress,
  MenuItem,
  Select,
  FormControl,
  InputLabel,
  InputAdornment,
  IconButton,
  FormControlLabel,
  Switch,
} from '@mui/material';
import { Add, PersonAdd, Visibility, VisibilityOff, Edit } from '@mui/icons-material';
import { useAuthFetch } from '../hooks/useAuthFetch';

interface Usuario {
  id: string;
  nombre: string;
  email: string;
  rol: string;
  activo: boolean;
  fechaCreacion: string;
}

const UserManagement: React.FC = () => {
  const authFetch = useAuthFetch();
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [loading, setLoading] = useState(true);
  const [openDialog, setOpenDialog] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  // Form state
  const [formData, setFormData] = useState({
    nombre: '',
    email: '',
    password: '',
    rol: 'User',
    activo: true,
  });

  useEffect(() => {
    fetchUsuarios();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const fetchUsuarios = async () => {
    setLoading(true);
    try {
      const response = await authFetch('/auth/users');

      if (response.ok) {
        const data = await response.json();
        setUsuarios(data);
      } else {
        console.error('Error al cargar usuarios');
      }
    } catch (error) {
      console.error('Error fetching usuarios:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = () => {
    setEditingId(null);
    setFormData({
      nombre: '',
      email: '',
      password: '',
      rol: 'User',
      activo: true,
    });
    setError('');
    setSuccess('');
    setOpenDialog(true);
  };

  const startEditing = (user: Usuario) => {
    setEditingId(user.id);
    setFormData({
      nombre: user.nombre,
      email: user.email,
      password: '', // Password is reset on edit
      rol: user.rol,
      activo: user.activo,
    });
    setError('');
    setSuccess('');
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setError('');
    setSuccess('');
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleRolChange = (e: any) => {
    setFormData({
      ...formData,
      rol: e.target.value,
    });
  };

  const handleActivoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      activo: e.target.checked
    });
  };

  const handleSaveUser = async () => {
    setError('');
    setSuccess('');

    // Validation
    if (!formData.nombre || !formData.email) {
      setError('Nombre y Email son obligatorios');
      return;
    }

    // Password validation: Required for New, Optional (min 6) for Edit
    if (!editingId && !formData.password) {
      setError('La contraseña es obligatoria para nuevos usuarios');
      return;
    }

    if (formData.password && formData.password.length < 6) {
      setError('La contraseña debe tener al menos 6 caracteres');
      return;
    }

    try {
      const url = editingId ? `/auth/users/${editingId}` : '/auth/register';
      const method = editingId ? 'PUT' : 'POST';

      const body = {
        Nombre: formData.nombre,
        Email: formData.email, // Email might not be editable in backend logic generally, but sending it just in case or ignored
        Rol: formData.rol,
        Activo: formData.activo,
        Password: formData.password || undefined // Send undefined if empty to avoid update
      };

      const response = await authFetch(url, {
        method: method,
        body: JSON.stringify(body),
        headers: { 'Content-Type': 'application/json' }
      });

      if (!response.ok) {
        // Try to read error text
        const errorText = await response.text();
        throw new Error(errorText || 'Error al guardar usuario');
      }

      setSuccess(editingId ? 'Usuario actualizado exitosamente' : 'Usuario creado exitosamente');
      setTimeout(() => {
        handleCloseDialog();
        fetchUsuarios();
      }, 1500);
    } catch (err: any) {
      setError(err.message || 'Error al procesar solicitud');
    }
  };

  return (
    <Box sx={{ flexGrow: 1 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight="bold">
          Gestión de Usuarios
        </Typography>
        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={handleOpenDialog}
          size="large"
        >
          Crear Usuario
        </Button>
      </Box>

      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Usuarios del Sistema
          </Typography>

          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <TableContainer component={Paper} sx={{ mt: 2 }}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Nombre</TableCell>
                    <TableCell>Email</TableCell>
                    <TableCell>Rol</TableCell>
                    <TableCell>Estado</TableCell>
                    <TableCell>Acciones</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {usuarios.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={5} align="center">
                        <Typography variant="body2" color="text.secondary" sx={{ py: 3 }}>
                          No hay usuarios registrados. Crea el primer usuario usando el botón de arriba.
                        </Typography>
                      </TableCell>
                    </TableRow>
                  ) : (
                    usuarios.map((usuario) => (
                      <TableRow key={usuario.id}>
                        <TableCell>{usuario.nombre}</TableCell>
                        <TableCell>{usuario.email}</TableCell>
                        <TableCell>
                          <Chip
                            label={usuario.rol}
                            color={usuario.rol === 'Admin' ? 'primary' : 'default'}
                            size="small"
                          />
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={usuario.activo ? 'Activo' : 'Inactivo'}
                            color={usuario.activo ? 'success' : 'default'}
                            size="small"
                          />
                        </TableCell>
                        <TableCell>
                          <IconButton onClick={() => startEditing(usuario)} color="primary">
                            <Edit />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      {/* Create/Edit User Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <PersonAdd sx={{ mr: 1 }} />
            {editingId ? 'Editar Usuario' : 'Crear Nuevo Usuario'}
          </Box>
        </DialogTitle>
        <DialogContent>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          {success && (
            <Alert severity="success" sx={{ mb: 2 }}>
              {success}
            </Alert>
          )}

          <TextField
            fullWidth
            label="Nombre Completo"
            name="nombre"
            value={formData.nombre}
            onChange={handleInputChange}
            margin="normal"
            required
          />

          <TextField
            fullWidth
            label="Correo Electrónico"
            name="email"
            type="email"
            value={formData.email}
            onChange={handleInputChange}
            margin="normal"
            required
            disabled={!!editingId} // Email usually immutable as it is login ID
          />

          <TextField
            fullWidth
            label={editingId ? "Contraseña (Dejar en blanco para mantener)" : "Contraseña"}
            name="password"
            type={showPassword ? 'text' : 'password'}
            value={formData.password}
            onChange={handleInputChange}
            margin="normal"
            required={!editingId}
            helperText={editingId ? "Solo llenar si desea cambiarla" : "Mínimo 6 caracteres"}
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    aria-label="toggle password visibility"
                    onClick={() => setShowPassword(!showPassword)}
                    edge="end"
                  >
                    {showPassword ? <VisibilityOff /> : <Visibility />}
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />

          <FormControl fullWidth margin="normal">
            <InputLabel>Rol</InputLabel>
            <Select value={formData.rol} onChange={handleRolChange} label="Rol">
              <MenuItem value="User">Usuario</MenuItem>
              <MenuItem value="Admin">Administrador</MenuItem>
            </Select>
          </FormControl>

          {editingId && (
            <Box sx={{ mt: 2 }}>
              <FormControlLabel
                control={
                  <Switch checked={formData.activo} onChange={handleActivoChange} color="primary" />
                }
                label={formData.activo ? "Usuario Activo" : "Usuario Inactivo"}
              />
            </Box>
          )}

        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancelar</Button>
          <Button
            onClick={handleSaveUser}
            variant="contained"
            disabled={!!success}
          >
            {editingId ? 'Guardar Cambios' : 'Crear Usuario'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default UserManagement;
