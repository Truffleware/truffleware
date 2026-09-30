using Truffleware.Abstractions.Messaging;

namespace Truffleware.Tests;

[RequestHandler<TestRequest, TestResponse>]
public sealed partial class TestRequestHandler
{
    public Task<TestResponse> InvokeAsync(TestRequest request) => Task.FromResult(new TestResponse());
}
