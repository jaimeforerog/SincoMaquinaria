import { renderHook } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useDashboardSocket } from './useDashboardSocket';

// Mock signalR
const mockOn = vi.fn();
const mockStart = vi.fn().mockResolvedValue(undefined);
const mockBuild = vi.fn(() => ({
    on: mockOn,
    start: mockStart
}));
const mockWithAutomaticReconnect = vi.fn(() => ({
    build: mockBuild
}));
const mockWithUrl = vi.fn(() => ({
    withAutomaticReconnect: mockWithAutomaticReconnect
}));

vi.mock('@microsoft/signalr', () => ({
    HubConnectionBuilder: vi.fn(() => ({
        withUrl: mockWithUrl
    }))
}));

describe('useDashboardSocket', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('establishes connection on mount', () => {
        const onUpdate = vi.fn();
        renderHook(() => useDashboardSocket(onUpdate));

        expect(mockWithUrl).toHaveBeenCalledWith(expect.stringContaining('/hubs/dashboard'));
        expect(mockWithAutomaticReconnect).toHaveBeenCalled();
        expect(mockBuild).toHaveBeenCalled();
    });

    it('starts connection and registers event handler', async () => {
        const onUpdate = vi.fn();
        renderHook(() => useDashboardSocket(onUpdate));

        // Use setTimeout to allow useEffect to run
        await new Promise(resolve => setTimeout(resolve, 0));

        expect(mockStart).toHaveBeenCalled();
        expect(mockOn).toHaveBeenCalledWith('DashboardUpdate', expect.any(Function));
    });

    it('triggers onUpdate callback when message received', async () => {
        const onUpdate = vi.fn();
        renderHook(() => useDashboardSocket(onUpdate));

        await new Promise(resolve => setTimeout(resolve, 0));

        // Get the callback passed to 'on'
        const callback = mockOn.mock.calls.find(call => call[0] === 'DashboardUpdate')[1];

        // Simulate receiving a message
        callback('OrderCreated');

        expect(onUpdate).toHaveBeenCalledWith('OrderCreated');
    });
});
