using AutoFixture;
using AutoFixture.AutoMoq;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using Truffleware.Abstractions.Messaging;
using Truffleware.CodeAnalysis.Tests.Inputs;
using Truffleware.Generated;

namespace Truffleware.CodeAnalysis.Tests.Generators;

public class RequestResponseGeneratorBehaviorTests
{
    private readonly IFixture _fixture;

    private readonly Mock<IRequestHandler<Ping, Pong>> _mockHandler;

    protected RequestResponseGeneratorBehaviorTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockHandler = _fixture.Freeze<Mock<IRequestHandler<Ping, Pong>>>();
    }

    [TestClass]
    public class HandlerTests : RequestResponseGeneratorBehaviorTests
    {
        [TestMethod]
        public async Task SendAsync_CorrectlyConfigured_Success()
        {
            var ping = _fixture.Create<Ping>();
            var expected = new Pong();
            _mockHandler.Setup(h => h.HandleAsync(ping)).ReturnsAsync(expected);

            var sender = _fixture.Create<SenderPingToPong>();
            var actual = await sender.SendAsync(ping);

            _mockHandler.Verify(h => h.HandleAsync(ping), Times.Once);
            _mockHandler.VerifyNoOtherCalls();
            Assert.AreSame(expected, actual);
        }
    }

    [TestClass]
    public class ServiceExtensionTests : RequestResponseGeneratorBehaviorTests
    {
        [TestMethod]
        public void AddSenders_CorrectlyConfigured_RegistersSuccessfully()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSenders();
            services.AddHandlers();

            using var provider = services.BuildServiceProvider();

            Assert.IsNotNull(provider.GetService<IRequestSender<Ping, Pong>>());
        }
    }
}
