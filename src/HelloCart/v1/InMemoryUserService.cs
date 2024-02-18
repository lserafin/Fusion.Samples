namespace Samples.HelloCart.V1;

public class InMemoryUserService : IUserService
{
    private readonly ConcurrentDictionary<string, User> _users = new();

    public virtual Task Edit(EditCommand<User> command, CancellationToken cancellationToken = default)
    {
        var (userId, user) = command;
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentOutOfRangeException(nameof(command));
        if (Computed.IsInvalidating()) {
            _ = Get(userId, default);
            return Task.CompletedTask;
        }

        if (user == null)
            _users.Remove(userId, out _);
        else
            _users[userId] = user;
        return Task.CompletedTask;
    }

    public virtual Task<User?> Get(string id, CancellationToken cancellationToken = default)
        => Task.FromResult(_users.GetValueOrDefault(id));
}
