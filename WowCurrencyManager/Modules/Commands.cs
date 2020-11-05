using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using WowCurrencyManager.Model;
using WowCurrencyManager.Room;

namespace WowCurrencyManager.Modules
{
    [RequireGuild("Финансы")]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public class FinanceCommands : ModuleBase<SocketCommandContext>
    {
        [Command("pay")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Pay()
        {

            var routing = FinanseRoomRouting.Get();
            var clients = routing.Cash.Rooms?
                .SelectMany(_ => _.Clients);
            var str = new StringBuilder();

            foreach (var client in clients)
            {
                if (client.USDBalance == 0) continue;
                str.AppendLine($"**{client.Name}**: {client.USDBalance} USD");
            }            
            
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(str.ToString());
            
            foreach (var room in routing.Cash.Rooms)
            {
                var channel = Context.Guild.GetTextChannel(room.Id);

                foreach (var item in room.Clients)
                {
                    try
                    {
                        var builder = new EmbedBuilder();
                        builder.WithTitle(channel.Name);
                        builder.WithDescription($"{item.Name} ОПЛАЧЕНО: {item.USDBalance}$");
                        await channel.SendMessageAsync("", false, item.PayEmbedBuild());
                        item.RefrashBalance();
                    }
                    catch (Exception)
                    {
                    }                    
                }
            }
        }

        [Command("set")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Set(int value, [Remainder]string username) 
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = FinanseRoomRouting.Get();
            var room = routing.Cash.Rooms.FirstOrDefault(_=>_.Id == Context.Channel.Id);

            if (room == null)
            {
                await Context.User.SendMessageAsync($"Ошибка запроса: комната не была инициализирована");
                return;
            }

            var client = room.Clients.FirstOrDefault(_ => _.Name.ToLower().Contains(username.ToLower()));

            if (client == null)
            {
                await Context.User.SendMessageAsync($"Пользователь с ником {username} не найден");
            }

            client.AddBalance(value);
            await Context.Channel.SendMessageAsync("", false, client.BalanceBuild());
        }

        private async Task<List<IUser>> GetAdmins()
        {
            var users = (await Context.Channel.GetUsersAsync()
                .FlattenAsync());
            return users.ToList()
                .Where(_ => ((IGuildUser)_)
                    .GetPermissions((IGuildChannel)Context.Channel)
                    .ToList()
                    .Contains(ChannelPermission.ManageChannels)
                    && _.IsBot == false).ToList();
        }
        [Command("spam")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Spam(int value, [Remainder]string username)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);

            for (int i = 0; i < 100; i++)
            {
                await Context.Channel.SendMessageAsync("spam");
            }
        }

        [Command("wage")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Wage(int value, [Remainder]string username)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);                              

            var routing = FinanseRoomRouting.Get();
            var client = routing.Cash.Rooms?
                .SelectMany(_ => _.Clients)?
                .FirstOrDefault(_ => _.Name.ToLower().Contains(username.ToLower()));
                   
            if (client == null)
            {
                await Context.Channel.SendMessageAsync($"Пользователь {username} не найден");
                return;
            }
            else
            {
                var builder = new EmbedBuilder();
                var footer = new EmbedFooterBuilder();

                client.AddUSDBalance(value);
                builder.WithTitle($"{Context.Channel.Name}");
                builder.WithDescription($"{client.Name}: зачеслено {value}$");
                footer.WithText($"Итоговый дебет: {client.USDBalance}");
                builder.WithFooter(footer);

                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }         
        }

        [Command("wage")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Wage()
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);

            var routing = FinanseRoomRouting.Get();
            var clients = routing.Cash.Rooms?
                .SelectMany(_ => _.Clients);
            var str = new StringBuilder();

            foreach (var client in clients)
            {
                if (client.USDBalance == 0) continue;
                str.AppendLine($"**{client.Name}**: {client.USDBalance} USD");
            }

            await Context.Channel.SendMessageAsync(str.ToString());
        }

        [Command("sold")]
        public async Task Sold(int value)
        {
            var client = FindClient();            
            client.AddBalance(value);

            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync("", false, client.SellEmbedBuild(value));
        }

        [Command("remove")]
        public async Task Remove(int value)
        {            
            var client = FindClient();
            client.RemoveBalance(value);

            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync("", false, client.RemoveEmbedBuilder(value));
        }

        private FinanceClient FindClient()
        {
            var room = RouteRoom();

            var client = room.Clients.FirstOrDefault(_ => _.Id == Context.User.Id);
            if (client == null)
            {
                client = new FinanceClient(Context.User.Id, Context.User.Username);
                room.AddClient(client);
            }
            else if (client.Name != Context.User.Username)
            {
                client.SetName(Context.User.Username);
            }

            return client;
        }

        private FinanceRoom RouteRoom()
        {
            var room = FinanseRoomRouting.Get().Cash.Rooms.FirstOrDefault(_ => _.Id == Context.Channel.Id);

            if (room == null)
            {
                room = new FinanceRoom(Context.Channel.Id);
                FinanseRoomRouting.Get().CreateRoom(room);
            }

            return room;
        }

        [Command("add")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Create([Remainder]string name)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);

            var users = await Context.Channel.GetUsersAsync().FlattenAsync();            

            var addedUser = users.FirstOrDefault(_ => _.Username.ToLower().Contains(name.ToLower()));

            if (addedUser != null)
            {
                var room = RouteRoom();
                var client = room.Clients.ToList().FirstOrDefault(_ => _.Name.ToLower().Contains(addedUser.Username.ToLower()));

                var builder = new EmbedBuilder();
                builder.WithTitle($"{Context.Channel.Name}");
                builder.WithDescription($"добавлен пользователь {addedUser.Username}");

                if (client == null)
                {
                    client = new FinanceClient(addedUser.Id, addedUser.Username);                   
                    room.AddClient(client);

                    await Context.User.SendMessageAsync("", false, builder.Build());
                }
                else
                {
                    builder.WithDescription($"{addedUser.Username} уже существует");
                    await Context.User.SendMessageAsync("", false, builder.Build());
                }
                
            }
        }
    }

    public class ForAllCommands : ModuleBase<SocketCommandContext>
    {
        [Command("clear")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task Clear()
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);

            var messages = Context.Channel.GetMessagesAsync(7)
                .FlattenAsync();
            foreach (var item in await messages)
            {
                if (item.Author.IsBot == true)
                {
                    await Context.Channel.DeleteMessageAsync(item);
                }
            }
        }
    }  


    [RequireGuild("BANK")]
    public class Commands : ModuleBase<SocketCommandContext>
    {
        public static Action Stoped;

        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }

        [Command("stop")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Stop()
        {
            Stoped?.Invoke();
            Environment.Exit(0);
        }

        [Command("gold")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task Gold(int value)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = FarmRoomManager.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);
            var client = room.GetClient(Context.User);

            client.SetGoldAmount(value);
            room.UpdateBalance();

            await room.SendBalanceMessage();
        }

        [Command("price")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task Price()
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = FarmRoomManager.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);

            if (DateTime.Now < room.Cash.NextMinimalPriceMessageDate)
            {               
                await Context.Channel.SendMessageAsync($"Команда заблокирована до {room.Cash.NextMinimalPriceMessageDate}");
                return;
            }

            if (room.LastMinimalPrice == 0)
            {
                await Context.Channel.SendMessageAsync("Бот ещё не совершил мониторинг цен");
            }
            else
            {
                room.Cash.NextMinimalPriceMessageDate = DateTime.Now.AddDays(7);
                lock (room.Cash)
                {
                    FarmRoomManager.GetRoomRouting().SaveCashRooms();
                }
                await Context.Channel.SendMessageAsync($"Текущая ствака {room.LastMinimalPrice}");
            }
        }

        [Command("add")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]

        public async Task Create(int gold, string name)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);

            var users = await Context.Channel.GetUsersAsync().FlattenAsync();

            var addedUser = users.FirstOrDefault(_ => _.Username.ToLower().Contains(name.ToLower()));

            if (addedUser != null)
            {
                var routing = FarmRoomManager.GetRoomRouting();
                var room = routing.GetRoom(Context.Channel);
                var client = room.GetClient(addedUser);

                client.SetGoldAmount(gold);
                room.UpdateBalance();

                await room.SendBalanceMessage();
            }                        
        }

        [Command("cancel")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]

        public async Task Cancel(string orderId)
        {
            var routing = FarmRoomManager.GetRoomRouting();
            var room = routing.GetRoom(Context.Channel);
            var curOrder = room.Orders.FirstOrDefault(_ => _.OrderId.Contains(orderId));
            if (curOrder != null)
            {
                curOrder.IsCansel = true;
                await Context.Channel.DeleteMessageAsync(curOrder.OrderMessageId);
                room.Orders.Remove(curOrder);
            }
        }

        [Command("wipe")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Wipe()
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = FarmRoomManager.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);
            room.RemoveAll();
        }


        [Command("set")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Set(int gold, [Remainder]string username)
        {   
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = FarmRoomManager.GetRoomRouting();
            var room = routing.GetRoom(Context.Channel);
            var selectClient = room.Cash.Clinets.FirstOrDefault(_ => _.Name.ToLower().Contains(username.ToLower()));
            
            if (selectClient == null)
            {
                await Context.User.SendMessageAsync($"Пользователь с ником {username} не найден");
            }
            else
            {
                selectClient.SetGoldAmount(gold);
                room.UpdateBalance();
                await room.SendBalanceMessage();
            }
        }

        [Command("disable")]
        public async Task Disable()
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = FarmRoomManager.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);
            room.RemoveClient(Context.User.Id);
        }

        [Command("minimal")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Minimal(decimal value, string channelName)
        {           
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = FarmRoomManager.GetRoomRouting();
            var room = routing.Rooms.FirstOrDefault(_=>_.Name.ToLower().Contains(channelName.ToLower()));

            if (room == null)
            {
                await Context.Channel.SendMessageAsync($"Сервер {channelName} не был найден");
            }
            else
            {
                var embed = room.SetMinLos(value);
                await Context.Channel.SendMessageAsync("", false, embed);
            }
        }

        [Command("minimal")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Minimal()
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);            
            var embed = new EmbedBuilder();
            embed.WithTitle("Минимальные ставки");
            var rooms = FarmRoomManager.GetRoomRouting().Rooms;

            foreach (var room in rooms)
            {
                try
                {
                    embed.AddField(room.Name, room.MinLos, false);
                }
                catch
                {

                }                
            }

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("update")]        
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Update(int value)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = FarmRoomManager.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);
            room.UpdateMinutes = value;
            await room.SendBalanceMessage();
        }

        // Inherit from PreconditionAttribute       

        [Command("order")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Order(int value)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = FarmRoomManager.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);

            G2gOrder order = new G2gOrder()
            {
                Buyer = "Udenlo",
                Amount = value,
                OrderId = "№4170194",
                Server = "Benediction [US] Alliance"
            };

            room.AddOrder(order);

            Emoji react = new Emoji("💰");
            var message = await Context.Channel.SendMessageAsync("", false, order.GetOrderEmbed());

            await message.AddReactionAsync(react);
        }
    }
    public class RequireGuildAttribute : PreconditionAttribute
    {
        // Create a field to store the specified name
        private readonly string _name;

        // Create a constructor so the name can be specified
        public RequireGuildAttribute(string name) => _name = name;

        // Override the CheckPermissions method
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            // Check if this user is a Guild User, which is the only context where roles exist
            if (context.Guild is SocketGuild guild)
            {
                // If this command was executed by a user with the appropriate role, return a success
                if (guild.Name == _name)
                    // Since no async work is done, the result has to be wrapped with `Task.FromResult` to avoid compiler errors
                    return Task.FromResult(PreconditionResult.FromSuccess());
                // Since it wasn't, fail
                else
                    return Task.FromResult(PreconditionResult.FromError($""));
            }
            else
                return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
        }
    }
}
