import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
// Tree-shaking optimized imports
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import ErrorBoundary from './components/ErrorBoundary';
import Login from './pages/Login';
import MainLayout from './layouts/MainLayout';

// Lazy load de todas las páginas (excepto Login que es crítico)
const Dashboard = lazy(() => import('./pages/Dashboard'));
const OrderDetail = lazy(() => import('./pages/OrderDetail'));
const CreateOrder = lazy(() => import('./pages/CreateOrder'));
const History = lazy(() => import('./pages/History'));
const ImportarRutinas = lazy(() => import('./pages/ImportarRutinas'));
const EditarRutinas = lazy(() => import('./pages/EditarRutinas'));
const EquipmentConfig = lazy(() => import('./pages/EquipmentConfig'));
const EmployeeConfig = lazy(() => import('./pages/EmployeeConfig'));
const Configuracion = lazy(() => import('./pages/Configuracion'));
const LogsErrores = lazy(() => import('./pages/LogsErrores'));
const UserManagement = lazy(() => import('./pages/UserManagement'));
const Auditoria = lazy(() => import('./pages/Auditoria'));

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
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
