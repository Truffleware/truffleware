using Microsoft.Extensions.DependencyInjection;
using Truffleware.Abstractions.Messaging;
using Truffleware.Messaging.Builders;

namespace Truffleware.Messaging.Extensions;

public static class ServiceExtension
{
    public static IRequestPipelineBuilder<TRequest, TResponse> AddRequestResponsePipeline<TRequest, TResponse>(
        this IServiceCollection services,
        Type pipeline)
    {
        services.AddTransient(typeof(IRequestPipeline<TRequest, TResponse>), pipeline);

        return new RequestPipelineBuilder<TRequest, TResponse>(services);
    }
}
