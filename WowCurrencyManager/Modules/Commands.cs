﻿using System;
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
            await Context.Message.DeleteAsync();
            var room = RouteRoom();
            var clients = room.PayOperation();

            var admins = await GetAdmins();

            foreach (var item in clients)
            {
                var builder = new EmbedBuilder();
                builder.WithTitle(Context.Channel.Name);
                builder.WithDescription($"{item.Name} ОПЛАЧЕНО: {item.USDBalance}$");

                foreach (var admin in admins)
                {
                    await admin.SendMessageAsync("", false, builder.Build());
                }

                await Context.Channel.SendMessageAsync("", false, item.PayEmbedBuild());
                item.RefrashBalance();
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

            var client = room.Clients.FirstOrDefault(_ => _.Name == username);

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

        [Command("wage")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Wage(int value, [Remainder]string username)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);                              

            var routing = FinanseRoomRouting.Get();
            var room = routing.Cash.Rooms.FirstOrDefault(_ => _.Id == Context.Channel.Id);
                   
            if (room == null)
            {
                await Context.User.SendMessageAsync($"Ошибка запроса: комната не была инициализирована");
                return;
            }

            var client = room.Clients.FirstOrDefault(_ => _.Name == username);

            if (client != null)
            {
                var builder = new EmbedBuilder();
                var footer = new EmbedFooterBuilder();

                client.AddUSDBalance(value);
                builder.WithTitle($"{Context.Channel.Name}");
                builder.WithDescription($"{client.Name}: зачеслено {value}$");
                footer.WithText($"Итоговый дебет: {client.USDBalance}");
                builder.WithFooter(footer);

                if (client == null)
                {
                    await Context.User.SendMessageAsync($"Пользователь с ником {username} не найден");
                }

                var admins = await GetAdmins();

                foreach (var item in admins)
                {
                    if (item.IsBot == false)
                        await item.SendMessageAsync("", false, builder.Build());
                }
            }         
        }

        [Command("sold")]
        public async Task Sold(int value)
        {
            var client = RoutClient();            
            client.AddBalance(value);

            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync("", false, client.SellEmbedBuild(value));
        }

        [Command("remove")]
        public async Task Remove(int value)
        {            
            var client = RoutClient();
            client.RemoveBalance(value);

            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync("", false, client.RemoveEmbedBuilder(value));
        }

        private FinanceClient RoutClient()
        {
            var room = RouteRoom();

            var clietn = room.Clients.FirstOrDefault(_ => _.Id == Context.User.Id);
            if (clietn == null)
            {
                clietn = new FinanceClient(Context.User.Id, Context.User.Username);
                room.AddClient(clietn);
            }

            return clietn;
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

            var addedUser = users.FirstOrDefault(_ => _.Username == name);

            if (addedUser != null)
            {
                var room = RouteRoom();
                var client = room.Clients.ToList().FirstOrDefault(_ => _.Name == addedUser.Username);

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
            var routing = FarmRoomRouting.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);
            var client = room.GetClient(Context.User);

            client.SetGoldAmount(value);
            room.UpdateBalance();

            await room.SendBalanceMessage();
        }

        [Command("add")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]

        public async Task Create(int gold, string name)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);

            var users = await Context.Channel.GetUsersAsync().FlattenAsync();

            var addedUser = users.FirstOrDefault(_ => _.Username == name);

            if (addedUser != null)
            {
                var routing = FarmRoomRouting.GetRoomRouting();
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

        public async Task Cancel()
        {
            var routing = FarmRoomRouting.GetRoomRouting();
            var room = routing.GetRoom(Context.Channel);

            if (room.Order != null)
            {
                room.Order.IsCansel = true;
                await Context.Channel.DeleteMessageAsync(room.Order.OrderMessageId);
            }
        }

        [Command("wipe")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Wipe()
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = FarmRoomRouting.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);
            room.RemoveAll();
        }


        [Command("set")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Set(int gold, [Remainder]string username)
        {   
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = FarmRoomRouting.GetRoomRouting();
            var room = routing.GetRoom(Context.Channel);
            var selectClient = room.Clients.FirstOrDefault(_ => _.Name == username);
            
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
            var routing = FarmRoomRouting.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);
            room.RemoveClient(Context.User.Id);
        }

        [Command("minimal")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Minimal(decimal value)
        {           

            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = FarmRoomRouting.GetRoomRouting();
            var room = routing.GetRoom(Context.Channel);
            var embed = room.SetMinLos(value);
            await Context.User.SendMessageAsync("", false, embed);
        }

        [Command("minimal")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Minimal()
        {

            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = FarmRoomRouting.GetRoomRouting();
            var room = routing.GetRoom(Context.Channel);
            var embed = room.GetMinimalPriceEmbed();
            await Context.User.SendMessageAsync("", false, embed);
        }

        [Command("update")]        
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Update(int value)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = FarmRoomRouting.GetRoomRouting();

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
            var routing = FarmRoomRouting.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);

            G2gOrder order = new G2gOrder()
            {
                Buyer = "Udenlo",
                Amount = value,
                OrderId = "№4170194",
                Server = "Benediction [US] Alliance"
            };

            room.SetOrder(order);

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
