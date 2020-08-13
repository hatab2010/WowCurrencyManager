using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WowCurrencyManager.Model;

namespace WowCurrencyManager.Room
{
    public enum BalanceState
    {
        Good, Bad
    }

    public class RoomBase
    {
        public List<RoomClient> Clients = new List<RoomClient>();
        public ISocketMessageChannel Channel { protected set; get; }
        public string Name => Channel.Name;

        protected int _balance;
    }

    public class DiscordRoom : RoomBase
    {
        public static Action<DiscordRoom> Changed;        
        public string Server => Regex.Replace(Name.Split('-')[0], "[_]", " ").ToLower();
        public string Fraction => Channel.Name.Split('-')[2].ToLower();
        public string WordPart => Channel.Name.Split('-')[1].ToLower();        
        public decimal MinLos 
        { 
            get => _minLos;
            private set 
            { 
                if (_minLos != value)
                {
                    Changed?.Invoke(this);
                }
                _minLos = value; 
            }
        }
        public int UpdateMinutes = 30;
        private G2gOrder _order;
        private Stopwatch _lLastUpdate;
        private decimal _minLos;
        private const int MIN_BALANS = 400;

        public int Balance
        {
            get => _balance;
            private set
            {
                if (_balance != value)
                {
                    _lLastUpdate.Restart();
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
                        SendOrderInChannle(value);
                    }

                    _order = value;
                }
            }
        }

        public bool IsOperationAllowed { private set; get; } = true;

        public DiscordRoom(ISocketMessageChannel channel)
        {
            _lLastUpdate = new Stopwatch();
            _lLastUpdate.Start();
            Task.Run(WathToUpdate);
            Channel = channel;
        }

        private void WathToUpdate()
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (_lLastUpdate.ElapsedMilliseconds > UpdateMinutes * 60 * 1000 && Balance > MIN_BALANS)
                {
                    Changed?.Invoke(this);
                    _lLastUpdate.Restart();
                }
            }
        }

        public Embed SetMinLos(decimal value)
        {
            MinLos = value;

            var builder = new EmbedBuilder();

            return GetMinimalPriceEmbed();
        }

        public Embed GetMinimalPriceEmbed()
        {
            var builder = new EmbedBuilder();
            builder.Description = $"{Server.FirstCharUp()} [{WordPart}] {Fraction}";
            builder.AddField("minimal price:", $"{MinLos} USD");

            return builder.Build();
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

        public void RemoveClient(ulong id)
        {
            var client = Clients.FirstOrDefault(_ => _.Id == id);

            if (client == null) return;
            Clients.Remove(client);

            UpdateBalance();
            SendBalanceMessage().Wait();
        }

        public void SetOrder(G2gOrder order)
        {
            Order = order;
        }        

        public void RemoveAll()
        {
            Clients.Clear();

            IsOperationAllowed = true;
            UpdateBalance();
            SendBalanceMessage().Wait();            
        }

        public void UpdateBalance()
        {
            int result = 0;

            if ((Clients == null || Clients.Count == 0) && Balance != 0)
            {
                Balance = 0;
                Changed?.Invoke(this);
                return;
            }

            result = Clients.Sum(_ => _.Balance);

            if (!IsOperationAllowed)
                return;

            Balance = result;
            Changed?.Invoke(this);            
        }

        public async void SendOrderInChannle(G2gOrder order)
        {
            Emoji react = new Emoji("💰");
            var message = await Channel.SendMessageAsync("", false, order.GetOrderEmbed());

            await message.AddReactionAsync(react);
        }

        public async Task SendBalanceMessage()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle($"Баланс {Server.FirstCharUp()} [{WordPart.ToUpper()}] {Fraction.FirstCharUp()}");
            //builder.WithThumbnailUrl("https://cdn.discordapp.com/attachments/739498423958372362/743083086945714176/hideaway-logo-final-flat-max.jpg");

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

            builder.AddField("Update", UpdateMinutes + " min", false);                    

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

        public async void OrderSuccess()
        {
            var operationResult = Order.Performer.Balance - Order.Amount;
            Balance -= Order.Amount;

            if (operationResult >= 0)
            {
                IsOperationAllowed = true;
                Order.Performer.SetGoldAmount(operationResult);
            }
            else
            {
                IsOperationAllowed = false;
                Order.Performer.SetGoldAmount(0);
            }

            await SendBalanceMessage();
        }
    }

    public class RoomClient : ClientBase
    {        
        public string AvatarUrl { private set; get; }        

        public RoomClient(ulong id, string name, string avatarUrl) 
        {
            Id = id;
            Name = name;
            AvatarUrl = avatarUrl;
        }
    }
}
