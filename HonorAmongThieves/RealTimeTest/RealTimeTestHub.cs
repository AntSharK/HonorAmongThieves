using Microsoft.AspNetCore.SignalR;
using System;
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

        public override async Task OnConnectedAsync()
        {
            var player = new Player();
            player.ConnectionId = Context.ConnectionId;
            player.LastUpdateTime = DateTime.UtcNow;
            player.PosX = 400;
            player.PosY = 400;
            this.room.Players[Context.ConnectionId] = player;

            await Clients.Caller.SendAsync("EstablishedConnection", player.ConnectionId, player.PosX, player.PosY);
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
