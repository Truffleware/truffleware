namespace Truffleware.Abstractions.Messaging;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class NotificationSubscriberAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class NotificationSubscriberAttribute<T> : Attribute
{
}
