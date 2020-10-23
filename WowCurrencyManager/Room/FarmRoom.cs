using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WowCurrencyManager.Model;

namespace WowCurrencyManager.Room
{
    [Serializable]
    public class FarmRoomData
    {
        public List<RoomClient> _clients = new List<RoomClient>();
        public decimal _minLos;
        public int _balance;
        public ulong _channelId;
        public ulong _guildId;
    }

    [Serializable]
    public class FarmRoom : RoomBase
    {
        public static Action<FarmRoom> Changed;

        private const int MIN_BALANS = 400;

        public string Server => Regex.Replace(Name.Split('-')[0], "[_]", " ").ToLower();
        public string Fraction => Channel.Name.Split('-')[2].ToLower();
        public string WordPart => Channel.Name.Split('-')[1].ToLower();

        #region propperty
        public FarmRoomData Cash { private set; get; }

        public decimal MinLos 
        { 
            get => Cash._minLos;
            private set 
            { 
                if (Cash._minLos != value)
                {
                    Changed?.Invoke(this);
                }
                Cash._minLos = value;
                var rm = FarmRoomManager.GetRoomRouting();
                rm.SaveCashRooms();
            }
        }

        internal void SetCash(FarmRoomData item)
        {
            Cash._minLos = item._minLos;
            Cash._balance = item._balance;
            Cash._clients = item._clients;
        }

        private G2gOrder _order;
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

        public int Balance
        {
            get => Cash._balance;
            private set
            {
                if (Cash._balance != value)
                {
                    _lLastUpdate.Restart();
                    Cash._balance = value;
                    var rm = FarmRoomManager.GetRoomRouting();
                    rm.SaveCashRooms();
                }
            }
        }
        #endregion

        public int UpdateMinutes = 30;
        
        private Stopwatch _lLastUpdate;       

        public RestUserMessage LastBalanceMessage { get; internal set; }

        public bool IsOperationAllowed { private set; get; } = true;

        public FarmRoom(ISocketMessageChannel channel)
        {
            Cash = new FarmRoomData()
            {
                _channelId = channel.Id,
                _guildId = ((SocketGuildChannel)channel).Guild.Id
            };

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

            return GetMinimalPriceEmbed();
        }

        public Embed GetMinimalPriceEmbed()
        {
            var builder = new EmbedBuilder();
            builder.AddField("server", $"{Name}");
            builder.AddField("minimal price:", $"{MinLos} USD");

            return builder.Build();
        }

        public RoomClient GetClient(IUser user)
        {
            RoomClient client = Cash._clients.FirstOrDefault(_ => _.Id == user.Id);

            if (client == null)
            {
                var avatar = user.GetAvatarUrl();

                if (string.IsNullOrEmpty(avatar))
                {
                    avatar = user.GetDefaultAvatarUrl();
                }

                client = new RoomClient(user.Id, user.Username, avatar);
                Cash._clients.Add(client);
                return client;
            }
            else
            {
                if (client.Name != user.Username)
                {
                    client.SetName(user.Username);
                }

                return client;                
            }
        }

        public void RemoveClient(ulong id)
        {
            var client = Cash._clients.FirstOrDefault(_ => _.Id == id);

            if (client == null) return;
            Cash._clients.Remove(client);

            UpdateBalance();
            SendBalanceMessage().Wait();
        }

        public void SetOrder(G2gOrder order)
        {
            Order = order;
        }        

        public void RemoveAll()
        {
            Cash._clients.Clear();

            IsOperationAllowed = true;
            UpdateBalance();
            SendBalanceMessage().Wait();            
        }

        public void UpdateBalance()
        {
            int result = 0;

            if ((Cash._clients == null || Cash._clients.Count == 0) && Balance != 0)
            {
                Balance = 0;
                Changed?.Invoke(this);
                return;
            }

            result = Cash._clients.Sum(_ => _.Balance);

            if (!IsOperationAllowed)
                return;

            Balance = result;
            Changed?.Invoke(this);            
        }

        public async void SendOrderInChannle(G2gOrder order)
        {
            try
            {
                await send();
            }
            catch (Exception)
            {
                await Task.Delay(60000);
                await send();
            }

            async Task send()
            {
                Emoji react = new Emoji("💰");
                var message = await Channel.SendMessageAsync("", false, order.GetOrderEmbed());

                await message.AddReactionAsync(react);
                order.OrderMessageId = message.Id;
            }            
        }

        public async Task SendBalanceMessage()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle($"Баланс {Server.FirstCharUp()} [{WordPart.ToUpper()}] {Fraction.FirstCharUp()}");
            //builder.WithThumbnailUrl("https://cdn.discordapp.com/attachments/739498423958372362/743083086945714176/hideaway-logo-final-flat-max.jpg");

            foreach (var item in Cash._clients)
            {
                builder.AddField(item.Name, item.Balance, true);
            }

            builder.AddField("Общий баланс", Balance, false);

            if (Balance != Cash._clients.Sum(_ => _.Balance))
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
                && !String.IsNullOrEmpty(channelMessages.Last().Content)
                && channelMessages.Last().Id == LastBalanceMessage.Id)
            {
                await LastBalanceMessage.ModifyAsync(msg => msg.Embed = builder.Build());
            }
            else if (LastBalanceMessage != null)
            {
                try
                {
                    await Channel.DeleteMessageAsync(LastBalanceMessage);
                }
                catch (Exception)
                {

                }
                
                LastBalanceMessage = await Channel.SendMessageAsync("", false, builder.Build());
            }
            else
            {
                LastBalanceMessage = await Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        public async void OrderAccept()
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
}
