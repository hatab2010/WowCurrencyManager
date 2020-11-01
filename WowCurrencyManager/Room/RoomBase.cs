using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace WowCurrencyManager.Room
{
    [Serializable]
    public class RoomBase
    {
        public ISocketMessageChannel Channel { protected set; get; }
        public string Name => Channel.Name;
    }
}
