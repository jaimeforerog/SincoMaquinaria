using Microsoft.AspNetCore.SignalR;
using SincoMaquinaria.Infrastructure.Hubs;

namespace SincoMaquinaria.Services;

public class DashboardNotifier
{
    private readonly IHubContext<DashboardHub> _hubContext;

    public DashboardNotifier(IHubContext<DashboardHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotificarOrdenCreada()
    {
        await _hubContext.Clients.All.SendAsync("DashboardUpdate", "OrdenCreada");
    }

    public async Task NotificarEquiposImportados()
    {
        await _hubContext.Clients.All.SendAsync("DashboardUpdate", "EquiposImportados");
    }
}
