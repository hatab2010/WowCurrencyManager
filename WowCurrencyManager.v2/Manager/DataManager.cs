using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WowCurrencyManager.v2.Data;
using WowCurrencyManager.v2.Model;

namespace WowCurrencyManager.v2.Manager
{
    public class DataManager
    {
        public MobileContext Db;
        public DataManager(MobileContext db)
        {
            Db = db;
        }

        public void Save(Guild guild)
        {
            var curGuild = Db.Guilds.FirstOrDefault(_ => _.Id == guild.Id);

            if (curGuild == null)
            {
                Db.Guilds.Add(guild);
            }
            else
            {
                curGuild.Name = guild.Name;
            }

            Db.SaveChanges();
        }

        public void Save(Channel value)
        {

            var channel = Db.Channels.FirstOrDefault(_ => _.Id == value.Id);
            if (channel == null)
            {
                Db.Channels.Add(value);
            }
            else
            {
                channel.ChannelRole = value.ChannelRole;
                channel.Fraction = value.Fraction;
                channel.WorldPart = value.WorldPart;
                channel.ServerName = value.ServerName;
                channel.GameVersion = value.GameVersion;
            }

            Db.SaveChanges();
        }
    }
}
