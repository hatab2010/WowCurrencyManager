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
        public async Task Pay() 
        {
            await Context.Message.DeleteAsync();
            var room = RouteRoom();
            var embedTickets = room.PayOperation();

            foreach (var item in embedTickets)
            {
                await Context.Channel.SendMessageAsync("", false, item);
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
    }

    [RequireGuild("BANK")]
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }

        [Command("gold")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task Gold(int value)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = RoomRouting.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);
            var client = room.GetClient(Context.User);

            client.SetBalance(value);
            room.UpdateBalance();

            await room.SendBalanceMessage();
        }

        [Command("wipe")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Wipe()
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = RoomRouting.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);
            room.RemoveAll();
        }

        [Command("disable")]
        public async Task Disable()
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = RoomRouting.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);
            room.RemoveClient(Context.User.Id);
        }

        [Command("minimal")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Minimal(decimal value)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = RoomRouting.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);
            room.SetMinLos(value);
            await room.SendBalanceMessage();
        }

        [Command("update")]        
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Update(int value)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = RoomRouting.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);
            room.UpdateMinutes = value;
            await room.SendBalanceMessage();
        }

        // Inherit from PreconditionAttribute       

        //[Command("order")]
        //[RequireBotPermission(ChannelPermission.ManageMessages)]
        //[RequireUserPermission(GuildPermission.Administrator)]
        //public async Task Order(int value)
        //{
        //    await Context.Channel.DeleteMessageAsync(Context.Message);
        //    var routing = RoomRouting.GetRoomRouting();

        //    var room = routing.GetRoom(Context.Channel);

        //    G2gOrder order = new G2gOrder()
        //    {
        //        Buyer = "Hacra",
        //        Amount = value,
        //        OrderId = "№5133842",
        //        Server = Context.Channel.Name
        //    };

        //    room.SetOrder(order);

        //    Emoji react = new Emoji("💰");
        //    var message = await Context.Channel.SendMessageAsync("", false, order.GetOrderEmbed());

        //    await message.AddReactionAsync(react);
        //}
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
