using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WowCurrencyManager.v2.Data
{
    public enum ChannelRole
    {
        Farm, SaleInfo
    }

    public enum GameVersion
    {
        Classic, Main
    }
    public enum WorldPart
    {
        EU, US
    }

    public enum Fraction
    {
        Horde, Alliance
    }

    public class Server
    {

    }

    public class Channel
    {
        public string Title => $"{ServerName} [{WorldPart}] {Fraction}";
        public ulong Id { set; get; }
        public string ServerName { set; get; }
        public WorldPart WorldPart { set; get; }
        public Fraction Fraction { set; get; }
        public GameVersion GameVersion { set; get; }
        public ChannelRole ChannelRole { set; get; }

        public ulong GuildId { set; get; }
        public Guild Guild { set; get; }
        public List<Purse> Purses {set; get;}
        public List<Order> Orders { set; get; }
    }
}
