using HonorAmongThieves.Game;
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
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/);
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
            var heist = new Heist("12345", 2 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/);
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
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/);
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
            Assert.AreEqual(cheeStarting + heist.TotalReward/2 + chee.Decision.BlackmailReward, chee.NetWorth);
            Assert.AreEqual(saraStarting + heist.TotalReward/2 + sara.Decision.BlackmailReward, sara.NetWorth);
            Assert.AreEqual(harvStarting - chee.Decision.BlackmailReward * 2, harv.NetWorth);

            Assert.AreEqual(chee.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(sara.Decision.NextStatus, Player.Status.FindingHeist);
            Assert.AreEqual(harv.Decision.NextStatus, Player.Status.FindingHeist);
        }

        [TestMethod]
        public void HeistSnitchGoesOnHeistAndBlackmailed()
        {
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/);
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
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/);
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
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/);
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
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/);
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
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*Year*/, 5 /*SnitchWindow*/);
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

        // public void HeistAbandonBlackmailFail()

        // public void HeistAbandonBlackmailSuccess()

        // public void HeistArrestedBlackmailFail()

        // public void HeistArrestedBlackmailSuccess()

        // public void HeistSuccessBlackmailFail()

        // public void HeistSuccessBlackmailSuccess()

        // public void ChainBlackmailSuccess()

        // public void DefenseAndSuccessfulBlackmail()

        // public void DefenseAndFailedBlackmail()

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
