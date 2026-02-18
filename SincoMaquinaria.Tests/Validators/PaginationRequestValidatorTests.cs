using FluentValidation.TestHelper;
using SincoMaquinaria.DTOs.Common;
using SincoMaquinaria.Validators;
using Xunit;

namespace SincoMaquinaria.Tests.Validators;

public class PaginationRequestValidatorTests
{
    private readonly PaginationRequestValidator _validator;

    public PaginationRequestValidatorTests()
    {
        _validator = new PaginationRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 10,
            OrderBy = "nombre",
            OrderDirection = "asc"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1, 50, "desc")]
    [InlineData(5, 100, "ASC")]
    [InlineData(10, 1, "DESC")]
    [InlineData(100, 25, "Asc")]
    public void Validate_WithVariousValidInputs_ShouldPass(int page, int pageSize, string orderDirection)
    {
        // Arrange
        var request = new PaginationRequest
        {
            Page = page,
            PageSize = pageSize,
            OrderBy = "nombre",
            OrderDirection = orderDirection
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithInvalidPage_ShouldHaveValidationError(int page)
    {
        // Arrange
        var request = new PaginationRequest
        {
            Page = page,
            PageSize = 10,
            OrderBy = "nombre",
            OrderDirection = "asc"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage("El número de página debe ser mayor a 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public void Validate_WithInvalidPageSize_ShouldHaveValidationError(int pageSize)
    {
        // Arrange
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = pageSize,
            OrderBy = "nombre",
            OrderDirection = "asc"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("El tamaño de página debe ser mayor a 0");
    }

    [Theory]
    [InlineData(101)]
    [InlineData(200)]
    [InlineData(1000)]
    public void Validate_WithPageSizeExceedingMaximum_ShouldHaveValidationError(int pageSize)
    {
        // Arrange
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = pageSize,
            OrderBy = "nombre",
            OrderDirection = "asc"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("El tamaño de página no puede exceder 100 elementos");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("ascending")]
    [InlineData("descending")]
    [InlineData("")]
    [InlineData("up")]
    [InlineData("down")]
    public void Validate_WithInvalidOrderDirection_ShouldHaveValidationError(string orderDirection)
    {
        // Arrange
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 10,
            OrderBy = "nombre",
            OrderDirection = orderDirection
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderDirection)
            .WithErrorMessage("La dirección de ordenamiento debe ser 'asc' o 'desc'");
    }

    [Fact]
    public void Validate_WithNullOrderDirection_ShouldHaveValidationError()
    {
        // Arrange
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 10,
            OrderBy = "nombre",
            OrderDirection = null
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderDirection);
    }

    [Fact]
    public void Validate_WithBoundaryValues_ShouldPass()
    {
        // Arrange - Minimum valid values
        var request1 = new PaginationRequest
        {
            Page = 1,
            PageSize = 1,
            OrderBy = "nombre",
            OrderDirection = "asc"
        };

        // Maximum valid page size
        var request2 = new PaginationRequest
        {
            Page = 1,
            PageSize = 100,
            OrderBy = "nombre",
            OrderDirection = "desc"
        };

        // Act
        var result1 = _validator.TestValidate(request1);
        var result2 = _validator.TestValidate(request2);

        // Assert
        result1.ShouldNotHaveAnyValidationErrors();
        result2.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var request = new PaginationRequest
        {
            Page = 0,
            PageSize = 101,
            OrderBy = "nombre",
            OrderDirection = "invalid"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Page);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
        result.ShouldHaveValidationErrorFor(x => x.OrderDirection);
    }
}
