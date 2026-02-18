using FluentAssertions;
using SincoMaquinaria.DTOs.Common;
using Xunit;

namespace SincoMaquinaria.Tests.DTOs;

public class PaginationRequestTests
{
    [Fact]
    public void GetPage_WhenPageIsNull_ShouldReturnDefaultValue()
    {
        // Arrange
        var request = new PaginationRequest { Page = null };

        // Act
        var result = request.GetPage();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void GetPage_WhenPageHasValue_ShouldReturnValue()
    {
        // Arrange
        var request = new PaginationRequest { Page = 5 };

        // Act
        var result = request.GetPage();

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void GetPageSize_WhenPageSizeIsNull_ShouldReturnDefaultValue()
    {
        // Arrange
        var request = new PaginationRequest { PageSize = null };

        // Act
        var result = request.GetPageSize();

        // Assert
        result.Should().Be(20);
    }

    [Fact]
    public void GetPageSize_WhenPageSizeHasValue_ShouldReturnValue()
    {
        // Arrange
        var request = new PaginationRequest { PageSize = 50 };

        // Act
        var result = request.GetPageSize();

        // Assert
        result.Should().Be(50);
    }

    [Theory]
    [InlineData(1, 10, 0)]
    [InlineData(2, 10, 10)]
    [InlineData(3, 20, 40)]
    [InlineData(5, 25, 100)]
    public void GetOffset_ShouldCalculateCorrectly(int page, int pageSize, int expectedOffset)
    {
        // Arrange
        var request = new PaginationRequest
        {
            Page = page,
            PageSize = pageSize
        };

        // Act
        var result = request.GetOffset();

        // Assert
        result.Should().Be(expectedOffset);
    }

    [Fact]
    public void GetOffset_WithNullValues_ShouldUseDefaults()
    {
        // Arrange
        var request = new PaginationRequest
        {
            Page = null,
            PageSize = null
        };

        // Act
        var result = request.GetOffset();

        // Assert
        // Default: (1 - 1) * 20 = 0
        result.Should().Be(0);
    }

    [Theory]
    [InlineData("desc", true)]
    [InlineData("DESC", true)]
    [InlineData("Desc", true)]
    [InlineData("asc", false)]
    [InlineData("ASC", false)]
    [InlineData("invalid", false)]
    [InlineData(null, false)]
    public void IsDescending_ShouldReturnCorrectValue(string? orderDirection, bool expected)
    {
        // Arrange
        var request = new PaginationRequest
        {
            OrderDirection = orderDirection
        };

        // Act
        var result = request.IsDescending();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange & Act
        var request = new PaginationRequest
        {
            Page = 3,
            PageSize = 15,
            OrderBy = "nombre",
            OrderDirection = "asc"
        };

        // Assert
        request.Page.Should().Be(3);
        request.PageSize.Should().Be(15);
        request.OrderBy.Should().Be("nombre");
        request.OrderDirection.Should().Be("asc");
    }

    [Fact]
    public void Page_ShouldSupportNullValue()
    {
        // Arrange & Act
        var request = new PaginationRequest();

        // Assert
        request.Page.Should().BeNull();
    }

    [Fact]
    public void PageSize_ShouldSupportNullValue()
    {
        // Arrange & Act
        var request = new PaginationRequest();

        // Assert
        request.PageSize.Should().BeNull();
    }

    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(10, 100, 900)]
    public void GetOffset_ForFirstPage_ShouldBeZero(int page, int pageSize, int expectedOffset)
    {
        // Arrange
        var request = new PaginationRequest
        {
            Page = page,
            PageSize = pageSize
        };

        // Act
        var offset = request.GetOffset();

        // Assert
        offset.Should().Be(expectedOffset);
    }

    [Fact]
    public void OrderBy_CanBeNull()
    {
        // Arrange & Act
        var request = new PaginationRequest
        {
            OrderBy = null
        };

        // Assert
        request.OrderBy.Should().BeNull();
    }

    [Fact]
    public void OrderDirection_CanBeNull()
    {
        // Arrange & Act
        var request = new PaginationRequest
        {
            OrderDirection = null
        };

        // Assert
        request.OrderDirection.Should().BeNull();
    }

    [Fact]
    public void GetPage_AfterSettingPageToZero_ShouldReturnZero()
    {
        // Arrange
        var request = new PaginationRequest { Page = 0 };

        // Act
        var result = request.GetPage();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Instance_WithAllPropertiesSet_ShouldRetainAllValues()
    {
        // Arrange & Act
        var request = new PaginationRequest
        {
            Page = 5,
            PageSize = 30,
            OrderBy = "fecha",
            OrderDirection = "desc"
        };

        // Assert
        request.GetPage().Should().Be(5);
        request.GetPageSize().Should().Be(30);
        request.OrderBy.Should().Be("fecha");
        request.IsDescending().Should().BeTrue();
    }
}
