# One-to-one

For decoupled one-to-one messaging use the request-response messaging pattern implemented by <xref:Truffleware.Abstractions.Messaging.RequestHandlerAttribute`2>.

```csharp
public sealed record RequestPing;
public sealed record ResponsePong;

[RequestResponse<RequestPing, ResponsePong>]
public partial class ExampleRequestResponseHandler
{
    public Task<Pong> HandleAsync(RequestPing request) => Task.FromResult(new ResponsePong());
}
```

You can then send the messages via the auto-generated sender elsewhere in your project:

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

Remember to register the senders and handlers to your DI. For `Microsoft.Extensions.DependencyInjection` Truffleware offers an auto-generated extension:

```csharp
using Truffleware.Generated;

IServiceCollection services = new ServiceCollection();

services.AddSenders();
services.AddHandlers();
```

They will be registered by their interfaces <xref:Truffleware.Abstractions.Messaging.IRequestHandler`2> & <xref:Truffleware.Abstractions.Messaging.IRequestSender`2> as `Transient`.
