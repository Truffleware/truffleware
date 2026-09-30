using Microsoft.Extensions.DependencyInjection;
using Truffleware.Abstractions.Messaging;
using Truffleware.Messaging.Pipelines;

namespace Truffleware.Messaging.Builders;

public sealed class RequestPipelineBuilder<TRequest, TResponse>(
    IServiceCollection services)
    : IRequestPipelineBuilder<TRequest, TResponse>,
        IRequestPipelineMiddlewareBuilder<TRequest, TResponse>
{
    private readonly IServiceCollection _services = services;

    public IRequestPipelineMiddlewareBuilder<TRequest, TResponse> AddExceptionPipeline()
    {
        _services.Decorate<IRequestPipeline<TRequest, TResponse>, ExceptionRequestPipeline<TRequest, TResponse>>();

        return this;
    }

    public IRequestPipelineMiddlewareBuilder<TRequest, TResponse> AddLoggingPipeline()
    {
        _services.Decorate<IRequestPipeline<TRequest, TResponse>, LoggingRequestPipeline<TRequest, TResponse>>();

        return this;
    }

    public IRequestPipelineMiddlewareBuilder<TRequest, TResponse> Use<TPipeline>()
        where TPipeline : class, IRequestPipeline<TRequest, TResponse>
    {
        _services.AddTransient<IRequestPipeline<TRequest, TResponse>, TPipeline>();

        return this;
    }
}
