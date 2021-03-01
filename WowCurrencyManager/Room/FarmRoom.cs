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
using WowCurrencyManager.Bot;
using WowCurrencyManager.Bot.Pages;
using WowCurrencyManager.Model;

namespace WowCurrencyManager.Room
{

    [Serializable]
    public class FarmRoom : RoomBase
    {
        public const string NAME_PATTERN = @"^\S*\s?\S*-\S{2}-\S*-?\S*$";

        public static Action<FarmRoom> Changed;
        public string Server => _nameParams[0].ToLower().Replace("_"," ");
        public string Fraction => _nameParams[2].ToLower();
        public GameVersion WowServerType
        {
            get
            {
                GameVersion result = GameVersion.Classic;
                

                if (_nameParams.Count() == 4)
                {
                    result = _nameParams[3].ToLower().Contains("main") ? GameVersion.Main : GameVersion.Classic;
                }

                return result;
            }
        }

        public WorldPart WordPart => (WorldPart) Enum.Parse(typeof(WorldPart), _nameParams[1], true);

        public decimal LastMinimalPrice = 0;

        #region propperty
        private int _minBalance
        {
            get
            {
                if (Channel.Name.Contains("main"))
                {
                    return 200;
                }
                else
                {
                    return 400;
                }
            }
        }
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
            Cash.GuildId = item.GuildId;
            Cash.ChannelId = item.ChannelId;
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
                GuildId = ((SocketGuildChannel)channel).Guild.Id
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
                if (_lLastUpdate.ElapsedMilliseconds > UpdateMinutes * 60 * 1000 && Balance > _minBalance)
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

            builder.WithTitle($"Баланс {Server.FirstCharUp()} [{WordPart.ToString().ToUpper()}] {Fraction.FirstCharUp()}");
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

        public static bool IsMath(string name)
        {
            return Regex.IsMatch(name, NAME_PATTERN);
        }
    }
}
