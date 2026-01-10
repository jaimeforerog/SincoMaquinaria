import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ThemeProvider, createTheme, CssBaseline } from '@mui/material';
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
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<MainLayout />}>
            <Route index element={<Dashboard />} />
            <Route path="nueva-orden" element={<CreateOrder />} />
            <Route path="ordenes/:id" element={<OrderDetail />} />
            <Route path="historial" element={<History />} />
            <Route path="importar-rutinas" element={<ImportarRutinas />} />
            <Route path="gestion-equipos" element={<EquipmentConfig />} />
            <Route path="gestion-empleados" element={<EmployeeConfig />} />
            <Route path="configuracion" element={<Configuracion />} />
            <Route path="logs" element={<LogsErrores />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </ThemeProvider>
  );
}

export default App;
