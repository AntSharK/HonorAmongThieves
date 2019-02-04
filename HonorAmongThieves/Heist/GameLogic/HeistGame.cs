using HonorAmongThieves.GameLogic;
using Microsoft.AspNetCore.SignalR;

namespace HonorAmongThieves.Heist.GameLogic
{
    public class HeistGame
        : Game<HeistHub, HeistRoom, HeistPlayer>
    {
        public HeistGame(IHubContext<HeistHub> hubContext)
            : base(hubContext) { }

        protected override HeistRoom InstantiateRoom(string roomId, IHubContext<HeistHub> hubContext)
        {
            return new HeistRoom(roomId, hubContext);
        }
    }
}
