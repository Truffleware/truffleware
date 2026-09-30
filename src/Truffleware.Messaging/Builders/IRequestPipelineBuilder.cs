using Truffleware.Abstractions.Messaging;

namespace Truffleware.Messaging.Builders;

/// <summary>
/// Build a pipeline.
/// </summary>
/// <typeparam name="TRequest">Incoming type to the pipeline this builder creates.</typeparam>
/// <typeparam name="TResponse">Outgoing response from the pipeline this builder creates.</typeparam>
public interface IRequestPipelineBuilder<TRequest, TResponse>
{
    /// <summary>
    /// Registers implementation type for the current pipeline builder.
    /// </summary>
    /// <typeparam name="TPipeline">Implementation type of pipeline to register.</typeparam>
    /// <returns>Next step of the builder.</returns>
    IRequestPipelineMiddlewareBuilder<TRequest, TResponse> Use<TPipeline>()
        where TPipeline : class, IRequestPipeline<TRequest, TResponse>;
}
