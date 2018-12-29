using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HonorAmongThieves.Game
{
    public class Player
    {
        public Room Room { get; set; }

        public string Name { get; private set; }

        public int NetWorth { get; set; } = 10;

        public int TimeSpentInJail { get; set; } = 0;

        public int YearsLeftInJail { get; set; } = -1;

        public int MinJailSentence { get; set; } = 1;

        public int MaxJailSentence { get; set; } = 2;

        public int BetrayalCount { get; set; } = 0;

        public Status CurrentStatus { get; set; }

        public Status NextStatus { get; set; }

        public DateTime LastUpdate { get; private set; }

        public string ConnectionId { get; set; }

        public HeistDecision Decision { get; set; } = new HeistDecision();

        public bool Okay { get; set; } = true;

        public Player(string name, Room room)
        {
            this.Name = name;
            this.Room = room;
            this.CurrentStatus = Status.WaitingForGameStart;
            this.LastUpdate = DateTime.UtcNow;
        }

        public void MurderDecision(Player victim)
        {
            this.Decision.DecisionMade = true;
            this.Decision.GoOnHeist = true;
            this.Decision.ReportPolice = false;
            this.Decision.PlayerToKill = victim;

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

        public void GenerateFateMessage()
        {
            if (this.Decision.NextStatus == Status.Dead)
            {
                this.Decision.FateTitle = "DEAD";
                this.Decision.FateDescription = "You got killed! ";

                if (this.Decision.PlayerToKill != null
                    && this.Decision.Killers.Count == 1
                    && this.Decision.Killers.Contains(this))
                {
                    this.Decision.FateDescription += "You confronted " + this.Decision.PlayerToKill.Name + ". Things got heated, and someone lost their head. Unfortunately, that someone was you.";
                    return;
                }

                if (this.Decision.GoOnHeist)
                {
                    this.Decision.FateDescription += "You were confronted at the heist. Things got heated, and the evidence against you piled up. As you ran away, you went down with an unfortunate case of a bullet in the brain.";
                    return;

                }
                else
                {
                    this.Decision.FateDescription += "You snuck around to snitch on the ongoing heist, and noticed that something wasn't right. But too late. Someone snuck behind you and lobbed off your head.";
                    return;
                }
            }

            if (this.Decision.ReportPolice)
            {
                if (!this.Decision.HeistHappens)
                {
                    this.Decision.FateTitle = "ARRESTED FOR MISLEADING POLICE";
                    this.Decision.FateDescription = "You reported a heist to the police, but the heist didn't happen. So you got arrested for wasting their time. You also got fined for $" + -this.Decision.NetworthChange + " MILLION.";
                    return;
                }
                else
                {
                    if (this.Decision.GoOnHeist)
                    {
                        this.Decision.FateTitle = "ARRESTED WITH REDUCED PENALTY";
                        this.Decision.FateDescription = "You went on the heist, but also reported the details to the police. They threw you and your heistmates in jail, and rewarded you with $" + this.Decision.NetworthChange + " MILLION.";
                        return;
                    }
                    else
                    {
                        this.Decision.FateTitle = "SUCCESSFULLY SNITCHED";
                        this.Decision.FateDescription = "You stalked your heistmates as they committing the heist, and reported the details to the police. They threw your heistmates in jail, and rewarded you for $" + this.Decision.NetworthChange + " MILLION.";
                        return;
                    }
                }
            }

            if (this.Decision.PlayerToKill != null
                && this.Decision.PlayerToKill.NextStatus == Status.Dead)
            {
                this.Decision.FateTitle = "KILLED " + this.Decision.PlayerToKill.Name + ", ";
                if (this.Decision.PlayerToKill.Decision.Killers.Count > 1)
                {
                    this.Decision.FateDescription = "You found " + this.Decision.PlayerToKill.Name + " and snuck up behind him. But before you could kill him, a shot rang out and he fell to the floor. You swung by later to check that he was really date, and stole some of his credit cards. ";
                }
                else
                {
                    this.Decision.FateDescription = "You found " + this.Decision.PlayerToKill.Name + " and put a bullet between his eyes. Then you swiftly emptied his bank account into yours. ";
                }
            }

            if (this.Decision.GoOnHeist)
            {
                if (!this.Decision.HeistHappens)
                {
                    this.Decision.FateTitle += "NOT ENOUGH ATTENDENCE";
                    this.Decision.FateDescription += "You gather for the heist, but there aren't enough people here to safely complete the job. So you all go home without trying. Your total networth change this year: $" + this.Decision.NetworthChange + " MILLION.";
                    return;
                }
                else
                {
                    if (this.Decision.PoliceReported)
                    {
                        this.Decision.FateTitle += "ARRESTED";
                        this.Decision.FateDescription += "You are arrested during the heist! You are sent to jail and fined! Your total networth change this year: $" + this.Decision.NetworthChange + " MILLION.";
                        return;
                    }
                    else
                    {
                        this.Decision.FateTitle += "SUCCESSFUL HEIST";
                        this.Decision.FateDescription += "Successful heist! Your total networth change this year: $" + this.Decision.NetworthChange + " MILLION.";
                        return;
                    }
                }
            }

            if (!this.Decision.GoOnHeist
                && !this.Decision.ReportPolice)
            {
                this.Decision.FateTitle = "TAKING A VACATION";
                this.Decision.FateDescription = "You spend the year doing other fun exciting things. Much like crime, but without the downside of getting arrested.";
                return;
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
            Dead,
            CleaningUp
        }

        public class HeistDecision
        {
            public bool DecisionMade { get; set; } = false;
            public bool GoOnHeist { get; set; } = true;
            public bool ReportPolice { get; set; } = false;
            public Player PlayerToKill { get; set; } = null;

            // For decision resolution
            public int NetworthChange { get; set; } = 0;
            public string FateTitle { get; set; } = "";
            public string FateDescription { get; set; } = "";
            public List<Player> FellowHeisters { get; set; }
            public List<Player> Killers { get; set; }
            public bool HeistHappens { get; set; }
            public bool PoliceReported { get; set; }
            public Status NextStatus { get; set; }
        }
    }
}
