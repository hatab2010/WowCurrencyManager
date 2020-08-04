using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WowCurrencyManager.Model;

namespace WowCurrencyManager.Room
{
    public enum BalanceState
    {
        Good, Bad
    }

    public class DiscordRoom
    {
        public ISocketMessageChannel Channel { private set; get; }
        public List<RoomClient> Clients = new List<RoomClient>();
        public string Name => Channel.Name;
        public int Balance { private set; get; }        
        public RestUserMessage LastBalanceMessage { get; internal set; }
        public G2gOrder Order { private set; get; }

        public bool IsOperationAllowed { private set; get; } = true;

        public DiscordRoom(ISocketMessageChannel channel)
        {
            Channel = channel;
        }

        public RoomClient GetClient(IUser user)
        {
            RoomClient client = Clients.FirstOrDefault(_ => _.Id == user.Id);

            if (client == null)
            {
                var avatar = user.GetAvatarUrl();

                if (string.IsNullOrEmpty(avatar))
                {
                    avatar = user.GetDefaultAvatarUrl();
                }

                client = new RoomClient(user.Id, user.Username, avatar);
                Clients.Add(client);
                return client;
            }
            else
            {
                return client;
            }
        }

        public void SetOrder(G2gOrder order)
        {
            Order = order;
        }

        public void RemoveClient(ulong id)
        {
            var client = Clients.FirstOrDefault(_ => _.Id == id);

            if (client == null) return;
            Clients.Remove(client);

            UpdateBalance();
            SendBalanceMessage().Wait();
        }

        public void UpdateBalance()
        {            
            int result = 0;

            if (Clients == null || Clients.Count == 0)
            {
                Balance = 0;
                return;
            }

            result = Clients.Sum(_ => _.Balance);

            if (!IsOperationAllowed)
                return;

            Balance = result;
        }

        public async Task SendBalanceMessage()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Баланс сервера");

            foreach (var item in Clients)
            {
                builder.AddField(item.Name, item.Balance, true);
            }

            builder.AddField("Общий баланс", Balance, false);

            if (Balance != Clients.Sum(_ => _.Balance))
            {
                builder.WithDescription("Скорректируйте балансы ваших кошельков");
                builder.WithColor(Color.Red);
            }
            else
            {
                IsOperationAllowed = true;
                builder.WithColor(Color.Green);
            }           

            var channelMessages = await Channel.GetMessagesAsync(1, CacheMode.AllowDownload).LastAsync();

            if (channelMessages.Count > 0
                && LastBalanceMessage != null
                && channelMessages.Last().Id == LastBalanceMessage.Id)
            {
                await LastBalanceMessage.ModifyAsync(msg => msg.Embed = builder.Build());
            }
            else if (LastBalanceMessage != null)
            {
                await Channel.DeleteMessageAsync(LastBalanceMessage);
                LastBalanceMessage = await Channel.SendMessageAsync("", false, builder.Build());
            }
            else
            {
                LastBalanceMessage = await Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        public void OrderAccepted()
        {
            //TODO selenium

            
        }

        public void OrderSuccess()
        {
            //TODO selenium

            var operationResult = Order.Performer.Balance - Order.Gold;
            Balance -= Order.Gold;

            if (operationResult >= 0)
            {
                IsOperationAllowed = true;
                Order.Performer.SetBalance(operationResult);
            }
            else
            {
                IsOperationAllowed = false;
                Order.Performer.SetBalance(0);
            }

            SendBalanceMessage().Wait();
        }
    }

    public class RoomClient : PurseBase
    {
        public ulong Id { private set; get; }
        public string AvatarUrl { private set; get; }
        public string Name { private set; get; }

        public RoomClient(ulong id, string name, string avatarUrl)
        {
            Id = id;
            Name = name;
            AvatarUrl = avatarUrl;
        }
    }
}
