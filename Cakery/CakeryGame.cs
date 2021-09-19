using Microsoft.AspNetCore.SignalR;


namespace HonorAmongThieves.Cakery.GameLogic
{
    public class CakeryGame
        : Game<CakeryHub, CakeryRoom, CakeryPlayer>
    {
        public CakeryGame(IHubContext<CakeryHub> hubContext)
               : base(hubContext) { }

        protected override CakeryRoom InstantiateRoom(string roomId, IHubContext<CakeryHub> hubContext)
        {
            return new CakeryRoom(roomId, hubContext);
        }
    }
}
