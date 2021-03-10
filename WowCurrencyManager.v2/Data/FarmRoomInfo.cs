using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowCurrencyManager.v2.Data
{
    public class FarmRoomInfo
    {

        public List<Purse> Purses { set; get; }
        public ulong ChannelId { set; get; }
        public Channel Channel { set; get; }
    }
}
