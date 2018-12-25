using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorAmongThieves.Pages
{
    public class IndexModel : PageModel
    {
        public List<Tuple<string, DateTime, DateTime>> HeistLobbies;

        public void OnGet()
        {
            HeistLobbies = new List<Tuple<string, DateTime, DateTime>>();
            foreach (var room in Program.Instance.Rooms.Values)
            {
                HeistLobbies.Add(Tuple.Create(room.Id, room.CreatedTime, room.UpdatedTime));
            }
        }
    }
}