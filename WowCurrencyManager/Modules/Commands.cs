using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using WowCurrencyManager.Model;
using WowCurrencyManager.Room;

namespace WowCurrencyManager.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }

        [Command("gold")]
        [Summary("Deletes the specified amount of messages.")]
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

        [Command("disable")]
        public async Task Disable()
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = RoomRouting.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);
            room.RemoveClient(Context.User.Id);
        }

        [Command("Minimal")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Minimal(decimal value)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = RoomRouting.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);
            room.SetMinLos(value);
        }

        [Command("Order")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Order(int value)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = RoomRouting.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel);

            G2gOrder order = new G2gOrder()
            {
                Buyer = "Hacra",
                Amount = value,
                OrderId = "№5133842",
                Server = Context.Channel.Name
            };

            room.SetOrder(order);

            Emoji react = new Emoji("💰");
            var message = await Context.Channel.SendMessageAsync("", false, order.GetOrderEmbed());
            
            await message.AddReactionAsync(react);
        }
    }
}
