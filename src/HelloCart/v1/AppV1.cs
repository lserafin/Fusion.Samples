namespace Samples.HelloCart.V1;

public class AppV1 : AppBase
{
    public AppV1()
    {
        var services = new ServiceCollection();
        services.AddFusion(fusion => {
            fusion.AddService<IUserService, InMemoryUserService>();
            fusion.AddService<IRoomService, InMemoryRoomService>();
        });
        ClientServices = ServerServices = services.BuildServiceProvider();
    }
}
