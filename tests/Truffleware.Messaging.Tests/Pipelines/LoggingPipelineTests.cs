using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using Truffleware.Abstractions.Messaging;
using Truffleware.Messaging.Pipelines;

namespace Truffleware.Messaging.Tests.Pipelines;

[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging")]
public class LoggingPipelineTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<ILogger<LoggingPipeline<TestRequest, TestResponse>>> _mockLogger;
    private readonly Mock<IRequestPipeline<TestRequest, TestResponse>> _mockNext;

    public LoggingPipelineTests()
    {
        _fixture.Customize(new AutoMoqCustomization());
        _mockLogger = _fixture.Freeze<Mock<ILogger<LoggingPipeline<TestRequest, TestResponse>>>>();
        _mockLogger
            .Setup(m => m.IsEnabled(It.IsAny<LogLevel>()))
            .Returns(true);

        var request = _fixture.Freeze<TestRequest>();
        var response = _fixture.Freeze<TestResponse>();

        _mockNext = _fixture.Freeze<Mock<IRequestPipeline<TestRequest, TestResponse>>>();
        _mockNext
            .Setup(x => x.InvokeAsync(request))
            .ReturnsAsync(response);
    }

    [Fact]
    public async Task InvokeAsync_Success_LogsRequestResponse()
    {
        var request = _fixture.Create<TestRequest>();
        var response = _fixture.Create<TestResponse>();

        var sut = _fixture.Create<LoggingPipeline<TestRequest, TestResponse>>();
        var actual = await sut.InvokeAsync(request);

        Assert.Same(response, actual);
        VerifyLogged(
            LogLevel.Information,
            $"Request {request} resulted in response: {response}.");
        _mockLogger.Verify(m => m.IsEnabled(LogLevel.Information), Times.Once);
        _mockLogger.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InvokeAsync_Fail_LogsRequestAndRethrowsException()
    {
        var request = _fixture.Create<TestRequest>();
        var exception = new InvalidOperationException();
        _mockNext
            .Setup(x => x.InvokeAsync(request))
            .ThrowsAsync(exception);

        var sut = _fixture.Create<LoggingPipeline<TestRequest, TestResponse>>();
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.InvokeAsync(request));

        Assert.Same(exception, actual);
        VerifyLogged(LogLevel.Warning, $"Request {request} failed.", exception);
        _mockLogger.Verify(m => m.IsEnabled(LogLevel.Warning), Times.Once);
        _mockLogger.VerifyNoOtherCalls();
    }

    private void VerifyLogged(LogLevel logLevel, string message)
    {
        _mockLogger.Verify(m => m.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private void VerifyLogged(LogLevel logLevel, string message, Exception exception)
    {
        _mockLogger.Verify(m => m.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                It.Is<Exception>(loggedException => loggedException == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
