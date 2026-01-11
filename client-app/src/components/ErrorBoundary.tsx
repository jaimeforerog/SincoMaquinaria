import React, { Component, ErrorInfo, ReactNode } from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';
import { Error as ErrorIcon } from '@mui/icons-material';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
  errorInfo: ErrorInfo | null;
}

class ErrorBoundary extends Component<Props, State> {
  public state: State = {
    hasError: false,
    error: null,
    errorInfo: null
  };

  public static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error, errorInfo: null };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('ErrorBoundary caught an error:', error, errorInfo);
    this.setState({
      error,
      errorInfo
    });
  }

  private handleReset = () => {
    this.setState({ hasError: false, error: null, errorInfo: null });
    window.location.href = '/';
  };

  public render() {
    if (this.state.hasError) {
      return (
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '100vh',
            bgcolor: 'background.default',
            p: 3
          }}
        >
          <Paper
            elevation={3}
            sx={{
              p: 4,
              maxWidth: 600,
              textAlign: 'center'
            }}
          >
            <ErrorIcon sx={{ fontSize: 60, color: 'error.main', mb: 2 }} />
            <Typography variant="h4" gutterBottom color="error">
              Error en la Aplicación
            </Typography>
            <Typography variant="body1" color="text.secondary" paragraph>
              Ocurrió un error inesperado al cargar esta página.
            </Typography>

            {this.state.error && (
              <Box
                sx={{
                  mt: 3,
                  p: 2,
                  bgcolor: 'action.hover',
                  borderRadius: 1,
                  textAlign: 'left',
                  overflow: 'auto'
                }}
              >
                <Typography variant="subtitle2" color="error" gutterBottom>
                  Error:
                </Typography>
                <Typography variant="body2" component="pre" sx={{ whiteSpace: 'pre-wrap' }}>
                  {this.state.error.toString()}
                </Typography>

                {this.state.errorInfo && (
                  <>
                    <Typography variant="subtitle2" color="error" gutterBottom sx={{ mt: 2 }}>
                      Stack Trace:
                    </Typography>
                    <Typography variant="body2" component="pre" sx={{ whiteSpace: 'pre-wrap', fontSize: '0.75rem' }}>
                      {this.state.errorInfo.componentStack}
                    </Typography>
                  </>
                )}
              </Box>
            )}

            <Button
              variant="contained"
              color="primary"
              onClick={this.handleReset}
              sx={{ mt: 3 }}
            >
              Volver al Inicio
            </Button>
          </Paper>
        </Box>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;