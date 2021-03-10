using Data;
using Discord;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WowCurrencyManager.v2.Data;

namespace WowCurrencyManager.v2.Model
{
    public class FarmRoomModel
    {
        private Channel channel;
        public List<Purse> Purses = new List<Purse>();
        public List<Order> Orders = new List<Order>();
        //public List<Client> Clients;

        public FarmRoomModel(Channel channel)
        {
            this.channel = channel;
            Init();
        }

        public Embed BuildEmbiend()
        {
            var builder = new EmbedBuilder();
            return builder.Build();
        }

        private void Init()
        {
            using (var db = new MobileContext())
            {
                Orders = db.Orders   
                           .AsQueryable()
                           .Where(_ => _.ChannelId == channel.Id 
                                  && (_.State == OrderState.Open || _.State == OrderState.Reserved)).ToList();

                Purses = db.Purses
                           .AsQueryable()
                           .Where(_ => _.ChannelId == channel.Id).ToList();
            }
        }


    }
}
