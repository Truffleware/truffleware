namespace Truffleware.Abstractions.Messaging;

public interface INotificationPublisher<in T>
{
    Task PublishAsync(T notification);
    Task PublishAsync(T notification, CancellationToken cancellationToken);
}
