using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowCurrencyManager.v2.Data
{
    public class Purse
    {
        public Guid Id { set; get; }
        public double USD { set; get; }
        public int Gold { set; get; }

        public ulong ChannelId { set; get; }
        public Channel Channel { set; get; }
        public ulong ClientId { set; get; }
        public Client Client { set; get; }
    }
}
