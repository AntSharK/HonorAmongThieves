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
            var heist = new Heist("12345", 3 /*Capacity*/, 10 /*SnitchReward*/, 10 /*MaxYears*/, 5 /*SnitchWindow*/);
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
            var heist = new Heist("12345", 2 /*Capacity*/, 10 /*SnitchReward*/, 10 /*MaxYears*/, 5 /*SnitchWindow*/);
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
