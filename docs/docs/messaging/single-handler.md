# Single handler

For decoupled messaging targeting a single handler use the request-response messaging pattern implemented by the marker attribute <xref:Truffleware.Abstractions.Messaging.RequestHandlerAttribute`2>:

```csharp
public record RequestPing;
public record ResponsePong;

[RequestHandler<RequestPing, ResponsePong>]
public partial class ExampleRequestResponseHandler
{
    public Task<Pong> HandleAsync(RequestPing request) => Task.FromResult(new ResponsePong());
}
```

The generator declares <xref:Truffleware.Abstractions.Messaging.IRequestHandler`2> on the class automatically but you must write the implementation for `HandleAsync` yourself.


You can then send messages via the auto-generated sender elsewhere in your project:

```csharp
public class MyClass(IRequestSender<RequestPing, RequestPong> sender)
{
    public async Task DoThingAsync()
    {
        var request = new();
        var response = await sender.SendAsync(request);
        // DoSomethingWithReponse(response)
    }
}
```

As shown in the example above the sender is designed to be used via dependency injection (DI). Therefore, remember to register the senders and handlers to your containers. For `Microsoft.Extensions.DependencyInjection` Truffleware offers auto-generated extensions:

```csharp
using Truffleware.Generated;

IServiceCollection services = new ServiceCollection();

services.AddSenders();
services.AddHandlers();
```

Both will be registered by their interfaces <xref:Truffleware.Abstractions.Messaging.IRequestHandler`2> & <xref:Truffleware.Abstractions.Messaging.IRequestSender`2> using `Transient` lifetimes.
