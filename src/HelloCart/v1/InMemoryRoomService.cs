namespace Samples.HelloCart.V1;

public class InMemoryRoomService : IRoomService
{
    private readonly ConcurrentDictionary<string, Room> _rooms = new();
    private readonly IUserService _users;

    public InMemoryRoomService(IUserService users)
        => _users = users;

    public virtual Task Edit(EditCommand<Room> command, CancellationToken cancellationToken = default)
    {
        var (roomId, room) = command;
        if (string.IsNullOrEmpty(roomId))
            throw new ArgumentOutOfRangeException(nameof(command));
        if (Computed.IsInvalidating()) {
            _ = Get(roomId, default);
            return Task.CompletedTask;
        }

        if (room == null)
            _rooms.Remove(roomId, out _);
        else
            _rooms[roomId] = room;
        return Task.CompletedTask;
    }

    public virtual Task<Room?> Get(string id, CancellationToken cancellationToken = default)
        => Task.FromResult(_rooms.GetValueOrDefault(id));

    public virtual async Task<decimal> GetAverageEstimates(string id, CancellationToken cancellationToken = default)
    {
        var room = await Get(id, cancellationToken);
        if (room == null)
            return 0;
        var total = 0M;
        foreach (var userId in room.Players) {
            var user = await _users.Get(userId, cancellationToken);
            total += (user?.Estimate ?? 0);
        }
        return total / room.Players.Count;
    }
}
