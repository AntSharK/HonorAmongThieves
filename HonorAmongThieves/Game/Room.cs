using HonorAmongThieves.Hubs;
using System;
using System.Collections.Generic;

namespace HonorAmongThieves.Game
{
    public class Room
    {
        public bool SigningUp { get; set; } = true;

        public string Id { get; private set; }

        public string OwnerName { get; set; }

        public List<Player> Players { get; } = new List<Player>();

        public DateTime StartTime { get; private set; }

        public DateTime CreatedTime { get; private set; }

        public DateTime UpdatedTime { get; set; }

        public int CurrentYear { get; set; } = 0;

        public int MaxYears { get; private set; } = Utils.Rng.Next(6, 15);

        public int BetrayalReward { get; private set; } = 60;

        public int InitialMaxHeistCapacity { get; private set; } = 5;

        public Dictionary<string, Heist> Heists { get; } = new Dictionary<string, Heist>();

        public HeistHub Hub { get; private set; }

        private Random Random = new Random();

        public Room(string id, HeistHub hub)
        {
            this.Id = id;
            this.Hub = hub;
            this.CreatedTime = DateTime.UtcNow;
            this.UpdatedTime = DateTime.UtcNow;
        }

        public void SpawnHeists()
        {
            this.UpdatedTime = DateTime.UtcNow;

            var eligiblePlayers = new List<Player>();
            foreach (var player in this.Players)
            {
                if (player.CurrentStatus == Player.Status.FindingHeist)
                {
                    player.CurrentStatus = Player.Status.InHeist;
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

            // Note that this DOES NOT CLEAR THE CURRENT HEIST LIST. That should ALREADY HAVE BEEN DONE.
        }

        private Heist CreateHeist(List<Player> eligiblePlayers, int heistCapacity)
        {
            var heistId = Utils.GenerateId(12, this.Heists);

            var snitchReward = this.BetrayalReward;
            var heist = new Heist(heistId, heistCapacity, snitchReward, this.Hub);

            for (var i = 0; i < heistCapacity; i++)
            {
                var playerToInsert = eligiblePlayers[i];
                heist.AddPlayer(playerToInsert);
            }

            eligiblePlayers.RemoveRange(0, heistCapacity);
            this.Heists[heistId] = heist;
            return heist;
        }

        public void StartGame(int betrayalReward, int maxGameLength, int maxHeistSize)
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

            if (maxGameLength >= 5)
            {
                // Game end time is a random number between the max game length and half of it
                this.MaxYears = Utils.Rng.Next(maxGameLength / 2, maxGameLength);
            }

            this.SigningUp = false;
        }

        public Player CreatePlayer(string playerName, string connectionId)
        {
            const int ROOMCAPACITY = 10;
            if (this.Players.Count >= ROOMCAPACITY)
            {
                return null;
            }

            if (!this.SigningUp)
            {
                return null;
            }

            // This is an unordered list, so we have to check every single player
            foreach (var player in this.Players)
            {
                if (player.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }

            var playerToAdd = new Player(playerName, this);
            playerToAdd.ConnectionId = connectionId;
            this.Players.Add(playerToAdd);
            this.UpdatedTime = DateTime.UtcNow;

            return playerToAdd;
        }

        public void Destroy()
        {
            foreach (var player in this.Players)
            {
                player.CurrentStatus = Player.Status.CleaningUp;
            }
        }

        public bool TryResolveHeists()
        {
            // TODO: TRY to resolve all heists
            return false;
        }
    }
}
