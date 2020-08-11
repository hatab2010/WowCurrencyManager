using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
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
        public static Action<DiscordRoom> BalanceChanged;
        public decimal _minLos { private set; get; }

        public ISocketMessageChannel Channel { private set; get; }
        public List<RoomClient> Clients = new List<RoomClient>();
        private int _balance;
        private G2gOrder _order;

        public string Name => Channel.Name;
        public string Server => Regex.Replace(Name.Split('-')[0], "[_]", " ").ToLower();
        public string Fraction => Channel.Name.Split('-')[2].ToLower();
        public int Balance
        {
            get => _balance;
            private set
            {
                if (_balance != value)
                {
                    _balance = value;
                }
            }
        }

        public RestUserMessage LastBalanceMessage { get; internal set; }
        public G2gOrder Order
        {
            get => _order;
            private set
            { 
                if (value != _order)
                {
                    if (value != null)
                    {
                        SenOrderInChannle(value);
                    }

                    _order = value;
                }
            }
        }

        public bool IsOperationAllowed { private set; get; } = true;

        public DiscordRoom(ISocketMessageChannel channel)
        {
            Channel = channel;
        }

        public void SetMinLos(decimal value)
        {
            _minLos = value;
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

            if ((Clients == null || Clients.Count == 0) && Balance != 0)
            {
                Balance = 0;
                BalanceChanged?.Invoke(this);
                return;
            }

            result = Clients.Sum(_ => _.Balance);

            if (!IsOperationAllowed)
                return;

            Balance = result;
            BalanceChanged?.Invoke(this);
        }

        public async void SenOrderInChannle(G2gOrder order)
        {
            Emoji react = new Emoji("💰");
            var message = await Channel.SendMessageAsync("", false, order.GetOrderEmbed());

            await message.AddReactionAsync(react);
        }

        public async Task SendBalanceMessage()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle($"Баланс {Server}"); 

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
                IsOperationAllowed = true;      //Разрешить операции
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

            var operationResult = Order.Performer.Balance - Order.Amount;
            Balance -= Order.Amount;

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
