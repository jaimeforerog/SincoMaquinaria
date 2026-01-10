import { Box, Drawer, List, ListItem, ListItemButton, ListItemIcon, ListItemText, Typography, Divider } from '@mui/material';
import { Dashboard, History, CloudUpload, ErrorOutline, Settings, Construction } from '@mui/icons-material';
import { NavLink, Outlet, useLocation } from 'react-router-dom';

const drawerWidth = 240;

const MainLayout = () => {
    const location = useLocation();

    const menuItems = [
        { text: 'Dashboard', icon: <Dashboard />, path: '/' },
        { text: 'Orden de Trabajo', icon: <History />, path: '/historial' },
        { text: 'Importar', icon: <CloudUpload />, path: '/importar-rutinas' },
        { text: 'Equipos', icon: <Construction />, path: '/gestion-equipos' },
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
                    {menuItems.map((item) => (
                        <ListItem key={item.text} disablePadding>
                            <ListItemButton
                                component={NavLink}
                                to={item.path}
                                selected={location.pathname === item.path}
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
                <Box sx={{ mt: 'auto', p: 2, textAlign: 'center' }}>
                    <Typography variant="caption" sx={{ color: 'text.disabled' }}>
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
