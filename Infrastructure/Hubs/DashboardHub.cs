using Microsoft.AspNetCore.SignalR;

namespace SincoMaquinaria.Infrastructure.Hubs;

public class DashboardHub : Hub
{
    // Por ahora no necesitamos métodos específicos recibidos desde el cliente,
    // pero aquí podríamos manejar suscripciones a grupos específicos (ej. por rol).
    
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}
