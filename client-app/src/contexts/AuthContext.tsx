import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';

interface User {
  id: string;
  email: string;
  nombre: string;
  rol: string;
}

interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  isAuthenticated: boolean;
  isLoading: boolean;
  refreshAccessToken: () => Promise<boolean>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [refreshToken, setRefreshToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Function to refresh access token
  const refreshAccessToken = async (): Promise<boolean> => {
    const storedRefreshToken = localStorage.getItem('refreshToken');
    if (!storedRefreshToken) return false;

    try {
      const response = await fetch('/auth/refresh', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: storedRefreshToken }),
      });

      if (!response.ok) return false;

      const data = await response.json();

      // Update tokens
      localStorage.setItem('authToken', data.token);
      localStorage.setItem('refreshToken', data.refreshToken);
      setToken(data.token);
      setRefreshToken(data.refreshToken);

      return true;
    } catch (error) {
      console.error('Error refreshing token:', error);
      return false;
    }
  };

  // Verify token with backend on mount
  useEffect(() => {
    const initAuth = async () => {
      const storedToken = localStorage.getItem('authToken');
      const storedRefreshToken = localStorage.getItem('refreshToken');
      const storedUser = localStorage.getItem('authUser');

      if (storedToken && storedRefreshToken && storedUser) {
        try {
          // Validate token with backend
          const response = await fetch('/auth/me', {
            headers: {
              'Authorization': `Bearer ${storedToken}`
            }
          });

          if (response.ok) {
            const verifiedUser = await response.json();
            setToken(storedToken);
            setRefreshToken(storedRefreshToken);
            setUser(verifiedUser);
          } else if (response.status === 401) {
            // Token expired, try to refresh
            console.log('Access token expired, attempting refresh...');
            const refreshed = await refreshAccessToken();

            if (refreshed && storedUser) {
              const userData = JSON.parse(storedUser);
              setUser(userData);
            } else {
              // Refresh failed, logout
              console.warn('Token refresh failed, logging out');
              localStorage.removeItem('authToken');
              localStorage.removeItem('refreshToken');
              localStorage.removeItem('authUser');
              setToken(null);
              setRefreshToken(null);
              setUser(null);
            }
          } else {
            console.warn('Token verification failed, logging out');
            localStorage.removeItem('authToken');
            localStorage.removeItem('refreshToken');
            localStorage.removeItem('authUser');
            setToken(null);
            setRefreshToken(null);
            setUser(null);
          }
        } catch (error) {
          console.error('Error verifying token:', error);
          localStorage.removeItem('authToken');
          localStorage.removeItem('refreshToken');
          localStorage.removeItem('authUser');
          setToken(null);
          setRefreshToken(null);
          setUser(null);
        }
      }
      setIsLoading(false);
    };

    initAuth();
  }, []);

  const login = async (email: string, password: string) => {
    try {
      const response = await fetch('/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      });

      if (!response.ok) {
        const error = await response.text();
        throw new Error(error || 'Error al iniciar sesión');
      }

      const data = await response.json();

      const userData = {
        id: data.id,
        email: data.email,
        nombre: data.nombre,
        rol: data.rol,
      };

      // Store tokens and user info
      localStorage.setItem('authToken', data.token);
      localStorage.setItem('refreshToken', data.refreshToken);
      localStorage.setItem('authUser', JSON.stringify(userData));

      setToken(data.token);
      setRefreshToken(data.refreshToken);
      setUser(userData);
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  };

  const logout = async () => {
    try {
      // Call backend logout to revoke refresh token
      if (token) {
        await fetch('/auth/logout', {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });
      }
    } catch (error) {
      console.error('Error during logout:', error);
    } finally {
      // Always clear local state
      localStorage.removeItem('authToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('authUser');
      setToken(null);
      setRefreshToken(null);
      setUser(null);
    }
  };

  const value: AuthContextType = {
    user,
    token,
    login,
    logout,
    isAuthenticated: !!token,
    isLoading,
    refreshAccessToken,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
