using System.Threading.Tasks;

using Truffleware.Abstractions.Messaging;

namespace Truffleware.CodeAnalysis.Tests.Inputs;

public sealed record DuplicatePing;
public sealed record DuplicatePong;

[RequestHandler<DuplicatePing, DuplicatePong>]
public sealed partial class FirstDuplicatePingHandler
{
    public Task<DuplicatePong> HandleAsync(DuplicatePing request) => Task.FromResult(new DuplicatePong());
}

[RequestHandler<DuplicatePing, DuplicatePong>]
public sealed partial class SecondDuplicatePingHandler
{
    public Task<DuplicatePong> HandleAsync(DuplicatePing request) => Task.FromResult(new DuplicatePong());
}
