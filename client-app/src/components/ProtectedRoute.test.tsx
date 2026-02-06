import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import ProtectedRoute from './ProtectedRoute';
import { MemoryRouter, Route, Routes } from 'react-router-dom';

// Mock useAuth
const mockUseAuth = vi.fn();

vi.mock('../contexts/AuthContext', () => ({
    useAuth: () => mockUseAuth()
}));

const TestComponent = () => <div>Protected Content</div>;
const Login = () => <div>Login Page</div>;

describe('ProtectedRoute', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('shows loading spinner when loading', () => {
        mockUseAuth.mockReturnValue({
            isLoading: true,
            isAuthenticated: false
        });

        render(
            <MemoryRouter>
                <ProtectedRoute>
                    <TestComponent />
                </ProtectedRoute>
            </MemoryRouter>
        );

        expect(screen.getByRole('progressbar')).toBeInTheDocument();
        expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
    });

    it('redirects to login when not authenticated', () => {
        mockUseAuth.mockReturnValue({
            isLoading: false,
            isAuthenticated: false
        });

        render(
            <MemoryRouter initialEntries={['/protected']}>
                <Routes>
                    <Route path="/protected" element={
                        <ProtectedRoute>
                            <TestComponent />
                        </ProtectedRoute>
                    } />
                    <Route path="/login" element={<Login />} />
                </Routes>
            </MemoryRouter>
        );

        expect(screen.getByText('Login Page')).toBeInTheDocument();
        expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
    });

    it('renders children when authenticated', () => {
        mockUseAuth.mockReturnValue({
            isLoading: false,
            isAuthenticated: true
        });

        render(
            <MemoryRouter>
                <ProtectedRoute>
                    <TestComponent />
                </ProtectedRoute>
            </MemoryRouter>
        );

        expect(screen.getByText('Protected Content')).toBeInTheDocument();
    });
});
