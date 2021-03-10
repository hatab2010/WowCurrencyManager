using Discord;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using WowCurrencyManager.v2.Model;
using Data;
using Microsoft.EntityFrameworkCore;

namespace WowCurrencyManager.v2.Data
{
    public enum OrderState
    {
        Open, Reserved, Ready, Close
    }

    public class Order
    {
        public double USD { set; get; }
        public OrderState State { set; get; }
        public ulong Id { set; get; }
        public int Amount { set; get; }
        public string Buyer { set; get; }
        public string OrderPage { set; get; }
        public ulong? OrderMessageId { set; get; }
        public int Reserved { set; get; }

        public List<Client> Clients { set; get; }
        public ulong ChannelId { set; get; }
        public Channel Channel { set; get; }
    }
}