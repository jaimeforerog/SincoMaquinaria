using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SincoMaquinaria.Extensions;
using Xunit;

namespace SincoMaquinaria.Tests.Extensions;

public class HttpContextExtensionsTests
{
    [Fact]
    public void GetUserContext_WithValidSubAndName_ShouldReturnUserIdAndName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.Name, userName)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        // Act
        var (resultUserId, resultUserName) = context.GetUserContext();

        // Assert
        resultUserId.Should().Be(userId);
        resultUserName.Should().Be(userName);
    }

    [Fact]
    public void GetUserContext_WithNameIdentifierClaim_ShouldReturnUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, userName)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        // Act
        var (resultUserId, resultUserName) = context.GetUserContext();

        // Assert
        resultUserId.Should().Be(userId);
        resultUserName.Should().Be(userName);
    }

    [Fact]
    public void GetUserContext_WithLowercaseNameClaim_ShouldReturnUserName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("name", userName)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        // Act
        var (resultUserId, resultUserName) = context.GetUserContext();

        // Assert
        resultUserId.Should().Be(userId);
        resultUserName.Should().Be(userName);
    }

    [Fact]
    public void GetUserContext_WithNoClaims_ShouldReturnNulls()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        // Act
        var (resultUserId, resultUserName) = context.GetUserContext();

        // Assert
        resultUserId.Should().BeNull();
        resultUserName.Should().BeNull();
    }

    [Fact]
    public void GetUserContext_WithInvalidGuidFormat_ShouldReturnNullUserId()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid"),
            new Claim(ClaimTypes.Name, "Test User")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        // Act
        var (resultUserId, resultUserName) = context.GetUserContext();

        // Assert
        resultUserId.Should().BeNull();
        resultUserName.Should().Be("Test User");
    }

    [Fact]
    public void ValidateFileUpload_WithNullFile_ShouldReturnBadRequest()
    {
        // Act
        var result = HttpContextExtensions.ValidateFileUpload(null, 10);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result as Microsoft.AspNetCore.Http.IResult;
        badRequestResult.Should().NotBeNull();
    }

    [Fact]
    public void ValidateFileUpload_WithEmptyFile_ShouldReturnBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);
        fileMock.Setup(f => f.FileName).Returns("test.xlsx");

        // Act
        var result = HttpContextExtensions.ValidateFileUpload(fileMock.Object, 10);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ValidateFileUpload_WithFileTooLarge_ShouldReturnBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var maxSizeMB = 10;
        var fileSizeInBytes = (long)(maxSizeMB + 1) * 1024 * 1024; // 11 MB
        fileMock.Setup(f => f.Length).Returns(fileSizeInBytes);
        fileMock.Setup(f => f.FileName).Returns("test.xlsx");

        // Act
        var result = HttpContextExtensions.ValidateFileUpload(fileMock.Object, maxSizeMB);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ValidateFileUpload_WithInvalidExtension_ShouldReturnBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("test.pdf");

        // Act
        var result = HttpContextExtensions.ValidateFileUpload(fileMock.Object, 10);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ValidateFileUpload_WithValidXlsxFile_ShouldReturnNull()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("test.xlsx");

        // Act
        var result = HttpContextExtensions.ValidateFileUpload(fileMock.Object, 10);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateFileUpload_WithValidXlsFile_ShouldReturnNull()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("test.xls");

        // Act
        var result = HttpContextExtensions.ValidateFileUpload(fileMock.Object, 10);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateFileUpload_WithUppercaseExtension_ShouldAccept()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("test.XLSX");

        // Act
        var result = HttpContextExtensions.ValidateFileUpload(fileMock.Object, 10);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateFileUpload_WithMixedCaseExtension_ShouldAccept()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("test.XlSx");

        // Act
        var result = HttpContextExtensions.ValidateFileUpload(fileMock.Object, 10);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateFileUpload_WithExactMaxSize_ShouldAccept()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var maxSizeMB = 10;
        var exactMaxSize = (long)maxSizeMB * 1024 * 1024;
        fileMock.Setup(f => f.Length).Returns(exactMaxSize);
        fileMock.Setup(f => f.FileName).Returns("test.xlsx");

        // Act
        var result = HttpContextExtensions.ValidateFileUpload(fileMock.Object, maxSizeMB);

        // Assert
        result.Should().BeNull();
    }
}
