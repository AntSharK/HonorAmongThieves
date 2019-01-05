﻿using HonorAmongThieves.Game.Heist;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace HeistTests
{
    [TestClass]
    public class UnitTests
    {
        private static Player harv;
        private static Player chee;
        private static Player ron;
        private static Player yc;
        private static Player sara;

        [TestInitialize]
        public void Initialize()
        {
            harv = new Player("HARVEY", null);
            chee = new Player("CHEEHOW", null);
            ron = new Player("RONWANG", null);
            yc = new Player("YUCHENG", null);
            sara = new Player("SARAH", null);
        }

        [TestMethod]
        public void HeistSuccess()
        {
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(chee);
            heist.AddPlayer(harv);
            heist.AddPlayer(ron);

            chee.Decision.GoOnHeist = true;
            harv.Decision.GoOnHeist = true;
            ron.Decision.GoOnHeist = false;

            var cheeStarting = chee.NetWorth;
            var harvStarting = harv.NetWorth;
            var ronStarting = ron.NetWorth;
            var reward = heist.TotalReward / 2;

            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            Assert.AreEqual(ronStarting, ron.NetWorth);
            Assert.AreEqual(cheeStarting + reward, chee.NetWorth);
            Assert.AreEqual(harvStarting + reward, harv.NetWorth);

            Assert.AreEqual(chee.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(ron.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(harv.Decision.NextStatus, Player.Status.FindingHeist);
        }

        [TestMethod]
        public void HeistAbandon()
        {
            var heist = new Heist("12345", 2 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(chee);
            heist.AddPlayer(harv);

            chee.Decision.GoOnHeist = false;
            harv.Decision.GoOnHeist = false;

            var cheeStarting = chee.NetWorth;
            var harvStarting = harv.NetWorth;

            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            Assert.AreEqual(cheeStarting, chee.NetWorth);
            Assert.AreEqual(harvStarting, harv.NetWorth);

            Assert.AreEqual(chee.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(harv.Decision.NextStatus, Player.Status.FindingHeist);
        }

        [TestMethod]
        public void HeistSnitchBailsAndBlackmailed()
        {
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(chee);
            heist.AddPlayer(harv);
            heist.AddPlayer(sara);

            chee.Decision.GoOnHeist = true;
            chee.Decision.PlayerToBlackmail = harv;
            sara.Decision.GoOnHeist = true;
            sara.Decision.PlayerToBlackmail = harv;
            harv.Decision.GoOnHeist = false;
            harv.Decision.ReportPolice = true;

            var cheeStarting = chee.NetWorth;
            var saraStarting = sara.NetWorth;
            var harvStarting = harv.NetWorth;

            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            Assert.IsTrue(chee.Decision.BlackmailReward > 0);
            Assert.AreEqual(cheeStarting + heist.TotalReward / 2 + chee.Decision.BlackmailReward, chee.NetWorth);
            Assert.AreEqual(saraStarting + heist.TotalReward / 2 + sara.Decision.BlackmailReward, sara.NetWorth);
            Assert.AreEqual(harvStarting - chee.Decision.BlackmailReward * 2, harv.NetWorth);

            Assert.AreEqual(chee.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(sara.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(harv.Decision.NextStatus, Player.Status.FindingHeist);
        }

        [TestMethod]
        public void HeistSnitchGoesOnHeistAndBlackmailed()
        {
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(chee);
            heist.AddPlayer(harv);
            heist.AddPlayer(sara);

            chee.Decision.GoOnHeist = true;
            chee.Decision.PlayerToBlackmail = harv;
            sara.Decision.GoOnHeist = true;
            sara.Decision.PlayerToBlackmail = harv;
            harv.Decision.GoOnHeist = true;
            harv.Decision.ReportPolice = true;

            var cheeStarting = chee.NetWorth;
            var saraStarting = sara.NetWorth;
            var harvStarting = harv.NetWorth;

            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            Assert.IsTrue(chee.Decision.BlackmailReward > 0);
            Assert.AreEqual(cheeStarting + heist.TotalReward / 3 + chee.Decision.BlackmailReward, chee.NetWorth);
            Assert.AreEqual(saraStarting + heist.TotalReward / 3 + sara.Decision.BlackmailReward, sara.NetWorth);
            Assert.AreEqual(harvStarting + heist.TotalReward / 3 - chee.Decision.BlackmailReward * 2, harv.NetWorth);

            Assert.AreEqual(chee.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(sara.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(harv.Decision.NextStatus, Player.Status.FindingHeist);
        }

        [TestMethod]
        public void BlackmailMissHeistAbandon()
        {
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(chee);
            heist.AddPlayer(harv);
            heist.AddPlayer(sara);

            chee.Decision.GoOnHeist = true;
            chee.Decision.PlayerToBlackmail = harv;
            sara.Decision.GoOnHeist = false;
            harv.Decision.GoOnHeist = false;

            var cheeStarting = chee.NetWorth;
            var saraStarting = sara.NetWorth;
            var harvStarting = harv.NetWorth;
            harv.BetrayalCount = 1;
            harv.LastBetrayedYear = 9;

            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            Assert.AreEqual(cheeStarting, chee.NetWorth);
            Assert.AreEqual(saraStarting, sara.NetWorth);
            Assert.AreEqual(harvStarting, harv.NetWorth);

            Assert.AreEqual(chee.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(sara.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(harv.Decision.NextStatus, Player.Status.FindingHeist);
        }

        [TestMethod]
        public void BlackmailMissHeistSuccess()
        {
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(chee);
            heist.AddPlayer(harv);
            heist.AddPlayer(sara);

            chee.Decision.GoOnHeist = true;
            chee.Decision.PlayerToBlackmail = harv;
            sara.Decision.GoOnHeist = true;
            harv.Decision.GoOnHeist = false;

            var cheeStarting = chee.NetWorth;
            var saraStarting = sara.NetWorth;
            var harvStarting = harv.NetWorth;
            harv.BetrayalCount = 1;
            harv.LastBetrayedYear = 9;

            heist.Resolve();
            this.VerifyNonBlankFate(heist);
            var reward = heist.TotalReward / 2;

            Assert.AreEqual(cheeStarting + reward, chee.NetWorth);
            Assert.AreEqual(saraStarting + reward, sara.NetWorth);
            Assert.AreEqual(harvStarting, harv.NetWorth);

            Assert.AreEqual(chee.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(sara.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(harv.Decision.NextStatus, Player.Status.FindingHeist);
        }

        [TestMethod]
        public void HeistSnitchSuccessAndSelfJail()
        {
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(chee);
            heist.AddPlayer(harv);
            heist.AddPlayer(sara);

            chee.Decision.GoOnHeist = true;
            sara.Decision.GoOnHeist = true;
            sara.Decision.ReportPolice = true;
            harv.Decision.GoOnHeist = false;
            harv.Decision.ReportPolice = true;

            var cheeStarting = chee.NetWorth;
            var saraStarting = sara.NetWorth;
            var harvStarting = harv.NetWorth;

            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            Assert.AreEqual(0, sara.Decision.JailFine);
            Assert.IsTrue(chee.Decision.JailTerm > 0);
            Assert.IsTrue(chee.Decision.JailFine > 0);
            Assert.IsTrue(sara.Decision.JailTerm > 0);
            Assert.AreEqual(0, harv.Decision.JailTerm);
            Assert.AreEqual(cheeStarting - chee.Decision.JailFine, chee.NetWorth);
            Assert.AreEqual(saraStarting + 5 /*SnitchReward / 2*/, sara.NetWorth);
            Assert.AreEqual(harvStarting + 5 /*SnitchReward / 2*/, harv.NetWorth);

            Assert.AreEqual(chee.Decision.NextStatus, Player.Status.InJail);
            Assert.AreEqual(chee.Decision.JailTerm, chee.YearsLeftInJail);
            Assert.AreEqual(sara.Decision.NextStatus, Player.Status.InJail);
            Assert.AreEqual(sara.Decision.JailTerm, sara.YearsLeftInJail);
            Assert.AreEqual(harv.Decision.NextStatus, Player.Status.FindingHeist);
        }

        [TestMethod]
        public void HeistAbandonSnitchFail()
        {
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(chee);
            heist.AddPlayer(harv);
            heist.AddPlayer(sara);

            chee.Decision.GoOnHeist = false;
            sara.Decision.GoOnHeist = true;
            sara.Decision.ReportPolice = true;
            harv.Decision.GoOnHeist = false;
            harv.Decision.ReportPolice = true;

            var cheeStarting = chee.NetWorth;
            var saraStarting = sara.NetWorth;
            var harvStarting = harv.NetWorth;

            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            Assert.AreEqual(0, chee.Decision.JailTerm);
            Assert.IsTrue(sara.Decision.JailTerm > 0);
            Assert.IsTrue(harv.Decision.JailTerm > 0);
            Assert.IsTrue(sara.Decision.JailFine > 0);
            Assert.IsTrue(harv.Decision.JailFine > 0);
            Assert.AreEqual(cheeStarting, chee.NetWorth);
            Assert.AreEqual(saraStarting - sara.Decision.JailFine, sara.NetWorth);
            Assert.AreEqual(harvStarting - harv.Decision.JailFine, harv.NetWorth);

            Assert.AreEqual(chee.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(sara.Decision.NextStatus, Player.Status.InJail);
            Assert.AreEqual(sara.Decision.JailTerm, sara.YearsLeftInJail);
            Assert.AreEqual(harv.Decision.NextStatus, Player.Status.InJail);
            Assert.AreEqual(harv.Decision.JailTerm, harv.YearsLeftInJail);
        }

        [TestMethod]
        public void HeistAbandonBlackmailFail()
        {
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(chee);
            heist.AddPlayer(harv);
            heist.AddPlayer(sara);
            heist.AddPlayer(ron);
            heist.AddPlayer(yc);

            chee.Decision.GoOnHeist = true;
            chee.Decision.PlayerToBlackmail = yc;
            harv.Decision.GoOnHeist = false;
            sara.Decision.GoOnHeist = false;
            ron.Decision.GoOnHeist = false;
            yc.Decision.GoOnHeist = true;
            yc.Decision.ReportPolice = false;

            // This should result in a FAILED blackmail
            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            var startingGold = sara.NetWorth;
            Assert.AreEqual(startingGold, harv.NetWorth);
            Assert.AreEqual(startingGold, sara.NetWorth);
            Assert.AreEqual(startingGold, ron.NetWorth);
            Assert.AreEqual(startingGold, yc.NetWorth);

            // Everyone else is normal - Chee is in jail
            Assert.IsTrue(chee.Decision.JailFine > 0);
            Assert.IsTrue(chee.Decision.JailTerm > 0);
            Assert.AreEqual(chee.Decision.JailTerm, chee.YearsLeftInJail);
            Assert.AreEqual(chee.Decision.NextStatus, Player.Status.InJail);
            Assert.AreEqual(startingGold - chee.Decision.JailFine, chee.NetWorth);

            Assert.AreEqual(sara.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(harv.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(ron.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(yc.Decision.NextStatus, Player.Status.FindingHeist);
        }

        [TestMethod]
        public void HeistAbandonBlackmailSuccess()
        {
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(chee);
            heist.AddPlayer(harv);
            heist.AddPlayer(sara);
            heist.AddPlayer(ron);
            heist.AddPlayer(yc);

            chee.Decision.GoOnHeist = true;
            chee.Decision.PlayerToBlackmail = yc;
            harv.Decision.GoOnHeist = false;
            sara.Decision.GoOnHeist = false;
            ron.Decision.GoOnHeist = false;
            yc.Decision.GoOnHeist = true;
            yc.Decision.ReportPolice = false;
            yc.LastBetrayedYear = 9;
            yc.BetrayalCount = 1;

            // This should result in a SUCCESSFUL blackmail
            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            var startingGold = sara.NetWorth;
            Assert.AreEqual(startingGold, harv.NetWorth);
            Assert.AreEqual(startingGold, sara.NetWorth);
            Assert.AreEqual(startingGold, ron.NetWorth);

            Assert.IsTrue(chee.Decision.BlackmailReward > 0);
            Assert.AreEqual(startingGold + chee.Decision.BlackmailReward, chee.NetWorth);
            Assert.AreEqual(startingGold - chee.Decision.BlackmailReward, yc.NetWorth);

            Assert.AreEqual(chee.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(sara.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(harv.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(ron.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(yc.Decision.NextStatus, Player.Status.FindingHeist);
        }

        [TestMethod]
        public void HeistArrestedBlackmailFail()
        {
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(ron);
            heist.AddPlayer(yc);

            ron.Decision.GoOnHeist = true;
            ron.Decision.ReportPolice = true;
            ron.Decision.PlayerToBlackmail = yc;
            ron.MinJailSentence = 2;
            ron.MaxJailSentence = 3;
            yc.Decision.GoOnHeist = true;
            yc.Decision.ReportPolice = false;
            yc.LastBetrayedYear = 4; // Before the snitch window
            yc.BetrayalCount = 1;

            var startingNetworth = yc.NetWorth;
            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            // Assert that ron has been sentenced twice
            Assert.AreEqual(ron.Decision.NextStatus, Player.Status.InJail);
            Assert.IsTrue(ron.Decision.JailTerm >= 4);
            Assert.IsTrue(ron.Decision.JailFine > 0);
            Assert.AreEqual(startingNetworth - ron.Decision.JailFine + 10 /*Snitch reward*/, ron.NetWorth);

            Assert.AreEqual(yc.Decision.NextStatus, Player.Status.InJail);
            Assert.AreEqual(startingNetworth - yc.Decision.JailFine, yc.NetWorth);
        }

        [TestMethod]
        public void HeistArrestedBlackmailSuccess()
        {
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(ron);
            heist.AddPlayer(yc);

            ron.Decision.GoOnHeist = true;
            ron.Decision.ReportPolice = true;
            ron.Decision.PlayerToBlackmail = yc;
            ron.MinJailSentence = 2;
            ron.MaxJailSentence = 3;
            yc.Decision.GoOnHeist = true;
            yc.Decision.ReportPolice = false;
            yc.LastBetrayedYear = 5; // After the snitch window
            yc.BetrayalCount = 1;

            var startingNetworth = yc.NetWorth;
            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            // Assert that ron has been sentenced twice
            Assert.AreEqual(ron.Decision.NextStatus, Player.Status.InJail);
            Assert.IsTrue(ron.Decision.JailTerm < 4);
            Assert.IsTrue(ron.Decision.JailFine == 0);
            Assert.IsTrue(ron.Decision.BlackmailReward > 0);
            Assert.AreEqual(startingNetworth + 10 /*Snitch reward*/ + ron.Decision.BlackmailReward, ron.NetWorth);

            Assert.AreEqual(yc.Decision.NextStatus, Player.Status.InJail);
            Assert.AreEqual(startingNetworth - yc.Decision.JailFine - ron.Decision.BlackmailReward, yc.NetWorth);
        }

        [TestMethod]
        public void HeistSuccessBlackmailFail()
        {
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(ron);
            heist.AddPlayer(yc);

            ron.Decision.GoOnHeist = true;
            ron.Decision.PlayerToBlackmail = yc;
            yc.Decision.GoOnHeist = true;
            yc.Decision.ReportPolice = false;

            var startingNetworth = yc.NetWorth;
            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            Assert.AreEqual(ron.Decision.NextStatus, Player.Status.InJail);
            Assert.AreEqual(ron.Decision.JailTerm, ron.YearsLeftInJail);
            Assert.IsTrue(ron.Decision.JailFine > 0);
            Assert.AreEqual(heist.TotalReward / 2, ron.Decision.HeistReward);
            Assert.AreEqual(startingNetworth + ron.Decision.HeistReward - ron.Decision.JailFine, ron.NetWorth);
            Assert.AreEqual(yc.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(startingNetworth + heist.TotalReward / 2, yc.NetWorth);
        }

        [TestMethod]
        public void HeistSuccessBlackmailSuccess()
        {
            var heist = new Heist("12345", 5 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(ron);
            heist.AddPlayer(yc);
            heist.AddPlayer(sara);
            heist.AddPlayer(chee);
            heist.AddPlayer(harv);

            ron.Decision.GoOnHeist = true;
            ron.Decision.PlayerToBlackmail = yc;
            yc.Decision.GoOnHeist = true;
            yc.Decision.ReportPolice = false;
            yc.LastBetrayedYear = 5; // Within window of snitching
            yc.BetrayalCount = 2;

            var startingNetworth = yc.NetWorth;
            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            Assert.AreEqual(ron.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(heist.TotalReward / 5, ron.Decision.HeistReward);
            Assert.AreEqual(startingNetworth + ron.Decision.HeistReward + ron.Decision.BlackmailReward, ron.NetWorth);
            Assert.AreEqual(yc.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(startingNetworth + heist.TotalReward / 5 - ron.Decision.BlackmailReward, yc.NetWorth);
            var originalBlackmailRewardWithSuccessfulHeist = ron.Decision.BlackmailReward;

            // Ensure that if the heist didn't happen on a successful blackmail, the reward is much less
            heist = new Heist("12345", 5 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(ron);
            heist.AddPlayer(yc);
            heist.AddPlayer(sara);
            heist.AddPlayer(chee);
            heist.AddPlayer(harv);
            yc.NetWorth = startingNetworth;
            ron.NetWorth = startingNetworth;
            chee.Decision.GoOnHeist = false;
            sara.Decision.GoOnHeist = false;
            harv.Decision.GoOnHeist = false;

            ron.Decision = new Player.HeistDecision();
            ron.Decision.GoOnHeist = true;
            ron.Decision.PlayerToBlackmail = yc;
            yc.Decision = new Player.HeistDecision();
            yc.Decision.GoOnHeist = true;
            yc.Decision.ReportPolice = false;
            yc.LastBetrayedYear = 5; // Within window of snitching
            yc.BetrayalCount = 2;

            heist.Resolve();
            Assert.IsTrue(ron.Decision.BlackmailReward < originalBlackmailRewardWithSuccessfulHeist);
            this.VerifyNonBlankFate(heist);
            Assert.AreEqual(0, ron.Decision.HeistReward);
            Assert.AreEqual(ron.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(startingNetworth + ron.Decision.BlackmailReward, ron.NetWorth);
            Assert.AreEqual(yc.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(startingNetworth - ron.Decision.BlackmailReward, yc.NetWorth);
        }

        [TestMethod]
        public void ChainBlackmailSuccess()
        {
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(ron);
            heist.AddPlayer(yc);
            heist.AddPlayer(chee);

            ron.Decision.GoOnHeist = true;
            ron.Decision.PlayerToBlackmail = yc;
            ron.LastBetrayedYear = 9;
            ron.BetrayalCount = 2;
            yc.Decision.GoOnHeist = true;
            yc.Decision.PlayerToBlackmail = ron;
            yc.LastBetrayedYear = 9;
            yc.BetrayalCount = 2;

            var startingNetworth = yc.NetWorth;
            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            Assert.AreEqual(ron.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(yc.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(ron.NetWorth, yc.NetWorth); // Blackmails are resolved equally
            Assert.AreEqual(yc.NetWorth, chee.NetWorth);
        }

        [TestMethod]
        public void DefenseAndSuccessfulBlackmail()
        {
            var heist = new Heist("12345", 2 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(ron);
            heist.AddPlayer(yc);

            ron.Decision.GoOnHeist = true;
            ron.Decision.PlayerToBlackmail = yc;
            ron.LastBetrayedYear = 9;
            ron.BetrayalCount = 2;
            yc.Decision.GoOnHeist = true;
            yc.Decision.PlayerToBlackmail = ron;
            yc.LastBetrayedYear = 4; // Out of snitch window
            yc.BetrayalCount = 2;

            var startingNetworth = yc.NetWorth;
            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            Assert.AreEqual(ron.Decision.NextStatus, Player.Status.InJail);
            Assert.AreEqual(yc.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.IsTrue(yc.Decision.BlackmailReward > 0);
            Assert.AreEqual(startingNetworth + heist.TotalReward / 2 + yc.Decision.BlackmailReward, yc.NetWorth);
            Assert.IsTrue(ron.Decision.JailFine > 0);
            Assert.AreEqual(startingNetworth + heist.TotalReward / 2 - ron.Decision.JailFine - yc.Decision.BlackmailReward, ron.NetWorth);
            Assert.IsTrue(ron.Decision.JailTerm > 0);
        }

        [TestMethod]
        public void DefenseAndFailedBlackmail()
        {
            var heist = new Heist("12345", 2 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/, 0 /*NetworthFudge*/, 60 /*BlackmailReward*/, 20 /*JailFine*/);
            heist.AddPlayer(ron);
            heist.AddPlayer(yc);

            ron.Decision.GoOnHeist = true;
            ron.Decision.PlayerToBlackmail = yc;
            ron.LastBetrayedYear = 4; // Out of snitch window
            ron.BetrayalCount = 2;
            yc.Decision.GoOnHeist = true;
            yc.Decision.PlayerToBlackmail = ron;
            yc.LastBetrayedYear = 4; // Out of snitch window
            yc.BetrayalCount = 2;

            var startingNetworth = yc.NetWorth;
            heist.Resolve();
            this.VerifyNonBlankFate(heist);

            Assert.AreEqual(ron.Decision.NextStatus, Player.Status.InJail);
            Assert.AreEqual(yc.Decision.NextStatus, Player.Status.InJail);
            Assert.IsTrue(ron.Decision.JailFine > 0);
            Assert.AreEqual(startingNetworth + heist.TotalReward / 2 - ron.Decision.JailFine, ron.NetWorth);
            Assert.IsTrue(ron.Decision.JailTerm > 0);
            Assert.IsTrue(yc.Decision.JailFine > 0);
            Assert.AreEqual(startingNetworth + heist.TotalReward / 2 - yc.Decision.JailFine, yc.NetWorth);
            Assert.IsTrue(yc.Decision.JailTerm > 0);
        }

        private void VerifyNonBlankFate(Heist heist)
        {
            foreach (var player in heist.Players.Values)
            {
                Assert.IsTrue(player.Decision.FateTitle.Length > 0, "{0}'s Fate Title is empty.", player.Name);
                Assert.IsTrue(player.Decision.FateDescription.Length > 0, "{0}'s Fate Decision is empty.", player.Name);
            }
        }
    }
}
