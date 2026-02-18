using FluentAssertions;
using SincoMaquinaria.DTOs.Requests;
using System;
using Xunit;

namespace SincoMaquinaria.Tests.DTOs;

public class RequestDTOTests
{
    #region CambiarEstadoRequest Tests

    [Fact]
    public void CambiarEstadoRequest_Constructor_DebeEstablecerEstado()
    {
        // Arrange & Act
        var request = new CambiarEstadoRequest(true);

        // Assert
        request.Activo.Should().BeTrue();
    }

    [Fact]
    public void CambiarEstadoRequest_ConEstadoFalse_DebeEstablecerCorrectamente()
    {
        // Arrange & Act
        var request = new CambiarEstadoRequest(false);

        // Assert
        request.Activo.Should().BeFalse();
    }

    [Fact]
    public void CambiarEstadoRequest_PropiedadActivo_DebeSerAccesible()
    {
        // Arrange & Act
        var request = new CambiarEstadoRequest(true);

        // Assert
        request.Activo.Should().BeTrue();
    }

    [Fact]
    public void CambiarEstadoRequest_Equality_DebeCompararCorrectamente()
    {
        // Arrange
        var request1 = new CambiarEstadoRequest(true);
        var request2 = new CambiarEstadoRequest(true);
        var request3 = new CambiarEstadoRequest(false);

        // Assert
        request1.Should().Be(request2);
        request1.Should().NotBe(request3);
    }

    #endregion

    #region RefreshTokenRequest Tests

    [Fact]
    public void RefreshTokenRequest_Constructor_DebeEstablecerRefreshToken()
    {
        // Arrange & Act
        var token = "sample-refresh-token-12345";
        var request = new RefreshTokenRequest(token);

        // Assert
        request.RefreshToken.Should().Be(token);
    }

    [Fact]
    public void RefreshTokenRequest_ConTokenVacio_DebePermitirCreacion()
    {
        // Arrange & Act
        var request = new RefreshTokenRequest("");

        // Assert
        request.RefreshToken.Should().BeEmpty();
    }

    [Fact]
    public void RefreshTokenRequest_PropiedadRefreshToken_DebeSerAccesible()
    {
        // Arrange & Act
        var expectedToken = "my-refresh-token-xyz";
        var request = new RefreshTokenRequest(expectedToken);

        // Assert
        request.RefreshToken.Should().Be(expectedToken);
    }

    [Fact]
    public void RefreshTokenRequest_Equality_DebeCompararCorrectamente()
    {
        // Arrange
        var request1 = new RefreshTokenRequest("token-123");
        var request2 = new RefreshTokenRequest("token-123");
        var request3 = new RefreshTokenRequest("token-456");

        // Assert
        request1.Should().Be(request2);
        request1.Should().NotBe(request3);
    }

    #endregion
}
