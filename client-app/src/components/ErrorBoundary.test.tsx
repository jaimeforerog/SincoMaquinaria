import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import ErrorBoundary from './ErrorBoundary';

const ThrowingComponent = ({ shouldThrow }: { shouldThrow: boolean }) => {
    if (shouldThrow) throw new Error('Test error message');
    return <div>Child Content</div>;
};

describe('ErrorBoundary', () => {
    const originalConsoleError = console.error;
    const originalLocation = window.location;

    beforeEach(() => {
        console.error = vi.fn();
    });

    afterEach(() => {
        console.error = originalConsoleError;
        Object.defineProperty(window, 'location', {
            writable: true,
            value: originalLocation,
        });
    });

    it('renders children when no error', () => {
        render(
            <ErrorBoundary>
                <ThrowingComponent shouldThrow={false} />
            </ErrorBoundary>
        );
        expect(screen.getByText('Child Content')).toBeInTheDocument();
    });

    it('renders error UI when child throws', () => {
        render(
            <ErrorBoundary>
                <ThrowingComponent shouldThrow={true} />
            </ErrorBoundary>
        );
        expect(screen.getByText('Error en la Aplicación')).toBeInTheDocument();
    });

    it('displays error message in details', () => {
        render(
            <ErrorBoundary>
                <ThrowingComponent shouldThrow={true} />
            </ErrorBoundary>
        );
        expect(screen.getByText(/Test error message/)).toBeInTheDocument();
    });

    it('renders Volver al Inicio button', () => {
        render(
            <ErrorBoundary>
                <ThrowingComponent shouldThrow={true} />
            </ErrorBoundary>
        );
        expect(screen.getByText('Volver al Inicio')).toBeInTheDocument();
    });

    it('navigates home on reset click', () => {
        Object.defineProperty(window, 'location', {
            writable: true,
            value: { href: '' },
        });

        render(
            <ErrorBoundary>
                <ThrowingComponent shouldThrow={true} />
            </ErrorBoundary>
        );
        fireEvent.click(screen.getByText('Volver al Inicio'));
        expect(window.location.href).toBe('/');
    });

    it('does not show error UI initially', () => {
        render(
            <ErrorBoundary>
                <ThrowingComponent shouldThrow={false} />
            </ErrorBoundary>
        );
        expect(screen.queryByText('Error en la Aplicación')).toBeNull();
    });
});
