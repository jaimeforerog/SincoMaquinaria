using FluentAssertions;
using SincoMaquinaria.Domain;
using Xunit;

namespace SincoMaquinaria.Tests.Domain;

public class DomainExceptionTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithSingleMessage_ShouldSetMessageAndErrors()
    {
        // Arrange
        var message = "Validation error occurred";

        // Act
        var exception = new DomainException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.Errors.Should().ContainSingle();
        exception.Errors[0].Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMultipleErrors_ShouldJoinErrorsInMessage()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2", "Error 3" };

        // Act
        var exception = new DomainException(errors);

        // Assert
        exception.Message.Should().Be("Error 1; Error 2; Error 3");
        exception.Errors.Should().BeEquivalentTo(errors);
        exception.Errors.Should().HaveCount(3);
    }

    [Fact]
    public void Constructor_WithEmptyErrorArray_ShouldCreateException()
    {
        // Arrange
        var errors = Array.Empty<string>();

        // Act
        var exception = new DomainException(errors);

        // Assert
        exception.Errors.Should().BeEmpty();
        exception.Message.Should().Be("");
    }

    [Fact]
    public void Constructor_WithSingleErrorArray_ShouldNotHaveSemicolon()
    {
        // Arrange
        var errors = new[] { "Single error" };

        // Act
        var exception = new DomainException(errors);

        // Assert
        exception.Message.Should().Be("Single error");
        exception.Errors.Should().ContainSingle();
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Errors_ShouldBeImmutable()
    {
        // Arrange
        var exception = new DomainException("Test error");

        // Act
        var errors = exception.Errors;

        // Assert
        errors.Should().NotBeNull();
        errors.Should().BeAssignableTo<string[]>();
    }

    [Fact]
    public void DomainException_ShouldInheritFromException()
    {
        // Arrange & Act
        var exception = new DomainException("Test");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithNullMessage_ShouldHandleGracefully()
    {
        // Arrange & Act
        var exception = new DomainException((string)null!);

        // Assert
        exception.Errors.Should().ContainSingle();
        exception.Errors[0].Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyMessage_ShouldCreateException()
    {
        // Arrange & Act
        var exception = new DomainException("");

        // Assert
        exception.Message.Should().Be("");
        exception.Errors.Should().ContainSingle();
        exception.Errors[0].Should().Be("");
    }

    [Fact]
    public void Constructor_WithWhitespaceMessage_ShouldPreserveWhitespace()
    {
        // Arrange
        var message = "   ";

        // Act
        var exception = new DomainException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.Errors[0].Should().Be(message);
    }

    [Fact]
    public void Constructor_WithLongErrorList_ShouldHandleAll()
    {
        // Arrange
        var errors = Enumerable.Range(1, 100).Select(i => $"Error {i}").ToArray();

        // Act
        var exception = new DomainException(errors);

        // Assert
        exception.Errors.Should().HaveCount(100);
        exception.Message.Should().Contain("Error 1");
        exception.Message.Should().Contain("Error 100");
    }

    #endregion
}
