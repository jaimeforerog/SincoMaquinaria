using Xunit;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.Domain.Events.ConfiguracionGlobal;

namespace SincoMaquinaria.Tests.Domain;

public class ConfiguracionGlobalTests
{
    [Fact]
    public void ConfiguracionGlobal_CausaFallaCreada_DebeAgregarCausa()
    {
        // Arrange
        var config = new ConfiguracionGlobal();
        var evento = new CausaFallaCreada("CAUSA001", "Desgaste por uso continuo");

        // Act
        config.Apply(evento);

        // Assert
        Assert.Single(config.CausasFalla);
        var causa = config.CausasFalla[0];
        Assert.Equal("CAUSA001", causa.Codigo);
        Assert.Equal("Desgaste por uso continuo", causa.Descripcion);
        Assert.True(causa.Activo);
    }

    [Fact]
    public void ConfiguracionGlobal_CausaFallaActualizada_DebeModificarDescripcion()
    {
        // Arrange
        var config = new ConfiguracionGlobal();
        config.Apply(new CausaFallaCreada("CAUSA001", "Descripción original"));
        
        // Act
        var eventoActualizacion = new CausaFallaActualizada("CAUSA001", "Descripción actualizada");
        config.Apply(eventoActualizacion);

        // Assert
        var causa = config.CausasFalla[0];
        Assert.Equal("Descripción actualizada", causa.Descripcion);
    }

    [Fact]
    public void ConfiguracionGlobal_EstadoCausaFallaCambiado_DebeToggleActivo()
    {
        // Arrange
        var config = new ConfiguracionGlobal();
        config.Apply(new CausaFallaCreada("CAUSA001", "Falta de mantenimiento"));
        
        // Act
        var eventoCambioEstado = new EstadoCausaFallaCambiado("CAUSA001", false);
        config.Apply(eventoCambioEstado);

        // Assert
        var causa = config.CausasFalla[0];
        Assert.False(causa.Activo);
    }

    [Fact]
    public void ConfiguracionGlobal_TipoFallaCreado_DebeAgregarTipo()
    {
        // Arrange
        var config = new ConfiguracionGlobal();
        var evento = new TipoFallaCreado("TIPO001", "Falla mecánica", "Alta");

        // Act
        config.Apply(evento);

        // Assert
        Assert.Single(config.TiposFalla);
        var tipo = config.TiposFalla[0];
        Assert.Equal("TIPO001", tipo.Codigo);
        Assert.Equal("Falla mecánica", tipo.Descripcion);
        Assert.Equal("Alta", tipo.Prioridad);
        Assert.True(tipo.Activo);
    }

    [Fact]
    public void ConfiguracionGlobal_NoDuplicarCausaFalla_CuandoYaExiste()
    {
        // Arrange
        var config = new ConfiguracionGlobal();
        var evento1 = new CausaFallaCreada("CAUSA001", "Primera causa");
        var evento2 = new CausaFallaCreada("CAUSA001", "Intento duplicado");

        // Act
        config.Apply(evento1);
        config.Apply(evento2);

        // Assert
        Assert.Single(config.CausasFalla);
        Assert.Equal("Primera causa", config.CausasFalla[0].Descripcion);
    }

    [Fact]
    public void ConfiguracionGlobal_TipoMedidorCreado_DebeAgregarTipoMedidor()
    {
        // Arrange
        var config = new ConfiguracionGlobal();
        var evento = new TipoMedidorCreado("MED001", "Horómetro", "HRS");

        // Act
        config.Apply(evento);

        // Assert
        Assert.Single(config.TiposMedidor);
        var tipo = config.TiposMedidor[0];
        Assert.Equal("MED001", tipo.Codigo);
        Assert.Equal("Horómetro", tipo.Nombre);
        Assert.Equal("HRS", tipo.Unidad);
        Assert.True(tipo.Activo);
    }

    [Fact]
    public void ConfiguracionGlobal_TipoMedidorActualizado_DebeModificarDatos()
    {
        // Arrange
        var config = new ConfiguracionGlobal();
        config.Apply(new TipoMedidorCreado("MED001", "Horómetro", "HRS"));

        // Act
        var eventoActualizado = new TipoMedidorActualizado("MED001", "Horómetro Digital", "HORAS");
        config.Apply(eventoActualizado);

        // Assert
        var tipo = config.TiposMedidor[0];
        Assert.Equal("Horómetro Digital", tipo.Nombre);
        Assert.Equal("HORAS", tipo.Unidad);
    }

    [Fact]
    public void ConfiguracionGlobal_EstadoTipoMedidorCambiado_DebeDesactivar()
    {
        // Arrange
        var config = new ConfiguracionGlobal();
        config.Apply(new TipoMedidorCreado("MED001", "Odómetro", "KM"));

        // Act
        var eventoCambioEstado = new EstadoTipoMedidorCambiado("MED001", false);
        config.Apply(eventoCambioEstado);

        // Assert
        var tipo = config.TiposMedidor[0];
        Assert.False(tipo.Activo);
    }

    [Fact]
    public void ConfiguracionGlobal_GrupoMantenimientoCreado_DebeAgregarGrupo()
    {
        // Arrange
        var config = new ConfiguracionGlobal();
        var evento = new GrupoMantenimientoCreado("GRP001", "Mantenimiento Preventivo", "Tareas preventivas regulares", true);

        // Act
        config.Apply(evento);

        // Assert
        Assert.Single(config.GruposMantenimiento);
        var grupo = config.GruposMantenimiento[0];
        Assert.Equal("GRP001", grupo.Codigo);
        Assert.Equal("Mantenimiento Preventivo", grupo.Nombre);
        Assert.Equal("Tareas preventivas regulares", grupo.Descripcion);
        Assert.True(grupo.Activo);
    }

    [Fact]
    public void ConfiguracionGlobal_GrupoMantenimientoActualizado_DebeModificarDatos()
    {
        // Arrange
        var config = new ConfiguracionGlobal();
        config.Apply(new GrupoMantenimientoCreado("GRP001", "Preventivo", "Desc original", true));

        // Act
        var eventoActualizado = new GrupoMantenimientoActualizado("GRP001", "Mantenimiento Preventivo", "Descripción actualizada");
        config.Apply(eventoActualizado);

        // Assert
        var grupo = config.GruposMantenimiento[0];
        Assert.Equal("Mantenimiento Preventivo", grupo.Nombre);
        Assert.Equal("Descripción actualizada", grupo.Descripcion);
    }

    [Fact]
    public void ConfiguracionGlobal_EstadoGrupoMantenimientoCambiado_DebeDesactivar()
    {
        // Arrange
        var config = new ConfiguracionGlobal();
        config.Apply(new GrupoMantenimientoCreado("GRP001", "Correctivo", "Desc", true));

        // Act
        var eventoCambioEstado = new EstadoGrupoMantenimientoCambiado("GRP001", false);
        config.Apply(eventoCambioEstado);

        // Assert
        var grupo = config.GruposMantenimiento[0];
        Assert.False(grupo.Activo);
    }

    [Fact]
    public void ConfiguracionGlobal_MultiplesEventos_DebeMantenerOrdenYEstado()
    {
        // Arrange
        var config = new ConfiguracionGlobal();

        // Act
        config.Apply(new TipoMedidorCreado("MED001", "Horómetro", "HRS"));
        config.Apply(new TipoMedidorCreado("MED002", "Odómetro", "KM"));
        config.Apply(new GrupoMantenimientoCreado("GRP001", "Preventivo", "Desc1", true));
        config.Apply(new TipoFallaCreado("TIPO001", "Mecánica", "Alta"));
        config.Apply(new CausaFallaCreada("CAUSA001", "Desgaste"));

        // Assert
        Assert.Equal(2, config.TiposMedidor.Count);
        Assert.Single(config.GruposMantenimiento);
        Assert.Single(config.TiposFalla);
        Assert.Single(config.CausasFalla);
        Assert.Equal(ConfiguracionGlobal.SingletonId, config.Id);
    }
}
