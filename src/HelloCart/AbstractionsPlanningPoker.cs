using MemoryPack;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Samples.HelloCart
{

    /// <summary>
    /// A User can be a participant of a Planning Poker Session or an Admin if he created the Session
    /// </summary>
    [DataContract, MemoryPackable]
    public partial record User : IHasId<string>
    {
        [DataMember] public string Id { get; init; } = "";

        [DataMember] public string Name { get; init; } = "";

        [DataMember] public int Estimate { get; init; } = 0;
    }

    /// <summary>
    /// A Room must have an Admin and can have multiple Players
    /// </summary>
    [DataContract, MemoryPackable]
    public partial record Room : IHasId<string>
    {
        [DataMember] public string Id { get; init; } = "";

        //Players
        [DataMember] public IImmutableList<string> Players { get; init; } = ImmutableList<string>.Empty;

    }

    public interface IUserService : IComputeService
    {
        [ComputeMethod]
        Task<User?> Get(string id, CancellationToken cancellationToken = default);

        [CommandHandler]
        Task Edit(EditCommand<User> command, CancellationToken cancellationToken = default);
    }

    public interface IRoomService : IComputeService
    {
        [ComputeMethod]
        Task<Room?> Get(string id, CancellationToken cancellationToken = default);

        [ComputeMethod]
        Task<decimal> GetAverageEstimates(string id, CancellationToken cancellationToken = default);

        [CommandHandler]
        Task Edit(EditCommand<Room> command, CancellationToken cancellationToken = default);
    }

    [DataContract, MemoryPackable]
    public partial record EditCommand<TItem> : ICommand<Unit>
    where TItem : class, IHasId<string>
    {
        [DataMember] public string Id { get; init; }
        [DataMember] public TItem? Item { get; init; }

        public EditCommand(TItem value) : this(value.Id, value) { }

        [JsonConstructor, MemoryPackConstructor]
        public EditCommand(string id, TItem item)
        {
            Id = id;
            Item = item;
        }

        public void Deconstruct(out string id, out TItem? item)
        {
            id = Id;
            item = Item;
        }
    }


}
