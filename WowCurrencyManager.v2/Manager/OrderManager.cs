using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WowCurrencyManager.v2.Data;
using Discord;
using Data;
using Microsoft.EntityFrameworkCore;

namespace WowCurrencyManager.v2.Manager
{
    public class OrderManager
    {
        private Order currentOrder;
        public OrderManager(Order order)
        {
            currentOrder = order;
        }

        public void TakePayer(ulong id)
        {
            using (var db = new MobileContext())
            {
                var client = db.Clients
                    .FirstOrDefault(_ => _.Id == id);

                if (client == null)
                    return;

                var purse = db.Purses
                    .AsQueryable()
                    .FirstOrDefault(_ => _.ChannelId == currentOrder.ChannelId
                                    && _.ClientId == client.Id);                 

                if (purse != null)
                {
                    var isFullAmount = purse.Gold >= currentOrder.Amount;
                    currentOrder.Clients.Add(client);

                    if (isFullAmount)
                    {
                        currentOrder.State = OrderState.Ready;
                        purse.Gold = 0;
                        purse.USD += currentOrder.USD * Global.ICOME_PROCENTAGE;
                    }
                    else
                    {
                        currentOrder.State = OrderState.Reserved;
                        currentOrder.Reserved = purse.Gold;                        
                    }

                    db.SaveChanges();
                    Save();
                }
            }
        }

        public void Save()
        {
            using (var db = new MobileContext())
            {
                var order = db.Orders.FirstOrDefault(_ => _.Id == currentOrder.Id);

                if (order == null)
                {
                    db.Orders.Add(currentOrder);
                }
                else
                {
                    order.OrderMessageId = currentOrder.OrderMessageId;
                    order.OrderPage = currentOrder.OrderPage;
                    order.Reserved = currentOrder.Reserved;
                    order.State = currentOrder.State;
                    order.Amount = currentOrder.Amount;
                    order.Buyer = currentOrder.Buyer;
                    order.ChannelId = currentOrder.ChannelId;
                }

                db.SaveChanges();
            }
        }

        public static Order GenerateOrder(int amount, ulong ChnnaleId, ulong id)
        {
            return new Order()
            {
                Amount = amount,
                Buyer = "TestUser",
                ChannelId = ChnnaleId,
                Id = id,
                OrderMessageId = null,
                OrderPage = "Test",
                Reserved = 0,
                State = OrderState.Open
            };
        }

        public static Embed OrderEmbed(ulong Id)
        {
            using (var db = new MobileContext())
            {
                var order = db.Orders
                    .Include(_=>_.Channel)
                    .Include(_=>_.Clients)
                    .FirstOrDefault(_=>_.Id == Id);

                if (order == null)
                    return null;

                var builder = new EmbedBuilder();

                builder.AddField("Order", order.Channel.ServerName, false);
                builder.AddField("Order", order.Id, false);
                builder.AddField("Gold", order.Amount, false);
                builder.AddField("Buyer", order.Buyer, false);
                builder.WithThumbnailUrl("https://cdn.discordapp.com/attachments" +
                    "/739498423958372362/757573166347321405/Logo.jpg");

                switch (order.State)
                {
                    case OrderState.Open:
                        builder.Color = Color.Blue;
                        break;
                    case OrderState.Reserved:
                        var footerReserved = new EmbedFooterBuilder();
                        builder.AddField("___", "Order delivery by:");
                        builder.WithFooter(footerReserved);
                        builder.Color = Color.Green;
                        footerReserved.WithIconUrl(Global.Icons["Safe"])
                                  .WithText(order.Reserved.ToString());
                        builder.Color = Color.LightOrange;
                        break;
                    case OrderState.Ready:
                        var footer = new EmbedFooterBuilder();

                        if (order.Clients.Count == 1)
                        {
                            footer.WithIconUrl(order.Clients[0].AvatarUrl)
                                  .WithText(order.Clients[0].Name);
                        }
                        else
                        {
                            var sBulder = new StringBuilder();

                            for (int i = 0; i < order.Clients.Count; i++)
                            {
                                sBulder.Append($"{order.Clients[i].Name}");
                                if (i != order.Clients.Count - 1)
                                {
                                    sBulder.Append($", ");
                                }
                            }

                            footer.WithIconUrl(Global.Icons["People"])
                                  .WithText(sBulder.ToString());
                        }

                        builder.AddField("___", "Order delivery by:");
                        builder.WithFooter(footer);
                        builder.Color = Color.Green;
                        break;
                    case OrderState.Close:
                        builder.Color = Color.Red;
                        break;
                    default:
                        break;
                }

                return builder.Build();

            }
        }
    }
}
