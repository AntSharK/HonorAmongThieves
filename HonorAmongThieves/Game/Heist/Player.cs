using HonorAmongThieves.Hubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HonorAmongThieves.Game.Heist
{
    public class Player
    {
        public Room Room { get; set; }

        public string Name { get; private set; }

        public int NetWorth { get; set; } = 50;

        public int ProjectedNetworth { get; set; } = 10;

        public int TimeSpentInJail { get; set; } = 0;

        public int YearsLeftInJail { get; set; } = -1;

        public int MinJailSentence { get; set; } = 1;

        public int MaxJailSentence { get; set; } = 2;

        public int BetrayalCount { get; set; } = 0;

        public int LastBetrayedYear { get; set; } = -1;

        public Status CurrentStatus { get; set; }

        public Status PreviousStatus { get; set; }

        public DateTime LastUpdate { get; private set; }

        public string ConnectionId { get; set; }

        public HeistDecision Decision { get; set; } = new HeistDecision();

        public bool Okay { get; set; } = true;

        public bool IsBot { get; set; } = false;

        public Heist CurrentHeist;

        public Player(string name, Room room)
        {
            this.Name = name;
            this.Room = room;
            this.CurrentStatus = Status.WaitingForGameStart;
            this.LastUpdate = DateTime.UtcNow;
        }

        public void BlackmailDecision(Player victim)
        {
            this.Decision.DecisionMade = true;
            this.Decision.GoOnHeist = true;
            this.Decision.ReportPolice = false;
            this.Decision.PlayerToBlackmail = victim;

            this.Okay = true;
            this.CurrentStatus = Status.HeistDecisionMade;
        }

        public void MakeDecision(bool turnUp, bool snitch)
        {
            this.Decision.DecisionMade = true;
            this.Decision.GoOnHeist = turnUp;
            this.Decision.ReportPolice = snitch;

            this.Okay = true;
            this.CurrentStatus = Status.HeistDecisionMade;
        }

        public void GenerateFateMessage(bool heistHappens, bool policeReported)
        {
            ( this.Decision.FateTitle, this.Decision.FateDescription) = TextGenerator.GenerateFateMessage(heistHappens, policeReported, this.Decision);
            this.Decision.FateSummary = TextGenerator.GenerateFateSummary(this.Decision);
        }

        public async Task ResumePlayerSession(HeistHub hub)
        {
            if (this.Room.SigningUp)
            {
                await hub.JoinRoom_UpdateView(this.Room, this);
                return;
            }

            // This doesn't actually happen since the endgame_broadcast deletes the session state
            if (this.Room.CurrentYear == this.Room.MaxYears)
            {
                await hub.EndGame_Broadcast(this.Room, true);
                return;
            }

            await hub.StartRoom_UpdatePlayer(this);

            if (this.Room.CurrentStatus == Room.Status.ResolvingHeists)
            {
                await this.UpdateFateView(hub, !this.Okay /*Don't set the OKAY button if it has already been pressed*/);
                return;
            }

            if (this.Room.CurrentStatus == Room.Status.AwaitingHeistDecisions
                || this.Room.CurrentStatus == Room.Status.NoHeists)
            {
                switch (this.CurrentStatus)
                {
                    // Idle statuses
                    case Status.FindingHeist:
                    case Status.InJail:
                        await hub.UpdateIdleStatus(this, !this.Okay /*Don't set the OKAY button if it has already been pressed*/);
                        return;
                    case Status.InHeist:
                        await hub.HeistPrep_ChangeState(this.CurrentHeist, true);
                        return;
                    case Status.HeistDecisionMade:
                        await hub.HeistPrep_UpdateDecision(this);
                        return;
                    default:
                        // This should not happen
                        await hub.ShowError("ERROR RESUMING CURRENT STATE");
                        return;
                }
            }
        }

        public async Task UpdateFateLogic(HeistHub hub)
        {
            this.PreviousStatus = this.CurrentStatus;
            switch (this.CurrentStatus)
            {
                case Player.Status.InJail:
                    this.YearsLeftInJail--;
                    this.TimeSpentInJail++;
                    if (this.YearsLeftInJail <= 0)
                    {
                        this.CurrentStatus = Player.Status.FindingHeist;
                    }
                    break;

                case Player.Status.FindingHeist:
                    break;

                case Player.Status.HeistDecisionMade:
                    this.CurrentStatus = this.Decision.NextStatus;
                    break;
            }

            if (!this.IsBot)
            {
                await this.UpdateFateView(hub);
            }
        }

        private async Task UpdateFateView(HeistHub hub, bool setOkayButton = true)
        {
            // Update idle people with some more information
            switch (this.PreviousStatus)
            {
                case Player.Status.InJail:
                    if (this.YearsLeftInJail <= 0)
                    {
                        var message = TextGenerator.FreeFromJail;
                        await hub.UpdateHeistStatus(this, message.Item1, message.Item2, setOkayButton);
                    }
                    else
                    {
                        var message = TextGenerator.StillInJail;
                        await hub.UpdateHeistStatus(this, message.Item1, string.Format(message.Item2, this.YearsLeftInJail), setOkayButton);
                    }
                    break;

                case Player.Status.FindingHeist:
                    var vacationMessage = TextGenerator.VacationEnded;
                    await hub.UpdateHeistStatus(this, vacationMessage.Item1, vacationMessage.Item2, setOkayButton);
                    break;

                case Player.Status.HeistDecisionMade:
                    await hub.UpdateHeistStatus(this, this.Decision.FateTitle, this.Decision.FateDescription, setOkayButton);
                    if (this.Decision.GoOnHeist && this.Decision.FellowHeisters != null && this.Decision.FellowHeisters.Count > 0)
                    {
                        await hub.UpdateHeistMeetup(this, this.Decision.FellowHeisters);
                    }

                    if (!string.IsNullOrEmpty(this.Decision.FateSummary))
                    {
                        await hub.UpdateHeistSummary(this, this.Decision.FateSummary);
                    }

                    break;
            }
        }

        public void BotUpdateState()
        {
            this.Okay = true;

            if (this.CurrentStatus == Status.InHeist)
            {
                // TODO: Actual intelligence
                this.Decision.DecisionMade = true;
                this.CurrentStatus = Status.HeistDecisionMade;
            }
        }

        public enum Status
        {
            WaitingForGameStart,
            FindingHeist,
            InHeist,
            HeistDecisionMade,

            // Non-action states
            InJail,
            CleaningUp
        }

        public class HeistDecision
        {
            public bool DecisionMade { get; set; } = false;
            public bool GoOnHeist { get; set; } = true;
            public bool ReportPolice { get; set; } = false;
            public Player PlayerToBlackmail { get; set; } = null;

            // For decision resolution
            public string FateTitle { get; set; } = "";
            public string FateDescription { get; set; } = "";
            public List<Player> Blackmailers { get; set; }
            public List<Player> FellowHeisters { get; set; }
            public Status NextStatus { get; set; } = Status.FindingHeist;
            public string HeistSuccessMessage { get; set; }
            public string FateSummary { get; set; }

            public bool? ExtortionSuccessful { get; set; } = null;
            public bool? WasExtortedFrom { get; set; } = null;
            public int HeistReward = 0;
            public int JailFine = 0;
            public int BlackmailReward = 0;
            public int JailTerm = 0;
        }
    }
}
