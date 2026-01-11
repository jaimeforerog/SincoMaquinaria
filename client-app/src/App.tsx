import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, createTheme, CssBaseline } from '@mui/material';
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import ErrorBoundary from './components/ErrorBoundary';
import Login from './pages/Login';
import MainLayout from './layouts/MainLayout';
import Dashboard from './pages/Dashboard';
import OrderDetail from './pages/OrderDetail';
import CreateOrder from './pages/CreateOrder';
import History from './pages/History';
import ImportarRutinas from './pages/ImportarRutinas';
import EquipmentConfig from './pages/EquipmentConfig';
import EmployeeConfig from './pages/EmployeeConfig';
import Configuracion from './pages/Configuracion';
import LogsErrores from './pages/LogsErrores';
import UserManagement from './pages/UserManagement';

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

function App() {
  return (
    <ThemeProvider theme={darkTheme}>
      <CssBaseline />
      <AuthProvider>
        <BrowserRouter>
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
              <Route path="gestion-equipos" element={<EquipmentConfig />} />
              <Route path="gestion-empleados" element={<EmployeeConfig />} />
              <Route path="gestion-usuarios" element={<UserManagement />} />
              <Route path="configuracion" element={<Configuracion />} />
              <Route path="logs" element={<LogsErrores />} />
            </Route>
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
