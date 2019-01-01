using HonorAmongThieves.Hubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HonorAmongThieves.Game
{
    public class Room
    {
        public bool SigningUp { get; set; } = true;

        public string Id { get; private set; }

        public string OwnerName { get; set; }

        public Dictionary<string, Player> Players { get; } = new Dictionary<string, Player>();

        public DateTime StartTime { get; private set; }

        public DateTime CreatedTime { get; private set; }

        public DateTime UpdatedTime { get; set; }

        public int CurrentYear { get; set; } = 0;

        public int MaxYears { get; private set; } = Utils.Rng.Next(5, 10);

        public int BetrayalReward { get; private set; } = 60;

        public int InitialMaxHeistCapacity { get; private set; } = 5;

        public int SnitchMurderWindow { get; private set; } = -1;

        public Dictionary<string, Heist> Heists { get; } = new Dictionary<string, Heist>();

        private Random Random = new Random();

        public Status CurrentStatus { get; set; } = Status.SettingUp;

        public Room(string id, HeistHub hub)
        {
            this.Id = id;
            this.CreatedTime = DateTime.UtcNow;
            this.UpdatedTime = DateTime.UtcNow;
        }

        public void SpawnHeists()
        {
            var eligiblePlayers = new List<Player>();
            foreach (var player in this.Players.Values)
            {
                if (player.CurrentStatus == Player.Status.FindingHeist)
                {
                    eligiblePlayers.Add(player);
                }
            }

            // Shuffle the list of players
            // This comment is completely unneccessary since the method name speaks for itself
            // So let's pad it with even more lines
            Utils.Shuffle(eligiblePlayers);

            var maxHeistCapacity = this.InitialMaxHeistCapacity;
            while (eligiblePlayers.Count > 5)
            {
                // Heist capacity should decrease so it can always form 2 groups
                if (eligiblePlayers.Count < maxHeistCapacity + 2)
                {
                    maxHeistCapacity = eligiblePlayers.Count - 2;
                }

                var heistCapacity = this.Random.Next(2, maxHeistCapacity);
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

        private Heist CreateHeist(List<Player> eligiblePlayers, int heistCapacity)
        {
            var heistId = Utils.GenerateId(10, this.Heists);

            var snitchReward = this.BetrayalReward;
            var heist = new Heist(heistId, heistCapacity, snitchReward, this.CurrentYear, this.SnitchMurderWindow);

            for (var i = 0; i < heistCapacity; i++)
            {
                var playerToInsert = eligiblePlayers[i];
                heist.AddPlayer(playerToInsert);
            }

            eligiblePlayers.RemoveRange(0, heistCapacity);
            this.Heists[heistId] = heist;
            return heist;
        }

        public void StartGame(int betrayalReward, int maxGameLength, int maxHeistSize, int snitchMurderWindow)
        {
            this.StartTime = DateTime.UtcNow;
            this.UpdatedTime = DateTime.UtcNow;

            if (betrayalReward >= 0)
            {
                this.BetrayalReward = betrayalReward;
            }

            if (maxHeistSize >= 2)
            {
                this.InitialMaxHeistCapacity = maxHeistSize;
            }

            if (maxGameLength >= 2)
            {
                // Game end time is a random number between the max game length and half of it
                this.MaxYears = Utils.Rng.Next(maxGameLength / 2, maxGameLength);
            }

            this.SnitchMurderWindow = snitchMurderWindow;
            this.SigningUp = false;
        }

        public Player CreatePlayer(string playerName, string connectionId)
        {
            const int ROOMCAPACITY = 20;
            if (this.Players.Count >= ROOMCAPACITY)
            {
                return null;
            }

            if (!this.SigningUp)
            {
                return null;
            }

            if (this.Players.ContainsKey(playerName))
            {
                return null;
            }

            var playerToAdd = new Player(playerName, this);
            playerToAdd.ConnectionId = connectionId;
            this.Players[playerName] = playerToAdd;
            this.UpdatedTime = DateTime.UtcNow;

            return playerToAdd;
        }

        public Player CreateBot()
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

        public void Destroy()
        {
            foreach (var player in this.Players.Values)
            {
                player.CurrentStatus = Player.Status.CleaningUp;
            }
        }

        public async Task Okay(HeistHub hub)
        {
            this.UpdatedTime = DateTime.UtcNow;
            foreach (var player in this.Players.Values)
            {
                if (player.CurrentStatus == Player.Status.FindingHeist 
                    || player.CurrentStatus == Player.Status.HeistDecisionMade
                    || player.CurrentStatus == Player.Status.InHeist
                    || player.CurrentStatus == Player.Status.InJail)
                {
                    // If a player isn't okay, do nothing
                    if (!player.Okay)
                    {
                        return;
                    }
                }

                // OKAY button is still valid for players who are newly dead
                if (player.CurrentStatus == Player.Status.Dead)
                {
                    if (player.PreviousStatus != Player.Status.Dead
                        && !player.Okay)
                    {
                        return;
                    }
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
