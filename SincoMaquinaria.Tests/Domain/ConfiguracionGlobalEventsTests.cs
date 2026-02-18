using System;
using FluentAssertions;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events.ConfiguracionGlobal;
using Xunit;

namespace SincoMaquinaria.Tests.Domain;

public class ConfiguracionGlobalEventsTests
{
    #region CausaFallaActualizada Tests

    [Fact]
    public void CausaFallaActualizada_ConDatosCompletos_DebeCrearseCorrectamente()
    {
        // Arrange & Act
        var usuarioId = Guid.NewGuid();
        var evento = new CausaFallaActualizada(
            "CAUSA-001",
            "Falta de mantenimiento preventivo",
            usuarioId,
            "Juan Pérez"
        );

        // Assert
        evento.Codigo.Should().Be("CAUSA-001");
        evento.Descripcion.Should().Be("Falta de mantenimiento preventivo");
        evento.UsuarioId.Should().Be(usuarioId);
        evento.UsuarioNombre.Should().Be("Juan Pérez");
    }

    [Fact]
    public void CausaFallaActualizada_SinDatosOpcionales_DebeCrearseCorrectamente()
    {
        // Arrange & Act
        var evento = new CausaFallaActualizada("CAUSA-002", "Sobrecarga");

        // Assert
        evento.Codigo.Should().Be("CAUSA-002");
        evento.Descripcion.Should().Be("Sobrecarga");
        evento.UsuarioId.Should().BeNull();
        evento.UsuarioNombre.Should().BeNull();
    }

    [Fact]
    public void CausaFallaActualizada_Deconstruct_DebeExtraerPropiedades()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var evento = new CausaFallaActualizada("COD", "Desc", usuarioId, "User");

        // Act
        var (codigo, descripcion, userId, userName) = evento;

        // Assert
        codigo.Should().Be("COD");
        descripcion.Should().Be("Desc");
        userId.Should().Be(usuarioId);
        userName.Should().Be("User");
    }

    #endregion

    #region EstadoCausaFallaCambiado Tests

    [Fact]
    public void EstadoCausaFallaCambiado_ConDatosCompletos_DebeCrearseCorrectamente()
    {
        // Arrange & Act
        var usuarioId = Guid.NewGuid();
        var evento = new EstadoCausaFallaCambiado(
            "CAUSA-001",
            true,
            usuarioId,
            "Admin User"
        );

        // Assert
        evento.Codigo.Should().Be("CAUSA-001");
        evento.Activo.Should().BeTrue();
        evento.UsuarioId.Should().Be(usuarioId);
        evento.UsuarioNombre.Should().Be("Admin User");
    }

    [Fact]
    public void EstadoCausaFallaCambiado_SinDatosOpcionales_DebeCrearseCorrectamente()
    {
        // Arrange & Act
        var evento = new EstadoCausaFallaCambiado("CAUSA-002", false);

        // Assert
        evento.Codigo.Should().Be("CAUSA-002");
        evento.Activo.Should().BeFalse();
        evento.UsuarioId.Should().BeNull();
        evento.UsuarioNombre.Should().BeNull();
    }

    #endregion

    #region EstadoGrupoMantenimientoCambiado Tests

    [Fact]
    public void EstadoGrupoMantenimientoCambiado_ConDatosCompletos_DebeCrearseCorrectamente()
    {
        // Arrange & Act
        var usuarioId = Guid.NewGuid();
        var evento = new EstadoGrupoMantenimientoCambiado(
            "GRP-001",
            true,
            usuarioId,
            "System Admin"
        );

        // Assert
        evento.Codigo.Should().Be("GRP-001");
        evento.Activo.Should().BeTrue();
        evento.UsuarioId.Should().Be(usuarioId);
        evento.UsuarioNombre.Should().Be("System Admin");
    }

    [Fact]
    public void EstadoGrupoMantenimientoCambiado_SinDatosOpcionales_DebeCrearseCorrectamente()
    {
        // Arrange & Act
        var evento = new EstadoGrupoMantenimientoCambiado("GRP-002", false);

        // Assert
        evento.Codigo.Should().Be("GRP-002");
        evento.Activo.Should().BeFalse();
        evento.UsuarioId.Should().BeNull();
        evento.UsuarioNombre.Should().BeNull();
    }

    #endregion

    #region EstadoTipoMedidorCambiado Tests

    [Fact]
    public void EstadoTipoMedidorCambiado_ConDatosCompletos_DebeCrearseCorrectamente()
    {
        // Arrange & Act
        var usuarioId = Guid.NewGuid();
        var evento = new EstadoTipoMedidorCambiado(
            "MED-001",
            true,
            usuarioId,
            "Operator"
        );

        // Assert
        evento.Codigo.Should().Be("MED-001");
        evento.Activo.Should().BeTrue();
        evento.UsuarioId.Should().Be(usuarioId);
        evento.UsuarioNombre.Should().Be("Operator");
    }

    [Fact]
    public void EstadoTipoMedidorCambiado_SinDatosOpcionales_DebeCrearseCorrectamente()
    {
        // Arrange & Act
        var evento = new EstadoTipoMedidorCambiado("MED-002", false);

        // Assert
        evento.Codigo.Should().Be("MED-002");
        evento.Activo.Should().BeFalse();
        evento.UsuarioId.Should().BeNull();
        evento.UsuarioNombre.Should().BeNull();
    }

    #endregion

    #region GrupoMantenimientoActualizado Tests

    [Fact]
    public void GrupoMantenimientoActualizado_ConDatosCompletos_DebeCrearseCorrectamente()
    {
        // Arrange & Act
        var usuarioId = Guid.NewGuid();
        var evento = new GrupoMantenimientoActualizado(
            "GRP-001",
            "Excavadoras",
            "Grupo de excavadoras y retroexcavadoras",
            usuarioId,
            "Maintenance Manager"
        );

        // Assert
        evento.Codigo.Should().Be("GRP-001");
        evento.Nombre.Should().Be("Excavadoras");
        evento.Descripcion.Should().Be("Grupo de excavadoras y retroexcavadoras");
        evento.UsuarioId.Should().Be(usuarioId);
        evento.UsuarioNombre.Should().Be("Maintenance Manager");
    }

    [Fact]
    public void GrupoMantenimientoActualizado_SinDatosOpcionales_DebeCrearseCorrectamente()
    {
        // Arrange & Act
        var evento = new GrupoMantenimientoActualizado("GRP-002", "Camiones", "Desc");

        // Assert
        evento.Codigo.Should().Be("GRP-002");
        evento.Nombre.Should().Be("Camiones");
        evento.Descripcion.Should().Be("Desc");
        evento.UsuarioId.Should().BeNull();
        evento.UsuarioNombre.Should().BeNull();
    }

    #endregion

    #region TipoMedidorActualizado Tests

    [Fact]
    public void TipoMedidorActualizado_ConDatosCompletos_DebeCrearseCorrectamente()
    {
        // Arrange & Act
        var usuarioId = Guid.NewGuid();
        var evento = new TipoMedidorActualizado(
            "MED-001",
            "Horómetro",
            "Horas",
            usuarioId,
            "Tech Admin"
        );

        // Assert
        evento.Codigo.Should().Be("MED-001");
        evento.Nombre.Should().Be("Horómetro");
        evento.Unidad.Should().Be("Horas");
        evento.UsuarioId.Should().Be(usuarioId);
        evento.UsuarioNombre.Should().Be("Tech Admin");
    }

    [Fact]
    public void TipoMedidorActualizado_SinDatosOpcionales_DebeCrearseCorrectamente()
    {
        // Arrange & Act
        var evento = new TipoMedidorActualizado("MED-002", "Odómetro", "Km");

        // Assert
        evento.Codigo.Should().Be("MED-002");
        evento.Nombre.Should().Be("Odómetro");
        evento.Unidad.Should().Be("Km");
        evento.UsuarioId.Should().BeNull();
        evento.UsuarioNombre.Should().BeNull();
    }

    #endregion

    #region ConfiguracionGlobal Apply Tests - Duplicate Detection

    [Fact]
    public void Apply_TipoMedidorCreado_Duplicado_DebeLanzarDomainException()
    {
        var config = new ConfiguracionGlobal();
        config.Apply(new TipoMedidorCreado("MED-001", "Horómetro", "Horas"));

        var act = () => config.Apply(new TipoMedidorCreado("MED-001", "Otro", "Otra"));
        act.Should().Throw<DomainException>().Which.Message.Should().Contain("MED-001");
    }

    [Fact]
    public void Apply_GrupoMantenimientoCreado_Duplicado_DebeLanzarDomainException()
    {
        var config = new ConfiguracionGlobal();
        config.Apply(new GrupoMantenimientoCreado("GRP-001", "Excavadoras", "Desc", true));

        var act = () => config.Apply(new GrupoMantenimientoCreado("GRP-001", "Otro", "Otra", true));
        act.Should().Throw<DomainException>().Which.Message.Should().Contain("GRP-001");
    }

    [Fact]
    public void Apply_TipoFallaCreado_Duplicado_DebeLanzarDomainException()
    {
        var config = new ConfiguracionGlobal();
        config.Apply(new TipoFallaCreado("FALLA-001", "Mecánica", "Alta"));

        var act = () => config.Apply(new TipoFallaCreado("FALLA-001", "Otra", "Baja"));
        act.Should().Throw<DomainException>().Which.Message.Should().Contain("FALLA-001");
    }

    [Fact]
    public void Apply_CausaFallaCreada_Duplicado_DebeLanzarDomainException()
    {
        var config = new ConfiguracionGlobal();
        config.Apply(new CausaFallaCreada("CAUSA-001", "Desgaste"));

        var act = () => config.Apply(new CausaFallaCreada("CAUSA-001", "Otra"));
        act.Should().Throw<DomainException>().Which.Message.Should().Contain("CAUSA-001");
    }

    #endregion

    #region ConfiguracionGlobal Apply Tests - Non-existent Entity

    [Fact]
    public void Apply_EstadoTipoMedidorCambiado_NoExiste_DebeLanzarDomainException()
    {
        var config = new ConfiguracionGlobal();

        var act = () => config.Apply(new EstadoTipoMedidorCambiado("NO-EXISTE", false));
        act.Should().Throw<DomainException>().Which.Message.Should().Contain("NO-EXISTE");
    }

    [Fact]
    public void Apply_TipoMedidorActualizado_NoExiste_DebeLanzarDomainException()
    {
        var config = new ConfiguracionGlobal();

        var act = () => config.Apply(new TipoMedidorActualizado("NO-EXISTE", "Nombre", "Unidad"));
        act.Should().Throw<DomainException>().Which.Message.Should().Contain("NO-EXISTE");
    }

    [Fact]
    public void Apply_EstadoGrupoMantenimientoCambiado_NoExiste_DebeLanzarDomainException()
    {
        var config = new ConfiguracionGlobal();

        var act = () => config.Apply(new EstadoGrupoMantenimientoCambiado("NO-EXISTE", false));
        act.Should().Throw<DomainException>().Which.Message.Should().Contain("NO-EXISTE");
    }

    [Fact]
    public void Apply_GrupoMantenimientoActualizado_NoExiste_DebeLanzarDomainException()
    {
        var config = new ConfiguracionGlobal();

        var act = () => config.Apply(new GrupoMantenimientoActualizado("NO-EXISTE", "Nombre", "Desc"));
        act.Should().Throw<DomainException>().Which.Message.Should().Contain("NO-EXISTE");
    }

    [Fact]
    public void Apply_CausaFallaActualizada_NoExiste_DebeLanzarDomainException()
    {
        var config = new ConfiguracionGlobal();

        var act = () => config.Apply(new CausaFallaActualizada("NO-EXISTE", "Desc"));
        act.Should().Throw<DomainException>().Which.Message.Should().Contain("NO-EXISTE");
    }

    [Fact]
    public void Apply_EstadoCausaFallaCambiado_NoExiste_DebeLanzarDomainException()
    {
        var config = new ConfiguracionGlobal();

        var act = () => config.Apply(new EstadoCausaFallaCambiado("NO-EXISTE", false));
        act.Should().Throw<DomainException>().Which.Message.Should().Contain("NO-EXISTE");
    }

    #endregion
}
