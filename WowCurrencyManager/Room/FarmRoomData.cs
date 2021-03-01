using System;
using System.Collections.Generic;

namespace WowCurrencyManager.Room
{
    [Serializable]
    public class FarmRoomData
    {
        public List<RoomClient> Clinets = new List<RoomClient>();
        public decimal MinLos;
        public int Balance;
        public ulong ChannelId;
        public ulong GuildId;
        public DateTime NextMinimalPriceMessageDate;
    }
}
