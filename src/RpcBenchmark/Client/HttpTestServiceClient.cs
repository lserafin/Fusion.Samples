using RestEase;

namespace Samples.RpcBenchmark.Client;

public class HttpTestServiceClient : ITestService
{
    private readonly ITestServiceClientDef _client;

    public HttpTestServiceClient(ITestServiceClientDef client)
        => _client = client;

    public Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        => _client.SayHello(request, cancellationToken);

    public Task<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        => _client.GetUser(userId, cancellationToken);
}

[BasePath("api/testService")]
public interface ITestServiceClientDef
{
    [Post(nameof(SayHello))]
    Task<HelloReply> SayHello([Body] HelloRequest request, CancellationToken cancellationToken = default);
    [Get(nameof(GetUser))]
    Task<User?> GetUser(long userId, CancellationToken cancellationToken = default);
}