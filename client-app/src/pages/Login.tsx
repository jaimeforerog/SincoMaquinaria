import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
// Tree-shaking optimized imports
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import Alert from '@mui/material/Alert';
import CircularProgress from '@mui/material/CircularProgress';
import Container from '@mui/material/Container';
import InputAdornment from '@mui/material/InputAdornment';
import IconButton from '@mui/material/IconButton';
import Build from '@mui/icons-material/Build';
import Visibility from '@mui/icons-material/Visibility';
import VisibilityOff from '@mui/icons-material/VisibilityOff';
import { useAuth } from '../contexts/AuthContext';

const Login: React.FC = () => {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await login(email, password);
      navigate('/');
    } catch (err) {
      setError(err instanceof Error && err.message ? err.message : 'Error al iniciar sesión. Verifica tus credenciales.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'linear-gradient(135deg, #0a1929 0%, #1a2332 100%)',
      }}
    >
      <Container maxWidth="sm">
        <Card
          sx={{
            p: 4,
            boxShadow: '0 8px 32px rgba(0, 0, 0, 0.3)',
            backgroundColor: 'rgba(16, 42, 67, 0.95)',
            backdropFilter: 'blur(10px)',
          }}
        >
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 3 }}>
              <Build sx={{ fontSize: 48, color: 'primary.main', mr: 2 }} />
              <Typography variant="h4" component="h1" fontWeight="bold" color="primary">
                SincoMaquinaria
              </Typography>
            </Box>

            <Typography variant="h6" align="center" gutterBottom color="text.secondary">
              Sistema de Gestión de Mantenimiento
            </Typography>

            <Box component="form" onSubmit={handleSubmit} sx={{ mt: 4 }}>
              {error && (
                <Alert severity="error" sx={{ mb: 2 }}>
                  {error}
                </Alert>
              )}

              <TextField
                fullWidth
                label="Correo Electrónico o Usuario"
                type="text"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                autoComplete="username"
                autoFocus
                sx={{ mb: 2 }}
              />

              <TextField
                fullWidth
                label="Contraseña"
                type={showPassword ? 'text' : 'password'}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                autoComplete="current-password"
                sx={{ mb: 3 }}
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

              <Button
                type="submit"
                fullWidth
                variant="contained"
                size="large"
                disabled={loading}
                sx={{
                  py: 1.5,
                  fontSize: '1.1rem',
                  fontWeight: 'bold',
                }}
              >
                {loading ? <CircularProgress size={24} /> : 'Iniciar Sesión'}
              </Button>
            </Box>

            <Typography variant="body2" align="center" sx={{ mt: 3 }} color="text.secondary">
              © 2026 SincoMaquinaria - Todos los derechos reservados
            </Typography>
          </CardContent>
        </Card>
      </Container>
    </Box>
  );
};

export default Login;
