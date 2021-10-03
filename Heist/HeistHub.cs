using HonorAmongThieves.Heist.GameLogic;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonorAmongThieves.Heist
{
    public class HeistHub : GameHub<HeistHub, HeistRoom, HeistPlayer>
    {
        public HeistHub(HeistGame lobby)
        {
            this.lobby = lobby;
        }

        /// <inheritdoc />
        public override async Task ResumePlayerSession(HeistPlayer player)
        {
            if (player.Room.SettingUp)
            {
                await this.JoinRoom_UpdateView(player.Room, player);
                return;
            }

            // This doesn't actually happen since the endgame_broadcast deletes the session state
            if (player.Room.CurrentYear == player.Room.MaxYears)
            {
                await this.EndGame_Broadcast(player.Room, true);
                return;
            }

            await this.ReconnectToActiveGame(player, player.Room);
            await this.StartRoom_UpdatePlayer(player);

            if (player.Room.CurrentStatus == HeistRoom.Status.ResolvingHeists)
            {
                await player.UpdateFateView(this, !player.Okay /*Don't set the OKAY button if it has already been pressed*/);
                return;
            }

            if (player.Room.CurrentStatus == HeistRoom.Status.AwaitingHeistDecisions
                || player.Room.CurrentStatus == HeistRoom.Status.NoHeists)
            {
                switch (player.CurrentStatus)
                {
                    // Idle statuses
                    case HeistPlayer.Status.FindingHeist:
                    case HeistPlayer.Status.InJail:
                        await this.UpdateIdleStatus(player, !player.Okay /*Don't set the OKAY button if it has already been pressed*/);
                        return;
                    case HeistPlayer.Status.InHeist:
                        await this.HeistPrep_ChangeState(player.CurrentHeist, true);
                        return;
                    case HeistPlayer.Status.HeistDecisionMade:
                        await this.HeistPrep_UpdateDecision(player);
                        return;
                    default:
                        // This should not happen
                        await this.ShowError("ERROR RESUMING CURRENT STATE");
                        return;
                }
            }
        }

        /// <inheritdoc />
        public override async Task ReconnectToActiveGame(HeistPlayer player, HeistRoom room)
        {
            await base.ReconnectToActiveGame(player, room);
            await room.UpdateRoomInfo(player);
            await this.RoomOkay(player.Room, true /*Only update the current caller*/);
        }

        public async Task OkayButton(string roomId, string playerName)
        {
            var room = this.lobby.Rooms[roomId];
            var player = room.Players[playerName];

            player.Okay = true;
            await room.Okay(this);
            await this.RoomOkay(room);
        }

        public async Task AddBot(string roomId)
        {
            HeistRoom room;
            if (!this.lobby.Rooms.TryGetValue(roomId, out room))
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

        public async Task StartRoom(string roomId, int betrayalReward, int maxGameLength, int minGameLength, int maxHeistSize, int minHeistSize, int snitchBlackmailWindow, int networthFudgePercentage, int blackmailRewardPercentage, int jailFinePercentage)
        {
            const int MINPLAYERCOUNT = 2;
            HeistRoom room;
            if (!this.lobby.Rooms.TryGetValue(roomId, out room)
                && room.SettingUp)
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

        internal async Task StartRoom_ChangeState(HeistRoom room, int betrayalReward, int maxGameLength, int minGameLength, int maxHeistSize, int minHeistSize, int snitchBlackmailWindow, int networthFudgePercentage, int blackmailRewardPercentage, int jailFinePercentage)
        {
            room.StartGame(betrayalReward, maxGameLength, minGameLength, maxHeistSize, minHeistSize, snitchBlackmailWindow, networthFudgePercentage, blackmailRewardPercentage, jailFinePercentage);
            await Clients.Group(room.Id).SendAsync("StartRoom_UpdateGameInfo", maxGameLength, minGameLength, snitchBlackmailWindow, blackmailRewardPercentage, jailFinePercentage);
            await this.StartRoom_UpdateState(room);
        }

        internal async Task StartRoom_UpdatePlayer(HeistPlayer player)
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

        internal async Task RoomOkay(HeistRoom room, bool updateCurrentCaller = false)
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

        internal async Task StartRoom_UpdateState(HeistRoom room)
        {
            foreach (var player in room.Players.Values)
            {
                if (player.CurrentStatus == HeistPlayer.Status.WaitingForGameStart)
                { 
                    player.CurrentStatus = HeistPlayer.Status.FindingHeist;
                }

                if (!player.IsBot)
                {
                    await this.StartRoom_UpdatePlayer(player);
                }
            }

            room.SettingUp = false;
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

        internal async Task UpdateIdleStatus(HeistPlayer player, bool setOkayButton = true)
        {
            switch (player.CurrentStatus)
            {
                case HeistPlayer.Status.FindingHeist:
                    var noResponseMessage = TextGenerator.NoHeists; // This means the player had no response - this is equivalent to making a decision to do nothing
                    player.Decision.DecisionMade = true;
                    player.Decision.GoOnHeist = false;
                    await this.UpdateHeistStatus(player, noResponseMessage.Item1, noResponseMessage.Item2, setOkayButton);
                    break;
                case HeistPlayer.Status.InJail:
                    var inJailMessage = TextGenerator.InJail;
                    await this.UpdateHeistStatus(player, inJailMessage.Item1, string.Format(inJailMessage.Item2, player.YearsLeftInJail), setOkayButton);
                    await this.UpdateCurrentJail(player, player.Room.Players.Values);
                    break;
            }
        }

        internal async Task UpdateHeistStatus(HeistPlayer player, string title, string message, bool okayButton = false)
        {
            if (okayButton)
            {
                player.Okay = false;
            }

            await Clients.Client(player.ConnectionId).SendAsync("UpdateHeistStatus", title, message, okayButton);
        }

        internal async Task UpdateCurrentJail(HeistPlayer currentPlayer, IEnumerable<HeistPlayer> players)
        {
            var currentJailNames = new StringBuilder();
            foreach (var player in players.Where(p => p.CurrentStatus == HeistPlayer.Status.InJail))
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

        internal async Task UpdateGlobalNews(HeistPlayer currentPlayer, IEnumerable<HeistPlayer> players, bool newToJail, bool heistUpdate)
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

        public async Task MakeDecision(string roomId, string playerName, bool turnUpToHeist, bool snitchToPolice, string blackmailVictimName)
        {
            var room = this.lobby.Rooms[roomId];
            var player = room.Players[playerName];

            if (!string.IsNullOrWhiteSpace(blackmailVictimName)
                && turnUpToHeist
                && !snitchToPolice)
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

        internal async Task HeistPrep_UpdateDecision(HeistPlayer player)
        {
            var heistDecisionMessage = TextGenerator.DecisionMessage(player.Decision);
            await this.UpdateHeistStatus(player, heistDecisionMessage.Item1, heistDecisionMessage.Item2);
        }

        internal async Task UpdateHeistMeetup(HeistPlayer currentPlayer, List<HeistPlayer> FellowHeisters)
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

        internal async Task UpdateHeistSummary(HeistPlayer currentPlayer, string fateSummary)
        {
            await Clients.Client(currentPlayer.ConnectionId).SendAsync("UpdateHeistSummary", fateSummary);
        }

        internal async Task EndGame_Broadcast(HeistRoom room, bool sendToCaller = false)
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
