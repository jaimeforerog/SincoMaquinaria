using FluentAssertions;
using SincoMaquinaria.DTOs.Common;
using SincoMaquinaria.Validators;
using Xunit;

namespace SincoMaquinaria.Tests.Validators;

public class PaginationTests
{
    #region PaginationRequestValidator Tests

    [Fact]
    public void PaginationRequestValidator_ConDatosValidos_DebeSerValido()
    {
        // Arrange
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 20,
            OrderBy = "nombre",
            OrderDirection = "asc"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PaginationRequestValidator_ConPageCero_DebeSerInvalido()
    {
        // Arrange
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest
        {
            Page = 0,
            PageSize = 20
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Page");
    }

    [Fact]
    public void PaginationRequestValidator_ConPageNegativo_DebeSerInvalido()
    {
        // Arrange
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest
        {
            Page = -1,
            PageSize = 20
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Page");
    }

    [Fact]
    public void PaginationRequestValidator_ConPageSizeCero_DebeSerInvalido()
    {
        // Arrange
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 0
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void PaginationRequestValidator_ConPageSizeNegativo_DebeSerInvalido()
    {
        // Arrange
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = -5
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void PaginationRequestValidator_ConPageSizeMayorA100_DebeSerInvalido()
    {
        // Arrange
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 101
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize" && e.ErrorMessage.Contains("100"));
    }

    [Fact]
    public void PaginationRequestValidator_ConPageSize100_DebeSerValido()
    {
        // Arrange
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 100,
            OrderDirection = "desc"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PaginationRequestValidator_ConOrderDirectionAsc_DebeSerValido()
    {
        // Arrange
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 20,
            OrderDirection = "asc"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PaginationRequestValidator_ConOrderDirectionDesc_DebeSerValido()
    {
        // Arrange
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 20,
            OrderDirection = "desc"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PaginationRequestValidator_ConOrderDirectionMayusculas_DebeSerValido()
    {
        // Arrange
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 20,
            OrderDirection = "DESC"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PaginationRequestValidator_ConOrderDirectionInvalido_DebeSerInvalido()
    {
        // Arrange
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 20,
            OrderDirection = "invalid"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OrderDirection");
    }

    [Fact]
    public void PaginationRequestValidator_ConSoloValoresRequeridos_DebeSerInvalido()
    {
        // Arrange - OrderDirection is required by validator
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest
        {
            Page = null,
            PageSize = null,
            OrderBy = "nombre",
            OrderDirection = null
        };

        // Act
        var result = validator.Validate(request);

        // Assert - Will fail because OrderDirection is required
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OrderDirection");
    }

    #endregion

    #region PaginationRequest DTO Tests

    [Fact]
    public void PaginationRequest_GetPage_ConValorNull_DebeRetornar1()
    {
        // Arrange
        var request = new PaginationRequest { Page = null };

        // Act
        var page = request.GetPage();

        // Assert
        page.Should().Be(1);
    }

    [Fact]
    public void PaginationRequest_GetPage_ConValor5_DebeRetornar5()
    {
        // Arrange
        var request = new PaginationRequest { Page = 5 };

        // Act
        var page = request.GetPage();

        // Assert
        page.Should().Be(5);
    }

    [Fact]
    public void PaginationRequest_GetPageSize_ConValorNull_DebeRetornar20()
    {
        // Arrange
        var request = new PaginationRequest { PageSize = null };

        // Act
        var pageSize = request.GetPageSize();

        // Assert
        pageSize.Should().Be(20);
    }

    [Fact]
    public void PaginationRequest_GetPageSize_ConValor50_DebeRetornar50()
    {
        // Arrange
        var request = new PaginationRequest { PageSize = 50 };

        // Act
        var pageSize = request.GetPageSize();

        // Assert
        pageSize.Should().Be(50);
    }

    [Fact]
    public void PaginationRequest_GetOffset_PrimeraPagina_DebeRetornar0()
    {
        // Arrange
        var request = new PaginationRequest { Page = 1, PageSize = 20 };

        // Act
        var offset = request.GetOffset();

        // Assert
        offset.Should().Be(0);
    }

    [Fact]
    public void PaginationRequest_GetOffset_SegundaPagina_DebeRetornar20()
    {
        // Arrange
        var request = new PaginationRequest { Page = 2, PageSize = 20 };

        // Act
        var offset = request.GetOffset();

        // Assert
        offset.Should().Be(20);
    }

    [Fact]
    public void PaginationRequest_GetOffset_TerceraPaginaTamano50_DebeRetornar100()
    {
        // Arrange
        var request = new PaginationRequest { Page = 3, PageSize = 50 };

        // Act
        var offset = request.GetOffset();

        // Assert
        offset.Should().Be(100);
    }

    [Fact]
    public void PaginationRequest_GetOffset_ConValoresNulos_DebeUsarDefaults()
    {
        // Arrange
        var request = new PaginationRequest { Page = null, PageSize = null };

        // Act
        var offset = request.GetOffset();

        // Assert
        // (1 - 1) * 20 = 0
        offset.Should().Be(0);
    }

    [Fact]
    public void PaginationRequest_IsDescending_ConDesc_DebeRetornarTrue()
    {
        // Arrange
        var request = new PaginationRequest { OrderDirection = "desc" };

        // Act
        var isDescending = request.IsDescending();

        // Assert
        isDescending.Should().BeTrue();
    }

    [Fact]
    public void PaginationRequest_IsDescending_ConDescMayusculas_DebeRetornarTrue()
    {
        // Arrange
        var request = new PaginationRequest { OrderDirection = "DESC" };

        // Act
        var isDescending = request.IsDescending();

        // Assert
        isDescending.Should().BeTrue();
    }

    [Fact]
    public void PaginationRequest_IsDescending_ConAsc_DebeRetornarFalse()
    {
        // Arrange
        var request = new PaginationRequest { OrderDirection = "asc" };

        // Act
        var isDescending = request.IsDescending();

        // Assert
        isDescending.Should().BeFalse();
    }

    [Fact]
    public void PaginationRequest_IsDescending_ConNull_DebeRetornarFalse()
    {
        // Arrange
        var request = new PaginationRequest { OrderDirection = null };

        // Act
        var isDescending = request.IsDescending();

        // Assert
        isDescending.Should().BeFalse();
    }

    [Fact]
    public void PaginationRequest_IsDescending_ConValorInvalido_DebeRetornarFalse()
    {
        // Arrange
        var request = new PaginationRequest { OrderDirection = "invalid" };

        // Act
        var isDescending = request.IsDescending();

        // Assert
        isDescending.Should().BeFalse();
    }

    #endregion
}
