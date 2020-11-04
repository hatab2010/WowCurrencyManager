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
        public List<RoomClient> Clinets = new List<RoomClient>();
        public decimal MinLos;
        public int Balance;
        public ulong ChannelId;
        public ulong guildId;
        public DateTime NextMinimalPriceMessageDate;
    }

    [Serializable]
    public class FarmRoom : RoomBase
    {
        public static Action<FarmRoom> Changed;
        private const int MIN_BALANS = 400;

        public string Server => Regex.Replace(Name.Split('-')[0], "[_]", " ").ToLower();
        public string Fraction => Channel.Name.Split('-')[2].ToLower();
        public string WordPart => Channel.Name.Split('-')[1].ToLower();
        public decimal LastMinimalPrice = 0;

        #region propperty
        public FarmRoomData Cash { private set; get; }

        public decimal MinLos 
        { 
            get => Cash.MinLos;
            private set 
            { 
                if (Cash.MinLos != value)
                {
                    Changed?.Invoke(this);
                }
                Cash.MinLos = value;
                var rm = FarmRoomManager.GetRoomRouting();
                rm.SaveCashRooms();
            }
        }

        internal void SetCash(FarmRoomData item)
        {
            Cash.MinLos = item.MinLos;
            Cash.Balance = item.Balance;
            Cash.Clinets = item.Clinets;
            Cash.NextMinimalPriceMessageDate = item.NextMinimalPriceMessageDate;
        }


        public List<G2gOrder> Orders = new List<G2gOrder>();

        public int Balance
        {
            get => Cash.Balance;
            private set
            {
                if (Cash.Balance != value)
                {
                    _lLastUpdate.Restart();
                    Cash.Balance = value;
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
                ChannelId = channel.Id,
                guildId = ((SocketGuildChannel)channel).Guild.Id
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
            RoomClient client = Cash.Clinets.FirstOrDefault(_ => _.Id == user.Id);

            if (client == null)
            {
                var avatar = user.GetAvatarUrl();

                if (string.IsNullOrEmpty(avatar))
                {
                    avatar = user.GetDefaultAvatarUrl();
                }

                client = new RoomClient(user.Id, user.Username, avatar);
                Cash.Clinets.Add(client);
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
            var client = Cash.Clinets.FirstOrDefault(_ => _.Id == id);

            if (client == null) return;
            Cash.Clinets.Remove(client);

            UpdateBalance();
            SendBalanceMessage().Wait();
        }

        public void AddOrder(G2gOrder order)
        {
            Orders.Add(order);
        }        

        public void RemoveAll()
        {
            Cash.Clinets.Clear();

            IsOperationAllowed = true;
            UpdateBalance();
            SendBalanceMessage().Wait();            
        }

        public void UpdateBalance()
        {
            int result = 0;

            if ((Cash.Clinets == null || Cash.Clinets.Count == 0) && Balance != 0)
            {
                Balance = 0;
                Changed?.Invoke(this);
                return;
            }

            result = Cash.Clinets.Sum(_ => _.Balance);

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

            foreach (var item in Cash.Clinets)
            {
                builder.AddField(item.Name, item.Balance, true);
            }

            builder.AddField("Общий баланс", Balance, false);

            if (Balance != Cash.Clinets.Sum(_ => _.Balance))
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

        public async void OrderAccept(G2gOrder order)
        {
            var operationResult = order.Performer.Balance - order.Amount;
            Balance -= order.Amount;

            if (operationResult >= 0)
            {
                IsOperationAllowed = true;
                order.Performer.SetGoldAmount(operationResult);
            }
            else
            {
                IsOperationAllowed = false;
                order.Performer.SetGoldAmount(0);
            }

            Orders.Remove(order);
            await SendBalanceMessage();
        }
    }
}
