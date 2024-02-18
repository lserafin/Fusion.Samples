using Samples.HelloCart;
using Samples.HelloCart.V1;
using static System.Console;

// Create services
AppBase? app;
var isFirstTry = true;
while(true) {
    WriteLine("Select the implementation to use:");
    WriteLine("  1: ConcurrentDictionary-based");
    Write("Type 1..5: ");
    var input = isFirstTry
        ? args.SingleOrDefault() ?? ReadLine()
        : ReadLine();
    input = (input ?? "").Trim();
    app = input switch {
        "1" => new AppV1(),
        _ => null,
    };
    if (app != null)
        break;
    WriteLine("Invalid selection.");
    WriteLine();
    isFirstTry = false;
}
await using var appDisposable = app;
await app.InitializeAsync(app.ServerServices);

// Starting watch tasks
WriteLine("Initial state:");
using var cts = new CancellationTokenSource();
_ = app.Watch(app.WatchedServices, cts.Token);
await Task.Delay(700); // Just to make sure watch tasks print whatever they want before our prompt appears
// await AutoRunner.Run(app);

var userService = app.ClientServices.GetRequiredService<IUserService>();
var commander = app.ClientServices.Commander();
WriteLine();
WriteLine("Change user Estimate by typing [userId]=[estimate], e.g. \"luzian=0\".");
WriteLine("See the everage of every affected room changes.");
while (true) {
    await Task.Delay(500);
    WriteLine();
    Write("[userId]=[estimate]: ");
    try {
        var input = (ReadLine() ?? "").Trim();
        if (input == "")
            break;
        var parts = input.Split("=");
        if (parts.Length != 2)
            throw new ApplicationException("Invalid estimate expression.");

        var userId = parts[0].Trim();
        var estimate = int.Parse(parts[1].Trim());
        var user = await userService.Get(userId);
        if (user == null)
            throw new KeyNotFoundException("Specified user doesn't exist.");

        var command = new EditCommand<Samples.HelloCart.User>(user with { Estimate = estimate });
        await commander.Call(command);
        // You can run absolutely identical action with:
        // await app.ClientServices.Commander().Call(command);
    }
    catch (Exception e) {
        WriteLine($"Error: {e.Message}");
    }
}
WriteLine("Terminating...");
cts.Cancel();
