import { Box, Drawer, List, ListItem, ListItemButton, ListItemIcon, ListItemText, Typography, Divider, Button, Avatar } from '@mui/material';
import { Dashboard, History, CloudUpload, ErrorOutline, Settings, Construction, Logout, People } from '@mui/icons-material';
import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const drawerWidth = 240;

const MainLayout = () => {
    const location = useLocation();
    const navigate = useNavigate();
    const { user, logout } = useAuth();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    const menuItems = [
        { text: 'Dashboard', icon: <Dashboard />, path: '/' },
        { text: 'Orden de Trabajo', icon: <History />, path: '/historial' },
        { text: 'Importar', icon: <CloudUpload />, path: '/importar-rutinas' },
        { text: 'Equipos', icon: <Construction />, path: '/gestion-equipos' },
        { text: 'Usuarios', icon: <People />, path: '/gestion-usuarios', adminOnly: true },
        { text: 'Configuración', icon: <Settings />, path: '/configuracion' },
        { text: 'Logs de Error', icon: <ErrorOutline />, path: '/logs' },
    ];

    return (
        <Box sx={{ display: 'flex' }}>
            <Drawer
                sx={{
                    width: drawerWidth,
                    flexShrink: 0,
                    '& .MuiDrawer-paper': {
                        width: drawerWidth,
                        boxSizing: 'border-box',
                        bgcolor: 'background.paper', // Matches Theme (Deep Blue)
                        borderRight: '1px solid rgba(255, 255, 255, 0.08)'
                    },
                }}
                variant="permanent"
                anchor="left"
            >
                <div style={{ padding: '1.5rem', textAlign: 'center' }}>
                    <div style={{ fontSize: '1.5rem', fontWeight: 'bold', color: '#4fc3f7' }}>M&E</div>
                    <Typography variant="h6" sx={{ fontWeight: 'bold', mt: 1, color: 'text.primary' }}>
                        Sinco<br />Maquinaria
                    </Typography>
                </div>
                <Divider sx={{ borderColor: 'rgba(255, 255, 255, 0.08)' }} />
                <List>
                    {menuItems
                        .filter(item => !item.adminOnly || user?.rol === 'Admin')
                        .map((item) => (
                        <ListItem key={item.text} disablePadding>
                            <ListItemButton
                                component={NavLink}
                                to={item.path}
                                sx={{
                                    '&.active': {
                                        bgcolor: 'rgba(79, 195, 247, 0.16)', // Primary with opacity 
                                        color: 'primary.main',
                                        fontWeight: 'bold',
                                        '& .MuiListItemIcon-root': { color: 'primary.main' }
                                    },
                                    '&:hover': { bgcolor: 'rgba(255, 255, 255, 0.05)' },
                                    color: 'text.secondary',
                                    borderRadius: 1,
                                    mx: 1,
                                    mb: 0.5,
                                    transition: 'all 0.2s',
                                    '& .MuiListItemIcon-root': { color: 'text.secondary' }
                                }}
                            >
                                <ListItemIcon sx={{ minWidth: 40, color: 'inherit' }}>
                                    {item.icon}
                                </ListItemIcon>
                                <ListItemText primary={item.text} />
                            </ListItemButton>
                        </ListItem>
                    ))}
                </List>
                <Box sx={{ mt: 'auto', p: 2 }}>
                    <Divider sx={{ mb: 2, borderColor: 'rgba(255, 255, 255, 0.08)' }} />
                    <Box sx={{ display: 'flex', alignItems: 'center', mb: 2, px: 1 }}>
                        <Avatar
                            sx={{
                                bgcolor: 'primary.main',
                                width: 32,
                                height: 32,
                                fontSize: '0.875rem',
                                mr: 1.5
                            }}
                        >
                            {user?.nombre?.charAt(0)?.toUpperCase() || 'U'}
                        </Avatar>
                        <Box sx={{ flex: 1, minWidth: 0 }}>
                            <Typography variant="body2" noWrap sx={{ fontWeight: 500 }}>
                                {user?.nombre || 'Usuario'}
                            </Typography>
                            <Typography variant="caption" noWrap sx={{ color: 'text.secondary' }}>
                                {user?.rol || 'User'}
                            </Typography>
                        </Box>
                    </Box>
                    <Button
                        fullWidth
                        variant="outlined"
                        size="small"
                        startIcon={<Logout />}
                        onClick={handleLogout}
                        sx={{
                            borderColor: 'rgba(255, 255, 255, 0.23)',
                            color: 'text.secondary',
                            '&:hover': {
                                borderColor: 'error.main',
                                bgcolor: 'rgba(211, 47, 47, 0.08)',
                                color: 'error.main'
                            }
                        }}
                    >
                        Cerrar Sesión
                    </Button>
                    <Typography variant="caption" sx={{ color: 'text.disabled', display: 'block', mt: 1.5, textAlign: 'center' }}>
                        Estado: 🟢 Online
                    </Typography>
                </Box>
            </Drawer>

            <Box
                component="main"
                sx={{
                    flexGrow: 1,
                    bgcolor: 'background.default',
                    p: 3,
                    minHeight: '100vh',
                    overflowX: 'hidden'
                }}
            >
                <Outlet />
            </Box>
        </Box>
    );
};

export default MainLayout;
