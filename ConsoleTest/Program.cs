using System;
using System.Collections.Generic;
using Necronomnomnom;
using Necronomnomnom.Cards;
using Necronomnomnom.Monsters;

namespace ConsoleTest
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Begin");

            var p1 = new Player();
            var p2 = new Player();

            var dungeon = new Dungeon() { Players = new List<Player>() { p1, p2 } };

            p1.GiveCard(new DamageMonster());
            p1.GiveCard(new DamageMonster());
            p1.GiveCard(new Amplify());
            p2.GiveCard(new DamageMonster());
            p2.GiveCard(new DamageMonster());
            p2.GiveCard(new Amplify());

            var monster = new RiceMonster();
            dungeon.CurrentBattle = new Battle() { Players = dungeon.Players, CurrentEnemy = monster }; 
            dungeon.CurrentBattle.CurrentTurn = new RoundState(6, dungeon.Players);

            // Test card shuffling
            p1.RefreshHand();
            p1.RefreshHand();
            p1.RefreshHand();
            p1.RefreshHand();
            p1.RefreshHand();

            // Play some player cards
            dungeon.CurrentBattle.CurrentTurn.Cards[0] = p1.Cards[0];
            dungeon.CurrentBattle.CurrentTurn.Cards[1] = p1.Cards[1];
            dungeon.CurrentBattle.CurrentTurn.Cards[2] = p1.Cards[2];
            dungeon.CurrentBattle.CurrentTurn.Cards[3] = p2.Cards[2];
            dungeon.CurrentBattle.CurrentTurn.Cards[4] = p2.Cards[1];

            // Play some monster cards
            dungeon.CurrentBattle.CurrentTurn.Cards[0] = monster.Cards[0];

            // Finish playing cards. Evaluate.
            dungeon.CurrentBattle.FinishCurrentTurn();
            Console.WriteLine("Finished");
        }
    }
}
