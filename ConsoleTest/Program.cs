using System;
using System.Collections.Generic;
using Necronomnomnom;
using Necronomnomnom.Cards;

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

            dungeon.CurrentBattle = new Battle() { Players = dungeon.Players }; 
            dungeon.CurrentBattle.CurrentTurn = new RoundState(6, dungeon.Players);

            // Play some cards
            dungeon.CurrentBattle.CurrentTurn.Cards[0] = p1.Cards[0];
            dungeon.CurrentBattle.CurrentTurn.Cards[1] = p1.Cards[1];
            dungeon.CurrentBattle.CurrentTurn.Cards[2] = p1.Cards[2];
            dungeon.CurrentBattle.CurrentTurn.Cards[3] = p2.Cards[2];
            dungeon.CurrentBattle.CurrentTurn.Cards[4] = p2.Cards[1];

            // Finish playing cards. Evaluate.
            dungeon.CurrentBattle.CurrentTurn.EvaluateCards();
            Console.WriteLine("Finished");
        }
    }
}
