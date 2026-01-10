import { useEffect, useState } from 'react';
import { ErrorLog } from '../types';
import {
    Box, Typography, Container, Paper, TableContainer, Table, TableHead, TableRow, TableCell, TableBody,
    Button, IconButton, Collapse, Alert
} from '@mui/material';
import { Refresh, KeyboardArrowDown, KeyboardArrowUp } from '@mui/icons-material';

const LogsErrores = () => {
    const [logs, setLogs] = useState<ErrorLog[]>([]);
    const [loading, setLoading] = useState(true);

    const fetchLogs = async () => {
        setLoading(true);
        try {
            const response = await fetch('/admin/logs');
            if (response.ok) {
                const data = await response.json();
                setLogs(data);
            } else {
                console.error("Failed to fetch logs");
            }
        } catch (error) {
            console.error("Error fetching logs:", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchLogs();
    }, []);

    return (
        <Container maxWidth="xl" sx={{ mt: 4 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
                <Typography variant="h4" component="h1" fontWeight="bold">
                    Logs de Errores
                </Typography>
                <Button variant="contained" startIcon={<Refresh />} onClick={fetchLogs}>
                    Refrescar
                </Button>
            </Box>

            <TableContainer component={Paper} elevation={3} sx={{ borderRadius: 2 }}>
                {loading ? (
                    <Box sx={{ p: 4, textAlign: 'center' }}>
                        <Typography>Cargando logs...</Typography>
                    </Box>
                ) : logs.length === 0 ? (
                    <Box sx={{ p: 4, textAlign: 'center' }}>
                        <Typography color="text.secondary">No hay errores registrados.</Typography>
                    </Box>
                ) : (
                    <Table aria-label="logs table">
                        <TableHead sx={{ bgcolor: 'action.hover' }}>
                            <TableRow>
                                <TableCell />
                                <TableCell><strong>Fecha</strong></TableCell>
                                <TableCell><strong>Ruta</strong></TableCell>
                                <TableCell><strong>Mensaje</strong></TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {logs.map((log) => (
                                <LogRow key={log.id} log={log} />
                            ))}
                        </TableBody>
                    </Table>
                )}
            </TableContainer>
        </Container>
    );
};

const LogRow = ({ log }: { log: ErrorLog }) => {
    const [open, setOpen] = useState(false);

    return (
        <>
            <TableRow sx={{ '& > *': { borderBottom: 'unset' } }} hover>
                <TableCell>
                    <IconButton
                        aria-label="expand row"
                        size="small"
                        onClick={() => setOpen(!open)}
                    >
                        {open ? <KeyboardArrowUp /> : <KeyboardArrowDown />}
                    </IconButton>
                </TableCell>
                <TableCell component="th" scope="row">
                    {new Date(log.fecha).toLocaleString()}
                </TableCell>
                <TableCell sx={{ fontFamily: 'monospace', color: 'warning.main' }}>
                    {log.path}
                </TableCell>
                <TableCell>{log.message}</TableCell>
            </TableRow>
            <TableRow>
                <TableCell style={{ paddingBottom: 0, paddingTop: 0 }} colSpan={6}>
                    <Collapse in={open} timeout="auto" unmountOnExit>
                        <Box sx={{ margin: 1 }}>
                            <Typography variant="h6" gutterBottom component="div" sx={{ fontSize: '0.9rem' }}>
                                Stack Trace
                            </Typography>
                            <Paper sx={{ p: 2, bgcolor: 'background.default', fontFamily: 'monospace', fontSize: '0.8rem', overflowX: 'auto' }}>
                                <pre style={{ margin: 0 }}>{log.stackTrace || "No stack trace available"}</pre>
                            </Paper>
                        </Box>
                    </Collapse>
                </TableCell>
            </TableRow>
        </>
    );
};

export default LogsErrores;
