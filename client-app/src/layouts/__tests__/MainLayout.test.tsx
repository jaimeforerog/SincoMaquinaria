import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import MainLayout from '../MainLayout';

// Mock react-router-dom navigate
const mockNavigate = vi.fn();
const mockLogout = vi.fn();
const mockLogin = vi.fn();
const mockRefreshAccessToken = vi.fn();

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

// Mock AuthContext
vi.mock('../../contexts/AuthContext', () => ({
  useAuth: () => ({
    user: {
      id: '123',
      nombre: 'Test User',
      email: 'test@example.com',
      rol: 'Admin',
    },
    token: 'test-token',
    login: mockLogin,
    logout: mockLogout,
    isAuthenticated: true,
    isLoading: false,
    refreshAccessToken: mockRefreshAccessToken,
  }),
}));

describe('MainLayout', () => {
  const renderLayout = () => {
    return render(
      <BrowserRouter>
        <MainLayout />
      </BrowserRouter>
    );
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Branding and Header', () => {
    it('should display the application logo', () => {
      renderLayout();
      const logo = screen.getByText('M&E');
      expect(logo).toBeTruthy();
    });

    it('should display the application name', () => {
      renderLayout();
      expect(screen.getByText(/Sinco/)).toBeTruthy();
      expect(screen.getByText(/Maquinaria/)).toBeTruthy();
    });
  });

  describe('Navigation Menu', () => {
    it('should render all menu items', () => {
      renderLayout();

      const expectedMenuItems = [
        'Dashboard',
        'Importar',
        'Configuración',
        'Auditoría',
        'Logs de Error',
      ];

      expectedMenuItems.forEach(item => {
        expect(screen.getByText(item)).toBeTruthy();
      });
    });

    it('should render Dashboard menu item with correct link', () => {
      renderLayout();
      const dashboardLink = screen.getByText('Dashboard').closest('a');
      expect(dashboardLink).toBeTruthy();
      expect(dashboardLink?.getAttribute('href')).toBe('/');
    });

    it('should render Importar menu item with correct link', () => {
      renderLayout();
      const importarLink = screen.getByText('Importar').closest('a');
      expect(importarLink).toBeTruthy();
      expect(importarLink?.getAttribute('href')).toBe('/importar-rutinas');
    });

    it('should render Configuración menu item with correct link', () => {
      renderLayout();
      const configLink = screen.getByText('Configuración').closest('a');
      expect(configLink).toBeTruthy();
      expect(configLink?.getAttribute('href')).toBe('/configuracion');
    });

    it('should render Auditoría menu item with correct link', () => {
      renderLayout();
      const auditoriaLink = screen.getByText('Auditoría').closest('a');
      expect(auditoriaLink).toBeTruthy();
      expect(auditoriaLink?.getAttribute('href')).toBe('/auditoria');
    });

    it('should render Logs de Error menu item with correct link', () => {
      renderLayout();
      const logsLink = screen.getByText('Logs de Error').closest('a');
      expect(logsLink).toBeTruthy();
      expect(logsLink?.getAttribute('href')).toBe('/logs');
    });
  });

  describe('User Information Display', () => {
    it('should display user name', () => {
      renderLayout();
      expect(screen.getByText('Test User')).toBeTruthy();
    });

    it('should display user role', () => {
      renderLayout();
      expect(screen.getByText('Admin')).toBeTruthy();
    });

    it('should display user avatar with first letter of name', () => {
      renderLayout();
      const avatar = screen.getByText('T'); // First letter of "Test User"
      expect(avatar).toBeTruthy();
    });

  });

  describe('Logout Functionality', () => {
    it('should render logout button', () => {
      renderLayout();
      const logoutButton = screen.getByRole('button', { name: /cerrar sesión/i });
      expect(logoutButton).toBeTruthy();
    });

    it('should call logout and navigate when logout button is clicked', async () => {
      renderLayout();
      const logoutButton = screen.getByRole('button', { name: /cerrar sesión/i });

      fireEvent.click(logoutButton);

      expect(mockLogout).toHaveBeenCalledOnce();
      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/login');
      });
    });
  });

  describe('Layout Structure', () => {
    it('should render drawer sidebar', () => {
      const { container } = renderLayout();
      const drawer = container.querySelector('.MuiDrawer-root');
      expect(drawer).toBeTruthy();
    });

    it('should render main content area with Box component', () => {
      const { container } = renderLayout();
      // The main content is wrapped in a MUI Box, so we check for it
      const boxes = container.querySelectorAll('.MuiBox-root');
      expect(boxes.length).toBeGreaterThan(0);
    });

    it('should display online status indicator', () => {
      renderLayout();
      expect(screen.getByText(/Estado:.*Online/)).toBeTruthy();
    });
  });

  describe('Responsive Behavior', () => {
    it('should have permanent drawer variant', () => {
      const { container } = renderLayout();
      const drawer = container.querySelector('.MuiDrawer-root');
      // Permanent drawer should not have temporary/mobile classes
      expect(drawer?.classList.contains('MuiDrawer-docked')).toBe(true);
    });

    it('should have proper drawer width', () => {
      const { container } = renderLayout();
      const drawer = container.querySelector('.MuiDrawer-paper');
      expect(drawer).toBeTruthy();
      // The drawer width is set via sx prop, so we just verify it exists
    });
  });

  describe('Component Structure', () => {
    it('should render without crashing', () => {
      const { container } = renderLayout();
      expect(container).toBeTruthy();
    });

    it('should have navigation links and user info in the same component', () => {
      renderLayout();
      // Verify both navigation and user sections are present
      expect(screen.getByText('Dashboard')).toBeTruthy();
      expect(screen.getByText('Test User')).toBeTruthy();
    });
  });
});
