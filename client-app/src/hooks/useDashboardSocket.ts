import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

export const useDashboardSocket = (onUpdate: (type: string) => void) => {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);

    useEffect(() => {
        // Build connection
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/dashboard")
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);
    }, []);

    useEffect(() => {
        if (!connection) return;

        connection.start()
            .then(() => {
                connection.on('DashboardUpdate', (message: string) => {
                    onUpdate(message);
                });
            })
            .catch(e => console.error('SignalR connection failed:', e));

        return () => {
            connection.stop();
        };
    }, [connection, onUpdate]);
};
