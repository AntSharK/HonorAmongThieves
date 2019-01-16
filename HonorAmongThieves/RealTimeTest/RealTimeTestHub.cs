using Microsoft.AspNetCore.SignalR;
using System;
using System.Text;
using System.Threading.Tasks;

namespace HonorAmongThieves.RealTimeTest
{
    public class RealTimeTestHub : Hub
    {
        private readonly Room room;

        public RealTimeTestHub(Room room)
        {
            this.room = room;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Player player;
            if (!this.room.Players.TryGetValue(Context.ConnectionId, out player))
            {
                return;
            }

            this.room.Players.Remove(player.ConnectionId);
            await Clients.All.SendAsync("Disconnect", player.ConnectionId);

            if (this.room.Players.Count == 0)
            {
                this.room.GameTimer.Stop();
            }

            await base.OnDisconnectedAsync(exception);
        }

        public override async Task OnConnectedAsync()
        {
            var player = new Player();
            player.ConnectionId = Context.ConnectionId;
            player.LastUpdateTime = DateTime.UtcNow;
            player.PosX = 400;
            player.PosY = 400;
            this.room.Players[Context.ConnectionId] = player;
            if (this.room.Players.Count == 1)
            {
                this.room.GameTimer.Start();
            }

            await Clients.Caller.SendAsync("EstablishedConnection", player.ConnectionId, player.PosX, player.PosY);

            var positions = new StringBuilder();
            foreach (var existingPlayer in room.Players.Values)
            {
                positions.Append(existingPlayer.ConnectionId);
                positions.Append('|');
                positions.Append(existingPlayer.PosX);
                positions.Append('|');
                positions.Append(existingPlayer.PosY);
                positions.Append(',');
            }

            if (positions.Length > 0)
            {
                positions.Length--;
            }

            await Clients.Caller.SendAsync("ExistingPlayers", positions.ToString());

            await Clients.All.SendAsync("NewPlayer", player.ConnectionId, player.PosX, player.PosY);
            await base.OnConnectedAsync();
        }

        public void UpdatePosition(int posX, int posY)
        {
            Player player;
            if (!this.room.Players.TryGetValue(Context.ConnectionId, out player))
            {
                return;
            }

            player.PosX = posX;
            player.PosY = posY;
            player.LastUpdateTime = DateTime.UtcNow;
        }
    }
}
