using FluentAssertions;
using SincoMaquinaria.DTOs.Common;
using SincoMaquinaria.Extensions;
using Xunit;

namespace SincoMaquinaria.Tests.Extensions;

public class PaginationExtensionsTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int Value { get; set; }
    }

    private List<TestEntity> GetTestData()
    {
        return new List<TestEntity>
        {
            new() { Id = 1, Name = "Alpha", CreatedAt = DateTime.Now.AddDays(-5), Value = 100 },
            new() { Id = 2, Name = "Beta", CreatedAt = DateTime.Now.AddDays(-4), Value = 200 },
            new() { Id = 3, Name = "Gamma", CreatedAt = DateTime.Now.AddDays(-3), Value = 150 },
            new() { Id = 4, Name = "Delta", CreatedAt = DateTime.Now.AddDays(-2), Value = 300 },
            new() { Id = 5, Name = "Epsilon", CreatedAt = DateTime.Now.AddDays(-1), Value = 250 }
        };
    }

    [Fact]
    public void ApplyOrdering_WithNullOrderBy_ShouldReturnOriginalQuery()
    {
        // Arrange
        var data = GetTestData();
        var query = data.AsQueryable();
        var pagination = new PaginationRequest { OrderBy = null };

        // Act
        var result = query.ApplyOrdering(pagination);

        // Assert
        result.Should().BeEquivalentTo(query);
    }

    [Fact]
    public void ApplyOrdering_WithEmptyOrderBy_ShouldReturnOriginalQuery()
    {
        // Arrange
        var data = GetTestData();
        var query = data.AsQueryable();
        var pagination = new PaginationRequest { OrderBy = "" };

        // Act
        var result = query.ApplyOrdering(pagination);

        // Assert
        result.Should().BeEquivalentTo(query);
    }

    [Fact]
    public void ApplyOrdering_WithWhitespaceOrderBy_ShouldReturnOriginalQuery()
    {
        // Arrange
        var data = GetTestData();
        var query = data.AsQueryable();
        var pagination = new PaginationRequest { OrderBy = "   " };

        // Act
        var result = query.ApplyOrdering(pagination);

        // Assert
        result.Should().BeEquivalentTo(query);
    }

    [Fact]
    public void ApplyOrdering_WithInvalidProperty_ShouldReturnOriginalQuery()
    {
        // Arrange
        var data = GetTestData();
        var query = data.AsQueryable();
        var pagination = new PaginationRequest { OrderBy = "NonExistentProperty" };

        // Act
        var result = query.ApplyOrdering(pagination);

        // Assert
        result.Should().BeEquivalentTo(query);
    }

    [Fact]
    public void ApplyOrdering_WithValidPropertyAscending_ShouldOrderAscending()
    {
        // Arrange
        var data = GetTestData();
        var query = data.AsQueryable();
        var pagination = new PaginationRequest { OrderBy = "Name", OrderDirection = "asc" };

        // Act
        var result = query.ApplyOrdering(pagination).ToList();

        // Assert
        result.Should().HaveCount(5);
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Beta");
        result[2].Name.Should().Be("Delta");
        result[3].Name.Should().Be("Epsilon");
        result[4].Name.Should().Be("Gamma");
    }

    [Fact]
    public void ApplyOrdering_WithValidPropertyDescending_ShouldOrderDescending()
    {
        // Arrange
        var data = GetTestData();
        var query = data.AsQueryable();
        var pagination = new PaginationRequest { OrderBy = "Name", OrderDirection = "desc" };

        // Act
        var result = query.ApplyOrdering(pagination).ToList();

        // Assert
        result.Should().HaveCount(5);
        result[0].Name.Should().Be("Gamma");
        result[1].Name.Should().Be("Epsilon");
        result[2].Name.Should().Be("Delta");
        result[3].Name.Should().Be("Beta");
        result[4].Name.Should().Be("Alpha");
    }

    [Fact]
    public void ApplyOrdering_WithIntPropertyAscending_ShouldOrderByInt()
    {
        // Arrange
        var data = GetTestData();
        var query = data.AsQueryable();
        var pagination = new PaginationRequest { OrderBy = "Value", OrderDirection = "asc" };

        // Act
        var result = query.ApplyOrdering(pagination).ToList();

        // Assert
        result.Should().HaveCount(5);
        result[0].Value.Should().Be(100);
        result[1].Value.Should().Be(150);
        result[2].Value.Should().Be(200);
        result[3].Value.Should().Be(250);
        result[4].Value.Should().Be(300);
    }

    [Fact]
    public void ApplyOrdering_WithIntPropertyDescending_ShouldOrderByIntDescending()
    {
        // Arrange
        var data = GetTestData();
        var query = data.AsQueryable();
        var pagination = new PaginationRequest { OrderBy = "Value", OrderDirection = "desc" };

        // Act
        var result = query.ApplyOrdering(pagination).ToList();

        // Assert
        result.Should().HaveCount(5);
        result[0].Value.Should().Be(300);
        result[1].Value.Should().Be(250);
        result[2].Value.Should().Be(200);
        result[3].Value.Should().Be(150);
        result[4].Value.Should().Be(100);
    }

    [Fact]
    public void ApplyOrdering_WithDateTimeProperty_ShouldOrderByDateTime()
    {
        // Arrange
        var data = GetTestData();
        var query = data.AsQueryable();
        var pagination = new PaginationRequest { OrderBy = "CreatedAt", OrderDirection = "asc" };

        // Act
        var result = query.ApplyOrdering(pagination).ToList();

        // Assert
        result.Should().HaveCount(5);
        result[0].Id.Should().Be(1); // Oldest
        result[4].Id.Should().Be(5); // Newest
    }

    [Fact]
    public void ApplyOrdering_WithDefaultOrderDirection_ShouldOrderAscending()
    {
        // Arrange
        var data = GetTestData();
        var query = data.AsQueryable();
        var pagination = new PaginationRequest { OrderBy = "Id" };

        // Act
        var result = query.ApplyOrdering(pagination).ToList();

        // Assert
        result.Should().HaveCount(5);
        result[0].Id.Should().Be(1);
        result[4].Id.Should().Be(5);
    }

    [Fact]
    public void ApplyOrdering_WithCaseSensitivePropertyName_ShouldMatch()
    {
        // Arrange
        var data = GetTestData();
        var query = data.AsQueryable();
        var pagination = new PaginationRequest { OrderBy = "Id", OrderDirection = "asc" };

        // Act
        var result = query.ApplyOrdering(pagination).ToList();

        // Assert
        result.Should().HaveCount(5);
        result[0].Id.Should().Be(1);
    }

    [Fact]
    public void ApplyOrdering_WithWrongCasePropertyName_ShouldNotMatch()
    {
        // Arrange
        var data = GetTestData();
        var query = data.AsQueryable();
        var pagination = new PaginationRequest { OrderBy = "id" }; // lowercase

        // Act
        var result = query.ApplyOrdering(pagination);

        // Assert - Should return original query since property doesn't match
        result.Should().BeEquivalentTo(query);
    }
}
