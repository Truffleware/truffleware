using Microsoft.Extensions.DependencyInjection;
using Truffleware.Abstractions.Messaging;
using Truffleware.Messaging.Pipelines;

namespace Truffleware.Messaging.Builders;

public sealed class RequestPipelineBuilder<TRequest, TResponse>(
    IServiceCollection services)
    : IRequestPipelineBuilder<TRequest, TResponse>
{
    private readonly IServiceCollection _services = services;

    public IRequestPipelineBuilder<TRequest, TResponse> AddExceptionPipeline()
    {
        _services.Decorate<IRequestPipeline<TRequest, TResponse>, ExceptionRequestPipeline<TRequest, TResponse>>();

        return this;
    }

    public IRequestPipelineBuilder<TRequest, TResponse> AddLoggingPipeline()
    {
        _services.Decorate<IRequestPipeline<TRequest, TResponse>, LoggingRequestPipeline<TRequest, TResponse>>();

        return this;
    }
}
