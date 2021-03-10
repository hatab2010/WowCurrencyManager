using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowCurrencyManager.v2.Data
{
    public class Guild
    {
        public string Name { set; get; }
        public ulong Id { set; get; }

        public List<Channel> Channels { set; get; }
}
}
