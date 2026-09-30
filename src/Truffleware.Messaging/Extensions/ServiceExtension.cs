using Microsoft.Extensions.DependencyInjection;
using Truffleware.Abstractions.Messaging;
using Truffleware.Messaging.Builders;

namespace Truffleware.Messaging.Extensions;

public static class ServiceExtension
{
    public static RequestPipelineBuilder<TRequest, TResponse> ForRequestPipeline<TRequest, TResponse>(
        this IServiceCollection services)
    {
        return new RequestPipelineBuilder<TRequest, TResponse>(services);
    }
}
