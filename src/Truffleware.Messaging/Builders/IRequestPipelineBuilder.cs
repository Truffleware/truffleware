namespace Truffleware.Messaging.Builders;

/// <summary>
/// Build a pipeline with middleware.
/// </summary>
/// <typeparam name="TRequest">Incoming type to the pipeline this builder creates.</typeparam>
/// <typeparam name="TResponse">Outgoing response from the pipeline this builder creates.</typeparam>
public interface IRequestPipelineBuilder<in TRequest, TResponse>
{
    /// <summary>
    /// Add exception handling to the pipeline.
    /// </summary>
    public IRequestPipelineBuilder<TRequest, TResponse> AddExceptionPipeline();

    /// <summary>
    /// Add logging to the pipeline.
    /// </summary>
    public IRequestPipelineBuilder<TRequest, TResponse> AddLoggingPipeline();
}
