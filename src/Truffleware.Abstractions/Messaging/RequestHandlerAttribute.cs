namespace Truffleware.Abstractions.Messaging;

/// <summary>
/// Marker attribute for source generation.
///
/// Target class must be a partial class as source generation will make it inherit <see cref="IRequestHandler{TRequest,TResponse}"/>.
/// Source generation will auto-generate a <see cref="IRequestSender{TRequest,TResponse}"/>.
/// </summary>
/// <typeparam name="TRequest">Incoming type sent to this handler.</typeparam>
/// <typeparam name="TResponse">Outgoing response from this handler back to the sender.</typeparam>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class RequestHandlerAttribute<TRequest, TResponse> : Attribute
{
}
