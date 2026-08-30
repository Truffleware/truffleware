using Microsoft.Extensions.DependencyInjection;

namespace Truffleware.Abstractions.Messaging;

public sealed class NotificationPublisher<T>(
    IServiceProvider serviceProvider)
    : INotificationPublisher<T>
{
    public async Task PublishAsync(T notification)
    {
        await PublishAsync(notification, CancellationToken.None);
    }

    public async Task PublishAsync(T notification, CancellationToken cancellationToken)
    {
        // TODO: Analyzer for this case
        var subscribers = serviceProvider
            .GetServices<INotificationSubscriber<T>>()
            .ToList();
        if (!subscribers.Any())
        {
            throw new InvalidOperationException($"No registrations for {typeof(INotificationSubscriber<T>)}.");
        }

        // TODO: Error handling?
        var tasks = subscribers.Select(s => s.ConsumeAsync(notification, cancellationToken));
        await Task.WhenAll(tasks);
    }
}
