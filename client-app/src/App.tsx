import { Suspense } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
// Tree-shaking optimized imports
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import { AuthProvider } from './contexts/AuthContext';
import { QueryProvider } from './contexts/QueryProvider';
import { NotificationProvider } from './contexts/NotificationContext';
import ProtectedRoute from './components/ProtectedRoute';
import ErrorBoundary from './components/ErrorBoundary';
import Login from './pages/Login';
import MainLayout from './layouts/MainLayout';
import { lazyWithRetry } from './utils/lazyWithRetry';

// Lazy load con retry mechanism (previene errores de cache en production)
const Dashboard = lazyWithRetry(() => import('./pages/Dashboard'));
const OrderDetail = lazyWithRetry(() => import('./pages/OrderDetail'));
const CreateOrder = lazyWithRetry(() => import('./pages/CreateOrder'));
const History = lazyWithRetry(() => import('./pages/History'));
const ImportarRutinas = lazyWithRetry(() => import('./pages/ImportarRutinas'));
const EditarRutinas = lazyWithRetry(() => import('./pages/EditarRutinas'));
const EquipmentConfig = lazyWithRetry(() => import('./pages/EquipmentConfig'));
const EmployeeConfig = lazyWithRetry(() => import('./pages/EmployeeConfig'));
const Configuracion = lazyWithRetry(() => import('./pages/Configuracion'));
const LogsErrores = lazyWithRetry(() => import('./pages/LogsErrores'));
const UserManagement = lazyWithRetry(() => import('./pages/UserManagement'));
const Auditoria = lazyWithRetry(() => import('./pages/Auditoria'));

const darkTheme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#4fc3f7', // Light Blue
    },
    background: {
      default: '#0a1929', // Deep Blue
      paper: '#102a43',   // Lighter Blue
    },
  },
});

// Componente de loading para lazy routes
const PageLoader = () => (
  <Box
    sx={{
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      minHeight: '400px',
      width: '100%'
    }}
  >
    <CircularProgress size={40} />
  </Box>
);

function App() {
  return (
    <ThemeProvider theme={darkTheme}>
      <CssBaseline />
      <AuthProvider>
        <QueryProvider>
          <NotificationProvider>
          <BrowserRouter>
            <ErrorBoundary>
              <Suspense fallback={<PageLoader />}>
                <Routes>
                  <Route path="/login" element={<Login />} />
                  <Route
                    path="/"
                    element={
                      <ProtectedRoute>
                        <MainLayout />
                      </ProtectedRoute>
                    }
                  >
                    <Route index element={<Dashboard />} />
                    <Route path="nueva-orden" element={<CreateOrder />} />
                    <Route path="ordenes/:id" element={<OrderDetail />} />
                    <Route path="historial" element={<History />} />
                    <Route path="importar-rutinas" element={<ImportarRutinas />} />
                    <Route path="editar-rutinas" element={<EditarRutinas />} />
                    <Route path="gestion-equipos" element={<EquipmentConfig />} />
                    <Route path="gestion-empleados" element={<EmployeeConfig />} />
                    <Route path="gestion-usuarios" element={<UserManagement />} />
                    <Route path="configuracion" element={<Configuracion />} />
                    <Route path="auditoria" element={<Auditoria />} />
                    <Route path="logs" element={<LogsErrores />} />
                  </Route>
                  <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>
              </Suspense>
            </ErrorBoundary>
          </BrowserRouter>
          </NotificationProvider>
        </QueryProvider>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
