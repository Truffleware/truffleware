using Truffleware.Abstractions.Messaging;

namespace Truffleware.Messaging.Tests;

public class TestRequestPipeline : IRequestPipeline<TestRequest, TestResponse>
{
    public Task<TestResponse> InvokeAsync(TestRequest request)
    {
        throw new NotImplementedException();
    }
}
