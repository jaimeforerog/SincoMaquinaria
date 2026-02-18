using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using SincoMaquinaria.Services;
using SincoMaquinaria.Services.Jobs;
using System.Linq.Expressions;
using Xunit;

namespace SincoMaquinaria.Tests.Services;

public class BackgroundJobServiceTests : IDisposable
{
    private readonly Mock<IBackgroundJobClient> _mockJobClient;
    private readonly Mock<IRecurringJobManager> _mockRecurringJobManager;
    private readonly BackgroundJobService _service;
    private readonly List<string> _tempFilesToCleanup;

    public BackgroundJobServiceTests()
    {
        _mockJobClient = new Mock<IBackgroundJobClient>();
        _mockRecurringJobManager = new Mock<IRecurringJobManager>();
        _service = new BackgroundJobService(_mockJobClient.Object, _mockRecurringJobManager.Object);
        _tempFilesToCleanup = new List<string>();
    }

    public void Dispose()
    {
        // Cleanup temporary files and directories created during tests
        foreach (var filePath in _tempFilesToCleanup)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                var directory = Path.GetDirectoryName(filePath);
                if (directory != null && Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    #region EnqueueImportarEquipos Tests

    [Fact]
    public void EnqueueImportarEquipos_WithValidStream_ShouldReturnJobId()
    {
        // Arrange
        var fileContent = "Test file content for equipos";
        var fileName = "equipos.xlsx";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));

        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Returns("job-123");

        // Act
        var jobId = _service.EnqueueImportarEquipos(stream, fileName);

        // Assert
        jobId.Should().Be("job-123");
        _mockJobClient.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Once);
    }

    [Fact]
    public void EnqueueImportarEquipos_WithEmptyStream_ShouldStillCreateJob()
    {
        // Arrange
        var fileName = "empty-equipos.xlsx";
        using var stream = new MemoryStream();

        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Returns("job-empty-123");

        // Act
        var jobId = _service.EnqueueImportarEquipos(stream, fileName);

        // Assert
        jobId.Should().Be("job-empty-123");
        _mockJobClient.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Once);
    }

    [Fact]
    public void EnqueueImportarEquipos_ShouldCallImportacionJobHandler()
    {
        // Arrange
        var fileName = "test-equipos.xlsx";
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        Job? capturedJob = null;
        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Callback<Job, IState>((job, state) =>
            {
                capturedJob = job;
                if (job.Args.Count > 0 && job.Args[0] is string path)
                {
                    _tempFilesToCleanup.Add(path);
                }
            })
            .Returns("job-capture-123");

        // Act
        var jobId = _service.EnqueueImportarEquipos(stream, fileName);

        // Assert
        capturedJob.Should().NotBeNull();
        capturedJob!.Type.Should().Be(typeof(ImportacionJobHandler));
        capturedJob.Method.Name.Should().Be("ImportarEquiposAsync");
    }

    [Fact]
    public void EnqueueImportarEquipos_ShouldSaveFileWithCorrectName()
    {
        // Arrange
        var fileName = "specific-equipos.xlsx";
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        string? savedPath = null;
        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Callback<Job, IState>((job, state) =>
            {
                if (job.Args.Count > 0 && job.Args[0] is string path)
                {
                    savedPath = path;
                    _tempFilesToCleanup.Add(path);
                }
            })
            .Returns("job-filename-123");

        // Act
        _service.EnqueueImportarEquipos(stream, fileName);

        // Assert
        savedPath.Should().NotBeNullOrEmpty();
        Path.GetFileName(savedPath).Should().Be(fileName);
    }

    [Fact]
    public void EnqueueImportarEquipos_ShouldCreateUniqueDirectoryForEachCall()
    {
        // Arrange
        var fileName = "equipos.xlsx";
        using var stream1 = new MemoryStream(new byte[] { 1, 2, 3 });
        using var stream2 = new MemoryStream(new byte[] { 4, 5, 6 });

        var capturedPaths = new List<string>();
        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Callback<Job, IState>((job, state) =>
            {
                if (job.Args.Count > 0 && job.Args[0] is string path)
                {
                    capturedPaths.Add(path);
                    _tempFilesToCleanup.Add(path);
                }
            })
            .Returns("job-unique-123");

        // Act
        _service.EnqueueImportarEquipos(stream1, fileName);
        _service.EnqueueImportarEquipos(stream2, fileName);

        // Assert
        capturedPaths.Should().HaveCount(2);
        var dir1 = Path.GetDirectoryName(capturedPaths[0]);
        var dir2 = Path.GetDirectoryName(capturedPaths[1]);
        dir1.Should().NotBe(dir2, "Each call should create a unique directory");
    }

    [Fact]
    public void EnqueueImportarEquipos_WithLargeFile_ShouldSaveCompleteContent()
    {
        // Arrange
        var largeContent = new byte[1024 * 100]; // 100 KB
        new Random().NextBytes(largeContent);
        var fileName = "large-equipos.xlsx";
        using var stream = new MemoryStream(largeContent);

        string? savedFilePath = null;
        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Callback<Job, IState>((job, state) =>
            {
                if (job.Args.Count > 0 && job.Args[0] is string path)
                {
                    savedFilePath = path;
                    _tempFilesToCleanup.Add(path);
                }
            })
            .Returns("job-large-123");

        // Act
        _service.EnqueueImportarEquipos(stream, fileName);

        // Assert
        savedFilePath.Should().NotBeNullOrEmpty();
        if (savedFilePath != null && File.Exists(savedFilePath))
        {
            var savedContent = File.ReadAllBytes(savedFilePath);
            savedContent.Should().Equal(largeContent);
        }
    }

    #endregion

    #region EnqueueImportarEmpleados Tests

    [Fact]
    public void EnqueueImportarEmpleados_WithValidStream_ShouldReturnJobId()
    {
        // Arrange
        var fileContent = "Test file content for empleados";
        var fileName = "empleados.xlsx";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));

        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Returns("job-emp-123");

        // Act
        var jobId = _service.EnqueueImportarEmpleados(stream, fileName);

        // Assert
        jobId.Should().Be("job-emp-123");
        _mockJobClient.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Once);
    }

    [Fact]
    public void EnqueueImportarEmpleados_WithEmptyStream_ShouldStillCreateJob()
    {
        // Arrange
        var fileName = "empty-empleados.xlsx";
        using var stream = new MemoryStream();

        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Returns("job-emp-empty-123");

        // Act
        var jobId = _service.EnqueueImportarEmpleados(stream, fileName);

        // Assert
        jobId.Should().Be("job-emp-empty-123");
        _mockJobClient.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Once);
    }

    [Fact]
    public void EnqueueImportarEmpleados_ShouldCallImportacionJobHandler()
    {
        // Arrange
        var fileName = "test-empleados.xlsx";
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        Job? capturedJob = null;
        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Callback<Job, IState>((job, state) =>
            {
                capturedJob = job;
                if (job.Args.Count > 0 && job.Args[0] is string path)
                {
                    _tempFilesToCleanup.Add(path);
                }
            })
            .Returns("job-emp-capture-123");

        // Act
        var jobId = _service.EnqueueImportarEmpleados(stream, fileName);

        // Assert
        capturedJob.Should().NotBeNull();
        capturedJob!.Type.Should().Be(typeof(ImportacionJobHandler));
        capturedJob.Method.Name.Should().Be("ImportarEmpleadosAsync");
    }

    [Fact]
    public void EnqueueImportarEmpleados_ShouldSaveFileWithCorrectName()
    {
        // Arrange
        var fileName = "specific-empleados.xlsx";
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        string? savedPath = null;
        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Callback<Job, IState>((job, state) =>
            {
                if (job.Args.Count > 0 && job.Args[0] is string path)
                {
                    savedPath = path;
                    _tempFilesToCleanup.Add(path);
                }
            })
            .Returns("job-emp-filename-123");

        // Act
        _service.EnqueueImportarEmpleados(stream, fileName);

        // Assert
        savedPath.Should().NotBeNullOrEmpty();
        Path.GetFileName(savedPath).Should().Be(fileName);
    }

    [Fact]
    public void EnqueueImportarEmpleados_ShouldCreateUniqueDirectoryForEachCall()
    {
        // Arrange
        var fileName = "empleados.xlsx";
        using var stream1 = new MemoryStream(new byte[] { 1, 2, 3 });
        using var stream2 = new MemoryStream(new byte[] { 4, 5, 6 });

        var capturedPaths = new List<string>();
        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Callback<Job, IState>((job, state) =>
            {
                if (job.Args.Count > 0 && job.Args[0] is string path)
                {
                    capturedPaths.Add(path);
                    _tempFilesToCleanup.Add(path);
                }
            })
            .Returns("job-emp-unique-123");

        // Act
        _service.EnqueueImportarEmpleados(stream1, fileName);
        _service.EnqueueImportarEmpleados(stream2, fileName);

        // Assert
        capturedPaths.Should().HaveCount(2);
        var dir1 = Path.GetDirectoryName(capturedPaths[0]);
        var dir2 = Path.GetDirectoryName(capturedPaths[1]);
        dir1.Should().NotBe(dir2, "Each call should create a unique directory");
    }

    [Fact]
    public void EnqueueImportarEmpleados_WithLargeFile_ShouldSaveCompleteContent()
    {
        // Arrange
        var largeContent = new byte[1024 * 100]; // 100 KB
        new Random().NextBytes(largeContent);
        var fileName = "large-empleados.xlsx";
        using var stream = new MemoryStream(largeContent);

        string? savedFilePath = null;
        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Callback<Job, IState>((job, state) =>
            {
                if (job.Args.Count > 0 && job.Args[0] is string path)
                {
                    savedFilePath = path;
                    _tempFilesToCleanup.Add(path);
                }
            })
            .Returns("job-emp-large-123");

        // Act
        _service.EnqueueImportarEmpleados(stream, fileName);

        // Assert
        savedFilePath.Should().NotBeNullOrEmpty();
        if (savedFilePath != null && File.Exists(savedFilePath))
        {
            var savedContent = File.ReadAllBytes(savedFilePath);
            savedContent.Should().Equal(largeContent);
        }
    }

    #endregion

    #region ScheduleLimpiezaTokensExpirados Tests

    [Fact]
    public void ScheduleLimpiezaTokensExpirados_ShouldScheduleRecurringJob()
    {
        // Act
        _service.ScheduleLimpiezaTokensExpirados();

        // Assert
        _mockRecurringJobManager.Verify(
            x => x.AddOrUpdate(
                "limpieza-tokens-expirados",
                It.IsAny<Job>(),
                It.IsAny<string>(),
                It.IsAny<RecurringJobOptions>()),
            Times.Once);
    }

    [Fact]
    public void ScheduleLimpiezaTokensExpirados_ShouldUseCorrectCronExpression()
    {
        // Arrange
        string? capturedCron = null;
        _mockRecurringJobManager
            .Setup(x => x.AddOrUpdate(
                It.IsAny<string>(),
                It.IsAny<Job>(),
                It.IsAny<string>(),
                It.IsAny<RecurringJobOptions>()))
            .Callback<string, Job, string, RecurringJobOptions>((id, job, cron, opts) =>
            {
                capturedCron = cron;
            });

        // Act
        _service.ScheduleLimpiezaTokensExpirados();

        // Assert
        capturedCron.Should().Be("0 3 * * *", "Job should run at 3 AM daily");
    }

    [Fact]
    public void ScheduleLimpiezaTokensExpirados_ShouldUseCorrectJobId()
    {
        // Arrange
        string? capturedJobId = null;
        _mockRecurringJobManager
            .Setup(x => x.AddOrUpdate(
                It.IsAny<string>(),
                It.IsAny<Job>(),
                It.IsAny<string>(),
                It.IsAny<RecurringJobOptions>()))
            .Callback<string, Job, string, RecurringJobOptions>((id, job, cron, opts) =>
            {
                capturedJobId = id;
            });

        // Act
        _service.ScheduleLimpiezaTokensExpirados();

        // Assert
        capturedJobId.Should().Be("limpieza-tokens-expirados");
    }

    [Fact]
    public void ScheduleLimpiezaTokensExpirados_ShouldUseLocalTimeZone()
    {
        // Arrange
        TimeZoneInfo? capturedTimeZone = null;
        _mockRecurringJobManager
            .Setup(x => x.AddOrUpdate(
                It.IsAny<string>(),
                It.IsAny<Job>(),
                It.IsAny<string>(),
                It.IsAny<RecurringJobOptions>()))
            .Callback<string, Job, string, RecurringJobOptions>((id, job, cron, opts) =>
            {
                capturedTimeZone = opts.TimeZone;
            });

        // Act
        _service.ScheduleLimpiezaTokensExpirados();

        // Assert
        capturedTimeZone.Should().Be(TimeZoneInfo.Local);
    }

    [Fact]
    public void ScheduleLimpiezaTokensExpirados_ShouldCallMantenimientoJobHandler()
    {
        // Arrange
        Job? capturedJob = null;
        _mockRecurringJobManager
            .Setup(x => x.AddOrUpdate(
                It.IsAny<string>(),
                It.IsAny<Job>(),
                It.IsAny<string>(),
                It.IsAny<RecurringJobOptions>()))
            .Callback<string, Job, string, RecurringJobOptions>((id, job, cron, opts) =>
            {
                capturedJob = job;
            });

        // Act
        _service.ScheduleLimpiezaTokensExpirados();

        // Assert
        capturedJob.Should().NotBeNull();
        capturedJob!.Type.Should().Be(typeof(MantenimientoJobHandler));
        capturedJob.Method.Name.Should().Be("LimpiarTokensExpiradosAsync");
    }

    [Fact]
    public void ScheduleLimpiezaTokensExpirados_CalledMultipleTimes_ShouldUpdateExistingJob()
    {
        // Act
        _service.ScheduleLimpiezaTokensExpirados();
        _service.ScheduleLimpiezaTokensExpirados();
        _service.ScheduleLimpiezaTokensExpirados();

        // Assert
        // AddOrUpdate should be called multiple times (it updates if exists)
        _mockRecurringJobManager.Verify(
            x => x.AddOrUpdate(
                "limpieza-tokens-expirados",
                It.IsAny<Job>(),
                It.IsAny<string>(),
                It.IsAny<RecurringJobOptions>()),
            Times.Exactly(3));
    }

    #endregion
}
