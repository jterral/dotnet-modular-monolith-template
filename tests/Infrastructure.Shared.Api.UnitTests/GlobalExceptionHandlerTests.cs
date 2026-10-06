using Infrastructure.Shared.Api.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Infrastructure.Shared.Api.UnitTests;

public sealed class GlobalExceptionHandlerTests : IDisposable
{
    private readonly Mock<IProblemDetailsService> _problemDetailsServiceMock = new();
    private readonly GlobalExceptionHandler _sut;

    public GlobalExceptionHandlerTests()
    {
        _sut = new GlobalExceptionHandler(_problemDetailsServiceMock.Object, NullLogger<GlobalExceptionHandler>.Instance);
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_WritesA500ProblemWithoutTheExceptionMessage()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        ProblemDetailsContext? written = null;
        _problemDetailsServiceMock
            .Setup(x => x.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(context => written = context)
            .Returns(ValueTask.FromResult(true));

        // Act
        bool handled = await _sut.TryHandleAsync(httpContext, new InvalidOperationException("secret"), CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        Assert.NotNull(written);
        Assert.Equal(StatusCodes.Status500InternalServerError, written.ProblemDetails.Status);
        Assert.DoesNotContain("secret", written.ProblemDetails.Title);
        Assert.DoesNotContain("secret", written.ProblemDetails.Detail ?? string.Empty);
    }

    [Fact]
    public async Task TryHandleAsync_ClientDisconnected_Returns499WithoutWritingAProblem()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var httpContext = new DefaultHttpContext { RequestAborted = cts.Token };
        await cts.CancelAsync();

        // Act
        bool handled = await _sut.TryHandleAsync(httpContext, new OperationCanceledException(cts.Token), CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(499, httpContext.Response.StatusCode);
    }

    public void Dispose()
    {
        _problemDetailsServiceMock.VerifyNoOtherCalls();
    }
}
