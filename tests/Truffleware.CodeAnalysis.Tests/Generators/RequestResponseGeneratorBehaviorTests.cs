using Microsoft.Extensions.DependencyInjection;

using Truffleware.Abstractions.Messaging;
using Truffleware.CodeAnalysis.Tests.Inputs;
using Truffleware.Generated;

namespace Truffleware.CodeAnalysis.Tests.Generators;

[TestClass]
public class RequestResponseGeneratorBehaviorTests
{
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
