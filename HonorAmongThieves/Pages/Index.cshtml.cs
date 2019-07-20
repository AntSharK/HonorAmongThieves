using System;
using System.Collections.Generic;
using HonorAmongThieves.Cakery.GameLogic;
using HonorAmongThieves.Heist.GameLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorAmongThieves.Pages
{
    public class IndexModel : PageModel
    {
        public List<Tuple<string, DateTime, int, string>> HeistRooms;
        public List<Tuple<string, DateTime, int, string>> CakeryRooms;

        public void OnGet(
            [FromServices] HeistGame heistLobby,
            [FromServices] CakeryGame cakeryLobby)
        {
            HeistRooms = new List<Tuple<string, DateTime, int, string>>();
            foreach (var room in heistLobby.Rooms.Values)
            {
                HeistRooms.Add(Tuple.Create(room.Id, room.UpdatedTime, room.Players.Count, room.SettingUp ? "SIGNING UP" : "STARTED"));
            }

            CakeryRooms = new List<Tuple<string, DateTime, int, string>>();
            foreach (var room in cakeryLobby.Rooms.Values)
            {
                CakeryRooms.Add(Tuple.Create(room.Id, room.UpdatedTime, room.Players.Count, room.SettingUp ? "SIGNING UP" : "STARTED"));
            }
        }
    }
}