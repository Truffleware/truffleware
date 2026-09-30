using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Truffleware.Abstractions.Messaging;
using Truffleware.Messaging.Extensions;
using Truffleware.Messaging.Pipelines;

namespace Truffleware.Tests;

/// <summary>
/// Tests testing multiple parts of Truffleware at once so that everything works harmoniously in the end.
/// </summary>
public class LibraryTests
{
    [Fact]
    public void AddRequestHandler_AsRequestPipeline_Successful()
    {
        var services = new ServiceCollection();

        services
            .ForRequestPipeline<TestRequest, TestResponse>()
            .Use<TestRequestHandler>();

        using var provider = services.BuildServiceProvider();
        Assert.IsType<TestRequestHandler>(provider.GetService<IRequestPipeline<TestRequest, TestResponse>>());
    }

    [Fact]
    public void AddRequestHandler_AsRequestPipelineWithMiddleware_Successful()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services
            .ForRequestPipeline<TestRequest, TestResponse>()
            .Use<TestRequestHandler>()
            .AddLoggingPipeline();

        using var provider = services.BuildServiceProvider();
        var loggingPipeline = Assert.IsType<LoggingRequestPipeline<TestRequest, TestResponse>>(
            provider.GetService<IRequestPipeline<TestRequest, TestResponse>>());
        Assert.IsType<TestRequestHandler>(GetNextPipeline(loggingPipeline));
    }

    [Fact]
    public void AddRequestHandler_AsRequestHandler_Successful()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services
            .ForRequestPipeline<TestRequest, TestResponse>()
            .Use<TestRequestHandler>();

        using var provider = services.BuildServiceProvider();
        Assert.IsType<TestRequestHandler>(provider.GetService<IRequestHandler<TestRequest, TestResponse>>());
    }

    private static object? GetNextPipeline(object pipeline)
    {
        // Don't want to change it to `internal` or similar,
        // so do reflection hacks instead.
        return pipeline
            .GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .FirstOrDefault(field => field.FieldType == typeof(IRequestPipeline<TestRequest, TestResponse>))
            ?.GetValue(pipeline);
    }
}
