using RestEase;

namespace Samples.Benchmark.Client;

public class HttpFusionTestServiceClient : ITestService
{
    private readonly IFusionTestServiceClientDef _client;

    public HttpFusionTestServiceClient(IFusionTestServiceClientDef client)
        => _client = client;

    public Task AddOrUpdate(TestItem item, long? version, CancellationToken cancellationToken = default)
        => _client.AddOrUpdate(item, version, cancellationToken);

    public Task Remove(long itemId, long version, CancellationToken cancellationToken = default)
        => _client.Remove(itemId, version, cancellationToken);

    public Task<TestItem[]> GetAll(CancellationToken cancellationToken = default)
        => _client.GetAll(cancellationToken);

    public Task<TestItem?> TryGet(long itemId, CancellationToken cancellationToken = default)
        => _client.TryGet(itemId, cancellationToken);
}

[BasePath("api/fusionTestService")]
public interface IFusionTestServiceClientDef
{
    [Post(nameof(AddOrUpdate))]
    Task AddOrUpdate([Body] TestItem item, long? version, CancellationToken cancellationToken = default);
    [Post(nameof(Remove))]
    Task Remove(long itemId, long version, CancellationToken cancellationToken = default);

    [Get(nameof(GetAll))]
    Task<TestItem[]> GetAll(CancellationToken cancellationToken = default);
    [Get(nameof(TryGet))]
    Task<TestItem?> TryGet(long itemId, CancellationToken cancellationToken = default);
}