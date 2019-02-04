using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HonorAmongThieves.Heist.GameLogic
{
    public class HeistRoom : Room<HeistPlayer, HeistHub>
    {
        public int CurrentYear { get; set; } = 0;

        public int MaxYears { get; private set; } = Utils.Rng.Next(5, 10);

        public int BetrayalReward { get; private set; } = 60;

        public int InitialMaxHeistCapacity { get; private set; } = 5;

        public int InitialMinHeistCapacity { get; private set; } = 3;

        public int SnitchBlackmailWindow { get; private set; } = -1;

        public int NetworthFudgePercentage { get; private set; } = 10;

        public int BlackmailRewardPercentage { get; private set; } = 60;

        public int JailFinePercentage { get; private set; } = 20;

        public Dictionary<string, Heist> Heists { get; } = new Dictionary<string, Heist>();

        private Random Random = new Random();

        public Status CurrentStatus { get; set; } = Status.SettingUp;

        private IHubContext<HeistHub> hubContext;

        private int minGameLength;

        private int maxGameLength;

        public HeistRoom(string id,IHubContext<HeistHub> hubContext)
            : base(id, hubContext) { }

        protected override HeistPlayer InstantiatePlayer(string playerName)
        {
            return new HeistPlayer(playerName, this);
        }

        public async Task UpdateRoomInfo(HeistPlayer player)
        {
            await this.hubContext.Clients.Client(player.ConnectionId).SendAsync("StartRoom_UpdateGameInfo", 
                this.maxGameLength,
                this.minGameLength,
                this.SnitchBlackmailWindow, 
                this.BlackmailRewardPercentage,
                this.JailFinePercentage);
        }

        public void SpawnHeists()
        {
            var eligiblePlayers = new List<HeistPlayer>();
            foreach (var player in this.Players.Values)
            {
                player.Decision = new HeistPlayer.HeistDecision(); // Reset all decisions
                if (player.CurrentStatus == HeistPlayer.Status.FindingHeist)
                {
                    eligiblePlayers.Add(player);
                }
            }

            // Shuffle the list of players
            // This comment is completely unneccessary since the method name speaks for itself
            // So let's pad it with even more lines
            Utils.Shuffle(eligiblePlayers);

            var minHeistCapacity = this.InitialMinHeistCapacity;
            var maxHeistCapacity = this.InitialMaxHeistCapacity;
            while (eligiblePlayers.Count > 5)
            {
                // Heist capacity should decrease so it can always form 2 groups
                if (eligiblePlayers.Count < maxHeistCapacity + 2)
                {
                    maxHeistCapacity = eligiblePlayers.Count - 2;
                }

                if (eligiblePlayers.Count <= minHeistCapacity * 2)
                {
                    minHeistCapacity = Math.Min(eligiblePlayers.Count - 1, maxHeistCapacity);
                }

                var heistCapacity = this.Random.Next(minHeistCapacity, maxHeistCapacity);
                this.CreateHeist(eligiblePlayers, heistCapacity);
            }

            switch (eligiblePlayers.Count)
            {
                case 5:
                    this.CreateHeist(eligiblePlayers, 3);
                    this.CreateHeist(eligiblePlayers, 2);
                    break;
                case 4:
                    this.CreateHeist(eligiblePlayers, 2);
                    this.CreateHeist(eligiblePlayers, 2);
                    break;
                case 3:
                case 2:
                    this.CreateHeist(eligiblePlayers, eligiblePlayers.Count);
                    break;
                case 1:
                default:
                    // Do nothing
                    break;
            }

            if (this.Heists.Count > 0)
            {
                this.CurrentStatus = Status.AwaitingHeistDecisions;
            }
            else
            {
                this.CurrentStatus = Status.NoHeists;
            }
        }

        private Heist CreateHeist(List<HeistPlayer> eligiblePlayers, int heistCapacity)
        {
            var heistId = Utils.GenerateId(10, this.Heists);

            var snitchReward = this.BetrayalReward;
            var heist = new Heist(heistId, heistCapacity, snitchReward, this.CurrentYear, this.SnitchBlackmailWindow, this.NetworthFudgePercentage, this.BlackmailRewardPercentage, this.JailFinePercentage);

            for (var i = 0; i < heistCapacity; i++)
            {
                var playerToInsert = eligiblePlayers[i];
                heist.AddPlayer(playerToInsert);
            }

            eligiblePlayers.RemoveRange(0, heistCapacity);
            this.Heists[heistId] = heist;
            return heist;
        }

        public void StartGame(int betrayalReward, int maxGameLength, int minGameLength, int maxHeistSize, int minHeistSize, int snitchBlackmailWindow, int networthFudgePercentage, int blackmailRewardPercentage, int jailFinePercentage)
        {
            this.UpdatedTime = DateTime.UtcNow;
            this.minGameLength = minGameLength;
            this.maxGameLength = maxGameLength;

            if (networthFudgePercentage > 0 && networthFudgePercentage < 99)
            {
                this.NetworthFudgePercentage = networthFudgePercentage;
            }

            if (betrayalReward >= 0)
            {
                this.BetrayalReward = betrayalReward;
            }

            if (maxHeistSize >= 3)
            {
                this.InitialMaxHeistCapacity = maxHeistSize;
            }

            if (minHeistSize <= this.InitialMaxHeistCapacity)
            {
                this.InitialMinHeistCapacity = minHeistSize;
            }

            if (maxGameLength >= 2)
            {
                // Game end time is a random number between the min and max game length
                this.MaxYears = Utils.Rng.Next(minGameLength, maxGameLength);
            }

            if (blackmailRewardPercentage > 0 && blackmailRewardPercentage < 101)
            {
                this.BlackmailRewardPercentage = blackmailRewardPercentage;
            }

            if (jailFinePercentage > 0 && jailFinePercentage < 101)
            {
                this.JailFinePercentage = jailFinePercentage;
            }

            this.SnitchBlackmailWindow = snitchBlackmailWindow;
            this.SettingUp = false;
        }

        public HeistPlayer CreateBot()
        {
            string[] BOTNAMES = { "SAMBOT", "ANNBOT", "RONBOT", "TIMBOT", "GEORGEBOT", "SARABOT", "GEORGEBOT" };
            var botName = BOTNAMES[Utils.Rng.Next(0, BOTNAMES.Length)];
            var bot = this.CreatePlayer(botName, null);

            if (bot != null)
            {
                bot.ConnectionId = null;
                bot.IsBot = true;
            }

            return bot;
        }

        public override void Destroy()
        {
            foreach (var player in this.Players.Values)
            {
                player.CurrentStatus = HeistPlayer.Status.CleaningUp;
            }
        }

        public async Task Okay(HeistHub hub)
        {
            this.UpdatedTime = DateTime.UtcNow;
            foreach (var player in this.Players.Values)
            {
                // If a player isn't okay, do nothing
                if (!player.Okay)
                {
                    //await hub.OkayButton_Acknowledge();
                    return;
                }
            }

            switch (this.CurrentStatus)
            {
                case Status.AwaitingHeistDecisions:
                    foreach (var heist in this.Heists.Values)
                    {
                        heist.Resolve();
                    }

                    await this.UpdateFate(hub);
                    this.CurrentStatus = Status.ResolvingHeists;
                    break;

                case Status.NoHeists:
                    await this.UpdateFate(hub);
                    this.CurrentStatus = Status.ResolvingHeists;
                    break;
                case Status.ResolvingHeists:
                    this.CurrentYear++;
                    if (this.CurrentYear == this.MaxYears)
                    {
                        await hub.EndGame_Broadcast(this);
                        return;
                    }

                    this.CurrentStatus = Status.SettingUp;
                    this.Heists.Clear();
                    await hub.StartRoom_UpdateState(this);
                    break;
            }
        }

        private async Task UpdateFate(HeistHub hub)
        {
            foreach (var player in this.Players.Values)
            {
                await player.UpdateFateLogic(hub);
            }
        }

        public enum Status
        {
            SettingUp,
            NoHeists,
            AwaitingHeistDecisions,
            ResolvingHeists
        }
    }
}
