using Samples.HelloBlazorServer.Models;

namespace Samples.HelloBlazorServer.Services
{
    public class PlanningPokerService : IComputeService
    {
        private readonly IList<Room> _allRooms = new List<Room>();
        private readonly IList<Player> _allPlayers = new List<Player>();
        private readonly IDictionary<string, IList<string>> _roomsToPlayersMap = new Dictionary<string, IList<string>>();

        private readonly object _lock = new();

        [ComputeMethod]
        public virtual Task<IList<Room>> GetAllRooms()
        {
            lock (_lock) {
                return Task.FromResult(_allRooms);
            }
        }

        [ComputeMethod]
        public virtual Task<Room?> GetRoom(string roomID)
        {
            lock (_lock) {
                return Task.FromResult(_allRooms.Where(r => r.Id == roomID).FirstOrDefault());
            }
        }

        [ComputeMethod]
        public virtual Task<List<Player>> GetAllPlayersForARoom(string roomID)
        {
            lock (_lock) {
                var allPlayerIds = _roomsToPlayersMap[roomID];
                return Task.FromResult(_allPlayers.Where(p => allPlayerIds.Contains(p.Id)).ToList());
            }
        }

        [ComputeMethod]
        public virtual Task<Player?> GetPlayer(string playerId)
        {
            lock (_lock) {
                return Task.FromResult(_allPlayers.Where(p => p.Id == playerId).FirstOrDefault());
            }
        }

        [CommandHandler]
        public virtual Task CreateRoom(Create_NewRoom command, CancellationToken cancellationToken = default)
        {
            
            if (Computed.IsInvalidating()) {
                _ = GetAllRooms();
                return Task.CompletedTask;
            }

            var newRoom = new Room(Guid.NewGuid().ToString(), command.RoomName);
            _allRooms.Add(newRoom);
            _roomsToPlayersMap.Add(newRoom.Id, new List<string>());
            return Task.CompletedTask;
        }

        [CommandHandler]
        public virtual Task CreatePlayerAndJoinRoom(CreatePlayer_And_Join_Room command, CancellationToken cancellationToken = default)
        {

            if (Computed.IsInvalidating()) {
                _ = GetAllPlayersForARoom(command.RoomId);
                return Task.CompletedTask;
            }

            var newPlayer = new Player(command.PlayerID, command.PlayerName);
            _allPlayers.Add(newPlayer);
            if(_roomsToPlayersMap.TryGetValue(command.RoomId, out var playersInRoom)) {
                playersInRoom.Add(newPlayer.Id);
            } else {
                //Should not happen - only if the room does not exists
                //_roomsToPlayersMap.Add(command.RoomId, new List<string>() { command.RoomId });
            }

            return Task.CompletedTask;
        }
    }

    // ReSharper disable once InconsistentNaming
    public record Create_NewRoom(string RoomName) : ICommand<Unit>;
    public record CreatePlayer_And_Join_Room(string RoomId,string PlayerName, string PlayerID) : ICommand<Unit>;
}
