using HonorAmongThieves.Hubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HonorAmongThieves.Game
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

        public void GenerateFateMessage(bool heistHappens, bool policeReported, List<Player> heisters)
        {
            this.Decision.FateTitle = "TODO";
            this.Decision.FateDescription = "TODO";
            //// TODO: Resolve blackmailing.
            //if (this.Decision.NextStatus == Status.Dead)
            //{
            //    var message = TextGenerator.DeathMessage(this);
            //    this.Decision.FateTitle = this.Decision.FateTitle + message.Item1;
            //    this.Decision.FateDescription = this.Decision.FateDescription + message.Item2;
            //}
            //
            //// TODO: Handle case where successful murder doesn't leave 
            //if (this.Decision.NextStatus != Status.Dead
            //    && this.Decision.Killers?.Count > 0)
            //{
            //    var message = TextGenerator.DeathMessage(this);
            //    this.Decision.FateTitle = this.Decision.FateTitle + message.Item1;
            //    this.Decision.FateDescription = this.Decision.FateDescription + message.Item2;
            //}
            //
            //if (this.Decision.ReportPolice)
            //{
            //    if (!this.Decision.HeistHappens)
            //    {
            //        this.Decision.FateTitle = this.Decision.FateTitle + "ARRESTED FOR MISLEADING POLICE";
            //        this.Decision.FateDescription = this.Decision.FateDescription + "You reported a heist to the police, but the heist didn't happen. So you got arrested for wasting their time. You also got fined for $" + -this.Decision.NetworthChange + " MILLION.";
            //        return;
            //    }
            //    else
            //    {
            //        if (this.Decision.GoOnHeist)
            //        {
            //            this.Decision.FateTitle = this.Decision.FateTitle + "ARRESTED WITH REDUCED PENALTY";
            //            this.Decision.FateDescription = this.Decision.FateDescription + "You went on the heist, but also reported the details to the police. They threw you and your heistmates in jail, and rewarded you with $" + this.Decision.NetworthChange + " MILLION.";
            //            return;
            //        }
            //        else
            //        {
            //            this.Decision.FateTitle = this.Decision.FateTitle + "SUCCESSFULLY SNITCHED";
            //            this.Decision.FateDescription = this.Decision.FateDescription + "You stalked your heistmates as they committing the heist, and reported the details to the police. They threw your heistmates in jail, and rewarded you for $" + this.Decision.NetworthChange + " MILLION.";
            //            return;
            //        }
            //    }
            //}
            //
            //if (this.Decision.PlayerToKill != null
            //    && this.Decision.PlayerToKill.Decision.NextStatus == Status.Dead)
            //{
            //    this.Decision.FateTitle = this.Decision.FateTitle + "KILLED " + this.Decision.PlayerToKill.Name + ", ";
            //    if (this.Decision.PlayerToKill.Decision.Killers.Count > 1)
            //    {
            //        this.Decision.FateDescription = this.Decision.FateDescription + "You found " + this.Decision.PlayerToKill.Name + " and snuck up behind him. But before you could kill him, a shot rang out and he fell to the floor. You swung by later to check that he was really date, and stole some of his credit cards. ";
            //    }
            //    else
            //    {
            //        this.Decision.FateDescription = this.Decision.FateDescription + "You found " + this.Decision.PlayerToKill.Name + " and put a bullet between his eyes. Then you swiftly emptied his bank account into yours. ";
            //    }
            //}
            //else if (this.Decision.PlayerToKill != null
            //    && this.Decision.PlayerToKill.Decision.NextStatus != Status.Dead
            //    && !this.Decision.PlayerToKill.Decision.GoOnHeist)
            //{
            //    this.Decision.FateTitle = this.Decision.FateTitle + "COULD NOT FIND " + this.Decision.PlayerToKill.Name + ", ";
            //    this.Decision.FateDescription = this.Decision.FateDescription + "Your contacts found " + this.Decision.PlayerToKill.Name + " halfway around the world, and you decide murder is too troublesome this time. ";
            //}
            //
            //if (this.Decision.GoOnHeist)
            //{
            //    if (!this.Decision.HeistHappens)
            //    {
            //        this.Decision.FateTitle = this.Decision.FateTitle + "NOT ENOUGH ATTENDENCE";
            //        this.Decision.FateDescription = this.Decision.FateDescription + "You gather for the heist, but there aren't enough people here to safely complete the job. So you all go home without trying. Your total networth change this year: $" + this.Decision.NetworthChange + " MILLION.";
            //    }
            //    else
            //    {
            //        if (this.Decision.PoliceReported)
            //        {
            //            this.Decision.FateTitle = this.Decision.FateTitle + "ARRESTED";
            //            this.Decision.FateDescription = this.Decision.FateDescription + "You are arrested during the heist! You are sent to jail and fined! Your total networth change this year: " 
            //                + (this.Decision.NetworthChange > 0 ? ("$" + this.Decision.NetworthChange) : ("-$" + -this.Decision.NetworthChange)) + " MILLION.";
            //        }
            //        else
            //        {
            //            this.Decision.FateTitle = this.Decision.FateTitle + "SUCCESSFUL HEIST";
            //            this.Decision.FateDescription = this.Decision.FateDescription + "Successful heist! Your total networth change this year: $" + this.Decision.NetworthChange + " MILLION.";
            //        }
            //    }
            //}
            //
            //if (this.Decision.KillFailure
            //    && this.Room.SnitchMurderWindow >= 0
            //    && !this.Decision.PoliceReported)
            //{
            //    // If we reach this point, that means that failure to kill results in jail, not death
            //    this.Decision.FateTitle = this.Decision.FateTitle + " AND LEFT OUT TO DRY";
            //    this.Decision.FateDescription = this.Decision.FateDescription + " You confronted " + this.Decision.PlayerToKill.Name + ". Things got heated, you were left on the floor bleeding, for the cops to arrest.";
            //    return;
            //}
            //
            //else if (this.Decision.KillFailure
            //    && this.Room.SnitchMurderWindow >= 0
            //    && !this.Decision.PoliceReported)
            //{
            //    // If we reach this point, that means that failure to kill results in jail, not death
            //    this.Decision.FateTitle = this.Decision.FateTitle + " AND FAILED TO KILL";
            //    this.Decision.FateDescription = this.Decision.FateDescription + " You confronted " + this.Decision.PlayerToKill.Name + " as you were being arrested, but your accusations were refuted.";
            //    return;
            //}
            //
            //if (!this.Decision.GoOnHeist
            //    && !this.Decision.ReportPolice)
            //{
            //    this.Decision.FateTitle = "TAKING A VACATION";
            //    this.Decision.FateDescription = "You spend the year doing other fun exciting things. Much like crime, but without the downside of getting arrested.";
            //    return;
            //}
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
            // TODO: At this stage, heists will have been resolved
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
                        await hub.UpdateHeistStatus(this, message.Item1, string.Format(message.Item2, this.YearsLeftInJail));
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

            public bool? ExtortionSuccessful { get; set; } = null;
            public bool? WasExtortedFrom { get; set; } = null;
            public int HeistReward = 0;
            public int BlackmailReward = 0;
            public int JailTerm = 0;
        }
    }
}
