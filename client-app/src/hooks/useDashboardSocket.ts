import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

export const useDashboardSocket = (onUpdate: (type: string) => void) => {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);

    useEffect(() => {
        // Build connection
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5000/hubs/dashboard")
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);
    }, []);

    useEffect(() => {
        if (connection) {
            connection.start()
                .then(() => {
                    console.log('SignalR Connected!');

                    connection.on('DashboardUpdate', (message: string) => {
                        console.log('DashboardUpdate received:', message);
                        onUpdate(message);
                    });
                })
                .catch(e => console.log('Connection failed: ', e));
        }
    }, [connection, onUpdate]);
};
