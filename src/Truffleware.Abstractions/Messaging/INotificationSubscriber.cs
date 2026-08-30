namespace Truffleware.Abstractions.Messaging;

public interface INotificationSubscriber<in T>
{
    Task ConsumeAsync(T notification, CancellationToken cancellationToken);
}
