using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace HonorAmongThieves.RealTimeTest
{
    public class Room
    {
        public Timer GameTimer;

        private IHubContext<RealTimeTestHub> hubContext;

        public Dictionary<string, Player> Players = new Dictionary<string, Player>();

        public Room(IHubContext<RealTimeTestHub> hubContext)
        {
            this.hubContext = hubContext;
            this.GameTimer = new Timer(25)
            {
                AutoReset = true,
                Enabled = false,
            };

            GameTimer.Elapsed += Broadcast;
        }

        private async void Broadcast(object sender, ElapsedEventArgs e)
        {
            if (this.Players.Count == 0)
            {
                return;
            }

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
