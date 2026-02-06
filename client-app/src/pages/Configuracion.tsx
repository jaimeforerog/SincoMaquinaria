import React, { useState } from 'react';
import {
    Container, Tabs, Tab, Box, Typography
} from '@mui/material';
import EmployeeConfig from './EmployeeConfig';
import UserManagement from './UserManagement';
import { useAuth } from '../contexts/AuthContext';

import MedidoresPanel from '../components/configuracion/MedidoresPanel';

import GruposPanel from '../components/configuracion/GruposPanel';

import FallasPanel from '../components/configuracion/FallasPanel';

import CausasFallaPanel from '../components/configuracion/CausasFallaPanel';

const Configuracion = () => {
    const { user } = useAuth();
    const [activeTab, setActiveTab] = useState(0);

    const isAdmin = user?.rol === 'Admin';

    const tabs = [
        { label: "Medidores", component: <MedidoresPanel /> },
        { label: "Grupos Mantenimiento", component: <GruposPanel /> },
        { label: "Tipos de Falla", component: <FallasPanel /> },
        { label: "Causas de Falla", component: <CausasFallaPanel /> },
        ...(isAdmin ? [{ label: "Usuarios", component: <UserManagement /> }] : []),
        { label: "Empleados", component: <EmployeeConfig /> }
    ];

    const handleChange = (_: React.SyntheticEvent, newValue: number) => {
        setActiveTab(newValue);
    };

    return (
        <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
            <Box sx={{ pb: 3 }}>
                <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 'bold' }}>
                    Configuraciones
                </Typography>
                <Typography variant="subtitle1" color="text.secondary">
                    Gestión de parámetros globales del sistema
                </Typography>
            </Box>

            <Box sx={{ width: '100%' }}>
                <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                    <Tabs value={activeTab} onChange={handleChange} aria-label="config tabs" variant="scrollable" scrollButtons="auto">
                        {tabs.map((tab, index) => (
                            <Tab key={index} label={tab.label} />
                        ))}
                    </Tabs>
                </Box>
                <Box sx={{ pt: 3 }}>
                    {tabs[activeTab] && tabs[activeTab].component}
                </Box>
            </Box>
        </Container>
    );
};

export default Configuracion;
