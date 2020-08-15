using Discord.WebSocket;
using System.Collections.Generic;

namespace WowCurrencyManager.Room
{
    public class RoomBase
    {
        public List<RoomClient> Clients = new List<RoomClient>();
        public ISocketMessageChannel Channel { protected set; get; }
        public string Name => Channel.Name;

        protected int _balance;
    }
}
