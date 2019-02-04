using System;
using System.Collections.Generic;
using HonorAmongThieves.Heist.GameLogic;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorAmongThieves.Pages
{
    public class IndexModel : PageModel
    {
        public List<Tuple<string, DateTime, int, string>> HeistRooms;

        /*public void OnGet(HeistGame heistLobby)
        {
            HeistRooms = new List<Tuple<string, DateTime, int, string>>();
            foreach (var room in heistLobby.Rooms.Values)
            {
                HeistRooms.Add(Tuple.Create(room.Id, room.UpdatedTime, room.Players.Count, room.SettingUp ? "SIGNING UP" : "STARTED"));
            }
        }*/

        public void OnGet()
        {
            HeistRooms = new List<Tuple<string, DateTime, int, string>>();
        }
    }
}