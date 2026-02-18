using System;
using FluentAssertions;
using SincoMaquinaria.Domain;
using Xunit;

namespace SincoMaquinaria.Tests.Domain;

public class ErrorLogTests
{
    [Fact]
    public void ErrorLog_ConstructorVacio_DebeCrearInstancia()
    {
        // Act
        var errorLog = new ErrorLog();

        // Assert
        errorLog.Should().NotBeNull();
        errorLog.Id.Should().Be(Guid.Empty);
        errorLog.Message.Should().BeEmpty();
        errorLog.StackTrace.Should().BeEmpty();
        errorLog.Path.Should().BeEmpty();
    }

    [Fact]
    public void ErrorLog_ConstructorConParametros_DebeEstablecerPropiedades()
    {
        // Arrange
        var message = "Test error message";
        var stackTrace = "at Test.Method() in file.cs:line 42";
        var path = "/api/test";

        // Act
        var errorLog = new ErrorLog(message, stackTrace, path);

        // Assert
        errorLog.Id.Should().NotBe(Guid.Empty);
        errorLog.Message.Should().Be(message);
        errorLog.StackTrace.Should().Be(stackTrace);
        errorLog.Path.Should().Be(path);
        errorLog.Fecha.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ErrorLog_ConstructorConParametros_DebeGenerarNuevoGuid()
    {
        // Arrange & Act
        var errorLog1 = new ErrorLog("Message 1", "Stack 1", "/path1");
        var errorLog2 = new ErrorLog("Message 2", "Stack 2", "/path2");

        // Assert
        errorLog1.Id.Should().NotBe(errorLog2.Id);
    }

    [Fact]
    public void ErrorLog_Propiedades_DebenSerModificables()
    {
        // Arrange
        var errorLog = new ErrorLog();
        var newId = Guid.NewGuid();
        var newFecha = DateTimeOffset.UtcNow.AddDays(-1);

        // Act
        errorLog.Id = newId;
        errorLog.Message = "Updated message";
        errorLog.StackTrace = "Updated stack trace";
        errorLog.Path = "/updated/path";
        errorLog.Fecha = newFecha;

        // Assert
        errorLog.Id.Should().Be(newId);
        errorLog.Message.Should().Be("Updated message");
        errorLog.StackTrace.Should().Be("Updated stack trace");
        errorLog.Path.Should().Be("/updated/path");
        errorLog.Fecha.Should().Be(newFecha);
    }

    [Fact]
    public void ErrorLog_ConParametrosVacios_DebeCrearseCorrectamente()
    {
        // Act
        var errorLog = new ErrorLog("", "", "");

        // Assert
        errorLog.Id.Should().NotBe(Guid.Empty);
        errorLog.Message.Should().BeEmpty();
        errorLog.StackTrace.Should().BeEmpty();
        errorLog.Path.Should().BeEmpty();
    }

    [Fact]
    public void ErrorLog_ConMensajeLargo_DebeAlmacenarCompleto()
    {
        // Arrange
        var longMessage = new string('A', 1000);
        var longStackTrace = new string('B', 5000);

        // Act
        var errorLog = new ErrorLog(longMessage, longStackTrace, "/api/endpoint");

        // Assert
        errorLog.Message.Should().Be(longMessage);
        errorLog.StackTrace.Should().Be(longStackTrace);
    }
}
