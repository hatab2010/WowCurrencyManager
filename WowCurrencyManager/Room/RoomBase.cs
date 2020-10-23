using Discord.WebSocket;
using System.Collections.Generic;

namespace WowCurrencyManager.Room
{
    public class RoomBase
    {
        public ISocketMessageChannel Channel { protected set; get; }
        public string Name => Channel.Name;
    }
}
