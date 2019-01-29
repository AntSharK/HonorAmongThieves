using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorAmongThieves.Pages
{
    public class IndexModel : PageModel
    {
        public List<Tuple<string, DateTime, DateTime, int, string>> HeistLobbies;

        public void OnGet()
        {
            HeistLobbies = new List<Tuple<string, DateTime, DateTime, int, string>>();
            foreach (var room in Heist.GameLogic.Lobby.Rooms.Values)
            {
                HeistLobbies.Add(Tuple.Create(room.Id, room.CreatedTime, room.UpdatedTime, room.Players.Count, room.SigningUp ? "SIGNING UP" : "STARTED"));
            }
        }
    }
}