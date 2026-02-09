// DEPRECATED: Este archivo re-exporta los eventos de los archivos separados
// Los eventos han sido reorganizados en:
//   - Events/OrdenDeTrabajo/OrdenDeTrabajoEvents.cs
//   - Events/Equipo/EquipoEvents.cs
//   - Events/Empleado/EmpleadoEvents.cs
//   - Events/Usuario/UsuarioEvents.cs
//   - Events/ConfiguracionGlobal/ConfiguracionGlobalEvents.cs
//
// Por favor actualiza los imports para usar los namespaces específicos:
//   - using SincoMaquinaria.Domain.Events.OrdenDeTrabajo;
//   - using SincoMaquinaria.Domain.Events.Equipo;
//   - using SincoMaquinaria.Domain.Events.Empleado;
//   - using SincoMaquinaria.Domain.Events.Usuario;
//   - using SincoMaquinaria.Domain.Events.ConfiguracionGlobal;

global using SincoMaquinaria.Domain.Events.OrdenDeTrabajo;
global using SincoMaquinaria.Domain.Events.Equipo;
global using SincoMaquinaria.Domain.Events.Empleado;
global using SincoMaquinaria.Domain.Events.Usuario;
global using SincoMaquinaria.Domain.Events.ConfiguracionGlobal;

namespace SincoMaquinaria.Domain.Events;

// Este namespace está vacío - todos los eventos están en los archivos separados
// Para evitar breaking changes, usamos global usings arriba para re-exportar todo
