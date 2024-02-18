using static System.Console;

namespace Samples.HelloCart;

public abstract class AppBase
{
    public IServiceProvider ServerServices { get; protected set; } = null!;
    public IServiceProvider ClientServices { get; protected set; } = null!;
    public virtual IServiceProvider WatchedServices => ClientServices;

    //Planning Poker (mod)
    public User[] ExistingUsers { get; set; } = Array.Empty<User>();
    public Room[] ExistingRooms { get; set; } = Array.Empty<Room>();

    public virtual async Task InitializeAsync(IServiceProvider services)
    {
        /*
        var dbContextFactory = services.GetService<IDbContextFactory<AppDbContext>>();
        if (dbContextFactory != null) {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }
        */

        var commander = services.Commander();

        // Setup Players + Rooms
        var uLuzian = new User { Id = "u:Luzian", Name="Luzian", Estimate= 13 };
        var uPeter = new User { Id = "u:Peter", Name = "Peter", Estimate = 7 };
        ExistingUsers = new[] { uLuzian, uPeter };
        foreach (var user in ExistingUsers)
            await commander.Call(new EditCommand<User>(user));

        var room1 = new Room() { Id = "room:luzian,peter",
            Players = ImmutableList<string>.Empty
                .Add(uLuzian.Id)
                .Add(uPeter.Id)
        };

        ExistingRooms = new [] { room1 };
        foreach (var room in ExistingRooms)
            await commander.Call(new EditCommand<Room>(room));
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (ClientServices is IAsyncDisposable csd)
            await csd.DisposeAsync();
        if (ServerServices is IAsyncDisposable sd)
            await sd.DisposeAsync();
    }

    public Task Watch(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>();
        foreach (var user in ExistingUsers)
            tasks.Add(WatchUser(services, user.Id, cancellationToken));
        foreach (var room in ExistingRooms)
            tasks.Add(WatchRoomAverage(services, room.Id, cancellationToken));
        return Task.WhenAll(tasks);
    }

    public async Task WatchUser(
        IServiceProvider services, string userId, CancellationToken cancellationToken = default)
    {
        var userService = services.GetRequiredService<IUserService>();
        var computed = await Computed.Capture(() => userService.Get(userId, cancellationToken));
        while (true) {
            WriteLine($"  {computed.Value}");
            await computed.WhenInvalidated(cancellationToken);
            computed = await computed.Update(cancellationToken);
        }
    }

    public async Task WatchRoomAverage(
        IServiceProvider services, string roomId, CancellationToken cancellationToken = default)
    {
        var roomService = services.GetRequiredService<IRoomService>();
        var computed = await Computed.Capture(() => roomService.GetAverageEstimates(roomId, cancellationToken));
        while (true) {
            WriteLine($"  {roomId}: average = {computed.Value}");
            await computed.WhenInvalidated(cancellationToken);
            computed = await computed.Update(cancellationToken);
        }
    }
}
