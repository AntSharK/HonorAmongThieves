using HonorAmongThieves.Game;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonorAmongThieves.Hubs
{
    public class HeistHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("FreshConnection");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // TODO: This requires keeping track of players on a global level
            // That's not hard - but screwing up means leaking players in memory
            // Which is highly resource-intensive for a moderate reward
            await base.OnDisconnectedAsync(exception);
        }

        public async Task ResumeSession(string roomId, string userName)
        {
            Room room;
            if (!Program.Instance.Rooms.TryGetValue(roomId, out room))
            {
                await Clients.Caller.SendAsync("ClearState");
                await this.ShowError("Cannot find Room ID.");
                return;
            }

            Player player;
            if (!room.Players.TryGetValue(userName, out player))
            {
                await Clients.Caller.SendAsync("ClearState");
                await this.ShowError("Cannot find player in room.");
                return;
            }

            // Transfer the connection ID
            player.ConnectionId = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            await player.ResumePlayerSession(this);
        }

        internal async Task ShowError(string errorMessage)
        {
            await Clients.Caller.SendAsync("ShowError", errorMessage);
        }

        public async Task OkayButton(string roomId, string playerName)
        {
            var room = Program.Instance.Rooms[roomId];
            var player = room.Players[playerName];

            player.Okay = true;
            await room.Okay(this);
        }

        public async Task CreateRoom(string userName)
        {
            var roomId = Program.Instance.CreateRoom(this, userName);
            if (!string.IsNullOrEmpty(roomId))
            {
                await this.JoinRoom(roomId, userName);
            }
            else
            {
                if (!Utils.IsValidName(userName))
                {
                    await this.ShowError("Invalid UserName. A valid UserName is required to create a room.");
                }
                else
                {
                    var numRooms = Program.Instance.Rooms.Count;
                    await this.ShowError("Unable to create room. Number of total rooms: " + numRooms);
                }
            }
        }

        public async Task AddBot(string roomId)
        {
            Room room;
            if (!Program.Instance.Rooms.TryGetValue(roomId, out room))
            {
                // Room does not exist
                await this.ShowError("Room does not exist.");
                return;
            }

            var createdBot = room.CreateBot();
            if (createdBot != null)
            {
                await this.JoinRoom_UpdateView(room, createdBot);
            }
            else
            {
                await this.ShowError("Bot creation failed.");
                return;
            }
        }

        public async Task JoinRoom(string roomId, string userName)
        {
            Room room;
            if (!Program.Instance.Rooms.TryGetValue(roomId, out room))
            {
                // Room does not exist
                await this.ShowError("Room does not exist.");
                return;
            }

            var createdPlayer = Program.Instance.JoinRoom(userName, room, Context.ConnectionId);
            if (createdPlayer != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                await this.JoinRoom_UpdateView(room, createdPlayer);
            }
            else
            {
                await this.ShowError("Unable to create player in room. Ensure you have a valid and unique UserName.");
                return;
            }
        }

        internal async Task JoinRoom_UpdateView(Room room, Player newPlayer)
        {
            await Clients.Group(room.Id).SendAsync("JoinRoom", room.Id, newPlayer.Name);

            if (!newPlayer.IsBot)
            {
                await Clients.Caller.SendAsync("JoinRoom_ChangeState", room.Id, newPlayer.Name);
            }

            if (room.OwnerName == newPlayer.Name)
            {
                await Clients.Caller.SendAsync("JoinRoom_CreateStartButton");
            }

            var playerNames = new StringBuilder();
            foreach (var player in room.Players.Values)
            {
                playerNames.Append(player.Name);
                playerNames.Append("|");
            }

            if (playerNames.Length > 0)
            {
                playerNames.Length--;
            }

            await Clients.Group(room.Id).SendAsync("JoinRoom_UpdateState", playerNames.ToString(), newPlayer.Name);
        }

        public async Task StartRoom(string roomId, int betrayalReward, int maxGameLength, int minGameLength, int maxHeistSize, int minHeistSize, int snitchBlackmailWindow)
        {
            const int MINPLAYERCOUNT = 2;
            Room room;
            if (!Program.Instance.Rooms.TryGetValue(roomId, out room)
                && room.SigningUp)
            {
                await this.ShowError("This room has timed out! Please refresh the page.");
                return;
            }

            if (room.Players.Count < MINPLAYERCOUNT)
            {
                await this.ShowError("Need at least " + MINPLAYERCOUNT + " players!");
                return;
            }

            if (minHeistSize < room.Players.Count)
            {
                minHeistSize = room.Players.Count;
            }

            if (minGameLength > maxGameLength)
            {
                minGameLength = maxGameLength;
            }

            await this.StartRoom_ChangeState(room, betrayalReward, maxGameLength, minGameLength, maxHeistSize, minHeistSize, snitchBlackmailWindow);
        }

        internal async Task StartRoom_ChangeState(Room room, int betrayalReward, int maxGameLength, int minGameLength, int maxHeistSize, int minHeistSize, int snitchBlackmailWindow)
        {
            room.StartGame(betrayalReward, maxGameLength, minGameLength, maxHeistSize, minHeistSize, snitchBlackmailWindow);
            room.UpdatedTime = DateTime.UtcNow; // Only update the room when the players click something

            await this.StartRoom_UpdateState(room);
        }

        internal async Task StartRoom_UpdatePlayer(Player player)
        {
            var snitchingEvidence = "NOT A SNITCH";
            if (player.LastBetrayedYear >= 0)
            {
                if (player.Room.SnitchBlackmailWindow < 0)
                {
                    snitchingEvidence = " NEVER";
                }
                else
                {
                    var yearsLeft = player.LastBetrayedYear + player.Room.SnitchBlackmailWindow - player.Room.CurrentYear + 1;
                    if (yearsLeft > 0)
                    {
                        snitchingEvidence = yearsLeft + " YEARS.";
                    }
                    else
                    {
                        snitchingEvidence = "PURGED";
                    }
                }
            }

            await Clients.Client(player.ConnectionId).SendAsync("StartRoom_UpdateState", player.NetWorth, player.Room.CurrentYear /* + 2018*/, player.Name, player.MinJailSentence, player.MaxJailSentence, snitchingEvidence);
        }

        internal async Task StartRoom_UpdateState(Room room)
        {
            foreach (var player in room.Players.Values)
            {
                if (player.CurrentStatus == Player.Status.WaitingForGameStart)
                { 
                    player.CurrentStatus = Player.Status.FindingHeist;
                }

                if (!player.IsBot)
                {
                    await this.StartRoom_UpdatePlayer(player);
                }
            }

            room.SigningUp = false;
            room.SpawnHeists();
            foreach (var heist in room.Heists.Values)
            {
                foreach (var player in heist.Players.Values.Where(p => !p.IsBot))
                {
                    await Groups.AddToGroupAsync(player.ConnectionId, heist.Id);
                }
            }

            foreach (var bot in room.Players.Values.Where(p => p.IsBot))
            {
                bot.BotUpdateState();
            }

            if (room.Heists.Count > 0)
            {
                foreach (var heist in room.Heists.Values)
                {
                    await this.HeistPrep_ChangeState(heist);
                }
            }

            // Update the message for each player who can't act
            foreach (var player in room.Players.Values.Where(p => !p.IsBot))
            {
                await this.UpdateIdleStatus(player);
            }
        }

        internal async Task UpdateIdleStatus(Player player, bool setOkayButton = true)
        {
            switch (player.CurrentStatus)
            {
                case Player.Status.FindingHeist:
                    var noResponseMessage = TextGenerator.NoHeists;
                    await this.UpdateHeistStatus(player, noResponseMessage.Item1, noResponseMessage.Item2, setOkayButton);
                    break;
                case Player.Status.InJail:
                    var inJailMessage = TextGenerator.InJail;
                    await this.UpdateHeistStatus(player, inJailMessage.Item1, string.Format(inJailMessage.Item2, player.YearsLeftInJail), setOkayButton);
                    break;
            }
        }

        internal async Task UpdateHeistStatus(Player player, string title, string message, bool okayButton = false)
        {
            if (okayButton)
            {
                player.Okay = false;
            }

            await Clients.Client(player.ConnectionId).SendAsync("UpdateHeistStatus", title, message, okayButton);
        }

        internal async Task HeistPrep_ChangeState(Heist heist, bool sendToCaller = false)
        {
            var totalNetworth = heist.Players.Values.Sum(n => n.ProjectedNetworth);
            var totalBarsOverNetworth = 20f * heist.Players.Count / (totalNetworth + 1);

            var playerInfo = new StringBuilder();
            foreach (var player in heist.Players.Values)
            {
                playerInfo.Append(player.Name);
                playerInfo.Append(",");

                var barLength = totalBarsOverNetworth * player.ProjectedNetworth;
                var networthBar = new string('|', (int)barLength);
                playerInfo.Append(networthBar);
                playerInfo.Append(",");
                playerInfo.Append(player.TimeSpentInJail);
                playerInfo.Append("=");
            }

            if (playerInfo.Length > 0)
            {
                playerInfo.Length = playerInfo.Length - 1;
            }

            if (sendToCaller)
            {
                await Clients.Caller.SendAsync("HeistPrep_ChangeState", playerInfo.ToString(), heist.TotalReward, heist.SnitchReward);
            }
            else
            {
                await Clients.Group(heist.Id).SendAsync("HeistPrep_ChangeState", playerInfo.ToString(), heist.TotalReward, heist.SnitchReward);
            }
        }

        public async Task CommitBlackmail(string roomId, string blackmailerName, string victimName)
        {
            var room = Program.Instance.Rooms[roomId];
            var blackmailer = room.Players[blackmailerName];
            var victim = room.Players[victimName];

            blackmailer.BlackmailDecision(victim);
            await this.HeistPrep_UpdateDecision(blackmailer);
            await this.OkayButton(roomId, blackmailerName);
        }

        public async Task MakeDecision(string roomId, string playerName, bool turnUpToHeist, bool snitchToPolice)
        {
            var room = Program.Instance.Rooms[roomId];
            var player = room.Players[playerName];

            player.MakeDecision(turnUpToHeist, snitchToPolice);
            await this.HeistPrep_UpdateDecision(player);
            await this.OkayButton(roomId, playerName);
        }

        internal async Task HeistPrep_UpdateDecision(Player player)
        {
            var heistDecisionMessage = TextGenerator.DecisionMessage(player.Decision);
            await this.UpdateHeistStatus(player, heistDecisionMessage.Item1, heistDecisionMessage.Item2);
        }

        internal async Task UpdateHeistMeetup(Player currentPlayer, List<Player> FellowHeisters)
        {
            var playerNames = new StringBuilder();
            foreach (var player in FellowHeisters)
            {
                playerNames.Append(player.Name);
                playerNames.Append("|");
            }

            if (playerNames.Length > 0)
            {
                playerNames.Length--;
            }

            await Clients.Client(currentPlayer.ConnectionId).SendAsync("UpdateHeistMeetup", playerNames.ToString());
        }

        internal async Task EndGame_Broadcast(Room room, bool sendToCaller = false)
        {
            var playerInfo = new StringBuilder();

            var playerList = room.Players.Values.OrderBy(p => p.NetWorth).ToList();
            foreach (var player in playerList)
            {
                playerInfo.Append(player.Name);
                playerInfo.Append("|");
                playerInfo.Append("$" + player.NetWorth + " MILLION");
                playerInfo.Append("|");
                playerInfo.Append(player.BetrayalCount);
                playerInfo.Append("|");
                playerInfo.Append(player.TimeSpentInJail);
                playerInfo.Append("=");
            }

            if (playerInfo.Length > 0)
            {
                playerInfo.Length = playerInfo.Length - 1;
            }

            if (sendToCaller)
            {
                await Clients.Caller.SendAsync("EndGame_Broadcast", room.CurrentYear + 2018, playerInfo.ToString());
            }
            else
            {
                await Clients.Group(room.Id).SendAsync("EndGame_Broadcast", room.CurrentYear + 2018, playerInfo.ToString());
            }
        }
    }
}
