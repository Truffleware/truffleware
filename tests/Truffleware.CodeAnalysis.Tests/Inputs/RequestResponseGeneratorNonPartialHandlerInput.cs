using System.Threading.Tasks;

using Truffleware.Abstractions.Messaging;

namespace Truffleware.CodeAnalysis.Tests.Inputs;

public sealed record Ping;
public sealed record Pong;

[RequestHandler<Ping, Pong>]
public class NonPartialHandler
{
    public Task<Pong> InvokeAsync(Ping request) => Task.FromResult(new Pong());
}
