using HonorAmongThieves.Heist.GameLogic;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonorAmongThieves.Heist
{
    public class HeistHub : Hub
    {
        private readonly Lobby lobby;

        public HeistHub(Lobby lobby)
        {
            this.lobby = lobby;
        }

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
            if (!Lobby.Rooms.TryGetValue(roomId, out room))
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

        internal async Task ReconnectToActiveGame(Player player)
        {
            await Clients.Caller.SendAsync("JoinRoom_ChangeState", player.Room.Id, player.Name);
            await Clients.Caller.SendAsync("JoinRoom_TakeOverSession", player.Room.Id, player.Name);
            await player.Room.UpdateRoomInfo(player);
            await this.RoomOkay(player.Room, true /*Only update the current caller*/);
        }

        internal async Task ShowError(string errorMessage)
        {
            await Clients.Caller.SendAsync("ShowError", errorMessage);
        }

        public async Task OkayButton(string roomId, string playerName)
        {
            var room = Lobby.Rooms[roomId];
            var player = room.Players[playerName];

            player.Okay = true;
            await room.Okay(this);
            await this.RoomOkay(room);
        }

        public async Task CreateRoom(string userName)
        {
            var roomId = this.lobby.CreateRoom(this, userName);
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
                    var numRooms = Lobby.Rooms.Count;
                    await this.ShowError("Unable to create room. Number of total rooms: " + numRooms);
                }
            }
        }

        public async Task AddBot(string roomId)
        {
            Room room;
            if (!Lobby.Rooms.TryGetValue(roomId, out room))
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
            if (!Lobby.Rooms.TryGetValue(roomId, out room))
            {
                // Room does not exist
                await this.ShowError("Room does not exist.");
                return;
            }

            var createdPlayer = this.lobby.JoinRoom(userName, room, Context.ConnectionId);
            if (createdPlayer != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                await this.JoinRoom_UpdateView(room, createdPlayer);
            }
            else if (!room.SigningUp
                && room.Players.ContainsKey(userName))
            {
                // Take over the existing session
                await this.ResumeSession(roomId, userName);
                return;
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

        public async Task StartRoom(string roomId, int betrayalReward, int maxGameLength, int minGameLength, int maxHeistSize, int minHeistSize, int snitchBlackmailWindow, int networthFudgePercentage, int blackmailRewardPercentage, int jailFinePercentage)
        {
            const int MINPLAYERCOUNT = 2;
            Room room;
            if (!Lobby.Rooms.TryGetValue(roomId, out room)
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

            await this.StartRoom_ChangeState(room, betrayalReward, maxGameLength, minGameLength, maxHeistSize, minHeistSize, snitchBlackmailWindow, networthFudgePercentage, blackmailRewardPercentage, jailFinePercentage);
        }

        internal async Task StartRoom_ChangeState(Room room, int betrayalReward, int maxGameLength, int minGameLength, int maxHeistSize, int minHeistSize, int snitchBlackmailWindow, int networthFudgePercentage, int blackmailRewardPercentage, int jailFinePercentage)
        {
            room.StartGame(betrayalReward, maxGameLength, minGameLength, maxHeistSize, minHeistSize, snitchBlackmailWindow, networthFudgePercentage, blackmailRewardPercentage, jailFinePercentage);
            room.UpdatedTime = DateTime.UtcNow; // Only update the room when the players click something

            await Clients.Group(room.Id).SendAsync("StartRoom_UpdateGameInfo", maxGameLength, minGameLength, snitchBlackmailWindow, blackmailRewardPercentage, jailFinePercentage);
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

        internal async Task RoomOkay(Room room, bool updateCurrentCaller = false)
        {
            var owner = room.Players[room.OwnerName];
            var okayPlayerList = new StringBuilder();
            foreach (var player in room.Players.Values)
            {
                okayPlayerList.Append(player.Name);
                if (!player.Okay)
                {
                    okayPlayerList.Append(" - NOT OKAY");
                }
                okayPlayerList.Append("|");
            }

            if (okayPlayerList.Length > 0)
            {
                okayPlayerList.Length--;
            }

            if (!updateCurrentCaller)
            {
                await Clients.Group(room.Id).SendAsync("RoomOkay_Update", okayPlayerList.ToString());
            }
            else
            {
                await Clients.Caller.SendAsync("RoomOkay_Update", okayPlayerList.ToString());
            }
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

            await this.RoomOkay(room);
        }

        internal async Task UpdateIdleStatus(Player player, bool setOkayButton = true)
        {
            switch (player.CurrentStatus)
            {
                case Player.Status.FindingHeist:
                    var noResponseMessage = TextGenerator.NoHeists; // This means the player had no response - this is equivalent to making a decision to do nothing
                    player.Decision.DecisionMade = true;
                    player.Decision.GoOnHeist = false;
                    await this.UpdateHeistStatus(player, noResponseMessage.Item1, noResponseMessage.Item2, setOkayButton);
                    break;
                case Player.Status.InJail:
                    var inJailMessage = TextGenerator.InJail;
                    await this.UpdateHeistStatus(player, inJailMessage.Item1, string.Format(inJailMessage.Item2, player.YearsLeftInJail), setOkayButton);
                    await this.UpdateCurrentJail(player, player.Room.Players.Values);
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

        internal async Task UpdateCurrentJail(Player currentPlayer, IEnumerable<Player> players)
        {
            var currentJailNames = new StringBuilder();
            foreach (var player in players.Where(p => p.CurrentStatus == Player.Status.InJail))
            {
                if (player.Name != currentPlayer.Name)
                {
                    currentJailNames.Append(player.Name);
                    currentJailNames.Append("|");
                }
            }

            if (currentJailNames.Length > 0)
            {
                currentJailNames.Length--;
            }

            await Clients.Client(currentPlayer.ConnectionId).SendAsync("UpdateCurrentJail", currentJailNames.ToString());
        }

        internal async Task UpdateGlobalNews(Player currentPlayer, IEnumerable<Player> players, bool newToJail, bool heistUpdate)
        {
            var newToJailNames = new StringBuilder();
            var heistUpdateNames = new StringBuilder();

            foreach (var player in players)
            {
                if (player.Name != currentPlayer.Name)
                {
                    if (newToJail
                        && player.Decision.DecisionMade
                        && player.Decision.JailTerm > 0)
                    {
                        newToJailNames.Append(player.Name);
                        newToJailNames.Append("|");
                    }

                    if (heistUpdate
                        && player.Decision.DecisionMade
                        && player.Decision.HeistReward > 0
                        && !player.Decision.ReportPolice)
                    {
                        heistUpdateNames.Append(player.Name);
                        heistUpdateNames.Append("|");
                    }
                }
            }

            if (newToJailNames.Length > 0)
            {
                newToJailNames.Length--;
            }

            if (heistUpdateNames.Length > 0)
            {
                heistUpdateNames.Length--;
            }

            await Clients.Client(currentPlayer.ConnectionId).SendAsync("UpdateGlobalNews", newToJailNames.ToString(), heistUpdateNames.ToString());
        }

        internal async Task HeistPrep_ChangeState(GameLogic.Heist heist, bool sendToCaller = false)
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
            var room = Lobby.Rooms[roomId];
            var blackmailer = room.Players[blackmailerName];
            var victim = room.Players[victimName];

            blackmailer.BlackmailDecision(victim);
            await this.HeistPrep_UpdateDecision(blackmailer);
            await this.OkayButton(roomId, blackmailerName);
        }

        public async Task MakeDecision(string roomId, string playerName, bool turnUpToHeist, bool snitchToPolice, string blackmailVictimName)
        {
            var room = Lobby.Rooms[roomId];
            var player = room.Players[playerName];

            if (!string.IsNullOrWhiteSpace(blackmailVictimName))
            {
                var victim = room.Players[blackmailVictimName];
                player.BlackmailDecision(victim);
            }
            else
            {
                player.MakeDecision(turnUpToHeist, snitchToPolice);
            }

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

        internal async Task UpdateHeistSummary(Player currentPlayer, string fateSummary)
        {
            await Clients.Client(currentPlayer.ConnectionId).SendAsync("UpdateHeistSummary", fateSummary);
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
