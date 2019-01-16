using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace HonorAmongThieves.RealTimeTest
{
    public class Room
    {
        private Timer gameTimer;

        private IHubContext<RealTimeTestHub> hubContext;

        public Dictionary<string, Player> Players = new Dictionary<string, Player>();

        public Room(IHubContext<RealTimeTestHub> hubContext)
        {
            this.hubContext = hubContext;
            this.gameTimer = new Timer(Broadcast, null, 100, 100);
        }

        private async void Broadcast(object state)
        {
            var positions = new StringBuilder();
            foreach (var player in this.Players.Values)
            {
                positions.Append(player.ConnectionId);
                positions.Append('|');
                positions.Append(player.PosX);
                positions.Append('|');
                positions.Append(player.PosY);
                positions.Append(',');
            }

            if (positions.Length > 0)
            {
                positions.Length--;
            }

            await this.hubContext.Clients.All.SendAsync("UpdatePositions", positions.ToString());
        }
    }
}
