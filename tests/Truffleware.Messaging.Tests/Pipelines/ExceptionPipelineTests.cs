using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using Truffleware.Abstractions.Messaging;
using Truffleware.Messaging.Pipelines;

namespace Truffleware.Messaging.Tests.Pipelines;

public class ExceptionPipelineTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IRequestPipeline<TestRequest, TestResponse>> _mockNext;
    private readonly Mock<Action<TestRequest, Exception>> _mockAction;
    private readonly Mock<Func<TestRequest, Exception, TestResponse>> _mockFallback;

    public ExceptionPipelineTests()
    {
        _fixture.Customize(new AutoMoqCustomization());

        var request = _fixture.Freeze<TestRequest>();
        var response = _fixture.Freeze<TestResponse>();

        _mockNext = _fixture.Freeze<Mock<IRequestPipeline<TestRequest, TestResponse>>>();
        _mockNext
            .Setup(x => x.InvokeAsync(request))
            .ReturnsAsync(response);

        _mockAction = _fixture.Freeze<Mock<Action<TestRequest, Exception>>>();
        _mockFallback = _fixture.Freeze<Mock<Func<TestRequest, Exception, TestResponse>>>();
    }

    [Fact]
    public async Task InvokeAsync_Success_DoesNothing()
    {
        var request = _fixture.Create<TestRequest>();
        var response = _fixture.Create<TestResponse>();

        var sut = _fixture.Create<ExceptionPipeline<TestRequest, TestResponse>>();
        var actual = await sut.InvokeAsync(request);

        Assert.Same(response, actual);
        _mockAction.VerifyNoOtherCalls();
        _mockFallback.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InvokeAsync_ExceptionWithAction_RunsActionAndRethrows()
    {
        var request = _fixture.Create<TestRequest>();
        var exception = new InvalidOperationException();
        _mockNext
            .Setup(m => m.InvokeAsync(request))
            .ThrowsAsync(exception);

        _fixture.Customize<ExceptionPipeline<TestRequest, TestResponse>>(c =>
            c.FromFactory((IRequestPipeline<TestRequest, TestResponse> next,
                    Mock<Action<TestRequest, Exception>> mockAction) =>
                new ExceptionPipeline<TestRequest, TestResponse>(next, mockAction.Object)));
        var sut = _fixture.Create<ExceptionPipeline<TestRequest, TestResponse>>();
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.InvokeAsync(request));

        Assert.Equal(exception, actual); // Not same after being rethrown
        _mockAction.Verify(m => m(request, exception), Times.Once);
        _mockAction.VerifyNoOtherCalls();
        _mockFallback.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InvokeAsync_ExceptionWithFallback_RunsFallbackAndReturnsResponse()
    {
        var request = _fixture.Create<TestRequest>();
        var response = _fixture.Create<TestResponse>();
        var exception = new InvalidOperationException();
        _mockNext
            .Setup(m => m.InvokeAsync(request))
            .ThrowsAsync(exception);
        _mockFallback
            .Setup(m => m(request, exception))
            .Returns(response);

        _fixture.Customize<ExceptionPipeline<TestRequest, TestResponse>>(c =>
            c.FromFactory((IRequestPipeline<TestRequest, TestResponse> next,
                    Mock<Func<TestRequest, Exception, TestResponse>> mockAction) =>
                new ExceptionPipeline<TestRequest, TestResponse>(next, mockAction.Object)));
        var sut = _fixture.Create<ExceptionPipeline<TestRequest, TestResponse>>();
        var actual = await sut.InvokeAsync(request);

        Assert.Equal(response, actual);
        _mockAction.VerifyNoOtherCalls();
        _mockFallback.Verify(m => m(request, exception), Times.Once);
        _mockFallback.VerifyNoOtherCalls();
    }
}
