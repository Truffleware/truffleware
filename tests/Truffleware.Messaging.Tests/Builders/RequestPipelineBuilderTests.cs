using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Truffleware.Abstractions.Messaging;
using Truffleware.Messaging.Extensions;
using Truffleware.Messaging.Pipelines;

namespace Truffleware.Messaging.Tests.Builders;

public class RequestPipelineBuilderTests
{
    [Fact]
    public void AddPipeline_WithoutMiddleware_Successful()
    {
        var services = new ServiceCollection();

        services.AddRequestResponsePipeline<TestRequest, TestResponse>(typeof(TestRequestPipeline));

        using var provider = services.BuildServiceProvider();
        var registeredPipeline = provider.GetRequiredService<IRequestPipeline<TestRequest, TestResponse>>();

        Assert.IsType<TestRequestPipeline>(registeredPipeline);
    }

    [Fact]
    public void AddPipeline_WithExceptions_Successful()
    {
        var services = new ServiceCollection();

        services
            .AddRequestResponsePipeline<TestRequest, TestResponse>(typeof(TestRequestPipeline))
            .AddExceptionPipeline();

        using var provider = services.BuildServiceProvider();
        var registeredPipeline = provider.GetRequiredService<IRequestPipeline<TestRequest, TestResponse>>();

        var exceptionPipeline = Assert.IsType<ExceptionRequestPipeline<TestRequest, TestResponse>>(registeredPipeline);
        var actualPipeline = Assert.IsType<TestRequestPipeline>(GetNextPipeline(exceptionPipeline));
        Assert.Null(GetNextPipeline(actualPipeline));
    }

    [Fact]
    public void AddPipeline_WithLogging_Successful()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services
            .AddRequestResponsePipeline<TestRequest, TestResponse>(typeof(TestRequestPipeline))
            .AddLoggingPipeline();

        using var provider = services.BuildServiceProvider();
        var registeredPipeline = provider.GetRequiredService<IRequestPipeline<TestRequest, TestResponse>>();

        var loggingPipeline = Assert.IsType<LoggingRequestPipeline<TestRequest, TestResponse>>(registeredPipeline);
        var actualPipeline = Assert.IsType<TestRequestPipeline>(GetNextPipeline(loggingPipeline));
        Assert.Null(GetNextPipeline(actualPipeline));
    }

    [Fact]
    public void AddPipeline_WithMultiple_CorrectOrder()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services
            .AddRequestResponsePipeline<TestRequest, TestResponse>(typeof(TestRequestPipeline))
            .AddExceptionPipeline()
            .AddLoggingPipeline();

        using var provider = services.BuildServiceProvider();
        var registeredPipeline = provider.GetRequiredService<IRequestPipeline<TestRequest, TestResponse>>();

        var loggingPipeline = Assert.IsType<LoggingRequestPipeline<TestRequest, TestResponse>>(registeredPipeline);
        var exceptionPipeline = Assert.IsType<ExceptionRequestPipeline<TestRequest, TestResponse>>(GetNextPipeline(loggingPipeline));
        var actualPipeline = Assert.IsType<TestRequestPipeline>(GetNextPipeline(exceptionPipeline));
        Assert.Null(GetNextPipeline(actualPipeline));
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
