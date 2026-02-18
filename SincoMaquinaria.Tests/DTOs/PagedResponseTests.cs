using FluentAssertions;
using SincoMaquinaria.DTOs.Common;
using Xunit;

namespace SincoMaquinaria.Tests.DTOs;

public class PagedResponseTests
{
    [Fact]
    public void PagedResponse_ConstructorPorDefecto_DebeCrearObjetoVacio()
    {
        // Act
        var response = new PagedResponse<string>();

        // Assert
        response.Data.Should().BeEmpty();
        response.Page.Should().Be(0);
        response.PageSize.Should().Be(0);
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    public void PagedResponse_ConstructorConDatos_DebeEstablecerPropiedades()
    {
        // Arrange
        var data = new List<string> { "Item1", "Item2", "Item3" };

        // Act
        var response = new PagedResponse<string>(data, 1, 10, 50);

        // Assert
        response.Data.Should().BeEquivalentTo(data);
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.TotalCount.Should().Be(50);
    }

    [Fact]
    public void PagedResponse_TotalPages_DebeCalcularCorrectamente()
    {
        // Arrange
        var data = new List<int> { 1, 2, 3 };
        var response = new PagedResponse<int>(data, 1, 10, 25);

        // Act
        var totalPages = response.TotalPages;

        // Assert
        // 25 items / 10 per page = 3 pages (rounded up)
        totalPages.Should().Be(3);
    }

    [Fact]
    public void PagedResponse_TotalPages_ConDivisonExacta_DebeCalcularCorrectamente()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            PageSize = 10,
            TotalCount = 100
        };

        // Act
        var totalPages = response.TotalPages;

        // Assert
        totalPages.Should().Be(10);
    }

    [Fact]
    public void PagedResponse_TotalPages_ConUnSoloElemento_DebeRetornar1()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            PageSize = 10,
            TotalCount = 1
        };

        // Act
        var totalPages = response.TotalPages;

        // Assert
        totalPages.Should().Be(1);
    }

    [Fact]
    public void PagedResponse_HasPrevious_EnPrimeraPagina_DebeRetornarFalse()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            Page = 1,
            PageSize = 10,
            TotalCount = 50
        };

        // Act
        var hasPrevious = response.HasPrevious;

        // Assert
        hasPrevious.Should().BeFalse();
    }

    [Fact]
    public void PagedResponse_HasPrevious_EnSegundaPagina_DebeRetornarTrue()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            Page = 2,
            PageSize = 10,
            TotalCount = 50
        };

        // Act
        var hasPrevious = response.HasPrevious;

        // Assert
        hasPrevious.Should().BeTrue();
    }

    [Fact]
    public void PagedResponse_HasNext_EnUltimaPagina_DebeRetornarFalse()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            Page = 5,
            PageSize = 10,
            TotalCount = 50
        };

        // Act
        var hasNext = response.HasNext;

        // Assert
        hasNext.Should().BeFalse();
    }

    [Fact]
    public void PagedResponse_HasNext_EnPrimeraPagina_DebeRetornarTrue()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            Page = 1,
            PageSize = 10,
            TotalCount = 50
        };

        // Act
        var hasNext = response.HasNext;

        // Assert
        hasNext.Should().BeTrue();
    }

    [Fact]
    public void PagedResponse_HasNext_EnPaginaIntermedia_DebeRetornarTrue()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            Page = 3,
            PageSize = 10,
            TotalCount = 50
        };

        // Act
        var hasNext = response.HasNext;

        // Assert
        hasNext.Should().BeTrue();
    }

    [Fact]
    public void PagedResponse_Create_ConPaginationRequest_DebeCrearCorrectamente()
    {
        // Arrange
        var data = new List<string> { "A", "B", "C" };
        var paginationRequest = new PaginationRequest
        {
            Page = 2,
            PageSize = 20,
            OrderBy = "name",
            OrderDirection = "asc"
        };

        // Act
        var response = PagedResponse<string>.Create(data, paginationRequest, 100);

        // Assert
        response.Data.Should().BeEquivalentTo(data);
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(20);
        response.TotalCount.Should().Be(100);
        response.TotalPages.Should().Be(5);
    }

    [Fact]
    public void PagedResponse_Create_ConPaginationRequestNulls_DebeUsarDefaults()
    {
        // Arrange
        var data = new List<int> { 10, 20, 30 };
        var paginationRequest = new PaginationRequest
        {
            Page = null,
            PageSize = null
        };

        // Act
        var response = PagedResponse<int>.Create(data, paginationRequest, 60);

        // Assert
        response.Data.Should().BeEquivalentTo(data);
        response.Page.Should().Be(1); // Default from GetPage()
        response.PageSize.Should().Be(20); // Default from GetPageSize()
        response.TotalCount.Should().Be(60);
        response.TotalPages.Should().Be(3);
    }

    [Fact]
    public void PagedResponse_PropiedadesModificables_DebenPermitirAsignacion()
    {
        // Arrange
        var response = new PagedResponse<string>();
        var newData = new List<string> { "New1", "New2" };

        // Act
        response.Data = newData;
        response.Page = 5;
        response.PageSize = 25;
        response.TotalCount = 200;

        // Assert
        response.Data.Should().BeEquivalentTo(newData);
        response.Page.Should().Be(5);
        response.PageSize.Should().Be(25);
        response.TotalCount.Should().Be(200);
        response.TotalPages.Should().Be(8);
        response.HasPrevious.Should().BeTrue();
        response.HasNext.Should().BeTrue();
    }
}
