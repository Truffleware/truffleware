using System.Threading.Tasks;

using Truffleware.Abstractions.Messaging;

namespace Truffleware.CodeAnalysis.Tests.Inputs;

public sealed record Ping;
public sealed record Pong;

[RequestHandler<Ping, Pong>]
internal sealed partial class PingHandler
{
    public Task<Pong> HandleAsync(Ping request) => Task.FromResult(new Pong());
}
