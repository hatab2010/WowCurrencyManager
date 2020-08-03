using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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

            var room = routing.GetRoom(Context.Channel.Name);
            var client = room.GetClient(Context.User.Id, Context.User.Username);

            client.SetBalance(value);
            room.UpdateBalance();

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Баланс комнаты");

            foreach (var item in room.Clients)
            {
                builder.AddField(item.Name, item.Balance, true);
            }

            builder.AddField("Общий баланс", room.Balance, false);
            builder.WithColor(Color.Green);

            var channelMessages = await Context.Channel.GetMessagesAsync(1, CacheMode.AllowDownload).LastAsync();

            if (channelMessages.Count > 0
                && room.LastBalanceMessage != null
                && channelMessages.Last().Id == room.LastBalanceMessage.Id)
            {
                await room.LastBalanceMessage.ModifyAsync( msg => msg.Embed = builder.Build());
            }
            else if (room.LastBalanceMessage != null)
            {
                await Context.Channel.DeleteMessageAsync(room.LastBalanceMessage);
                room.LastBalanceMessage = await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
            else
            {
                room.LastBalanceMessage = await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        [Command("disable")]
        public async Task Disable()
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var routing = RoomRouting.GetRoomRouting();

            var room = routing.GetRoom(Context.Channel.Name);
            room.RemoveClient(Context.User.Id);
        }

        [Command("Order")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Order(int value)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);

            Emoji react = new Emoji("💰");
            var message = await Context.Channel.SendMessageAsync($"Ордер на: {value}");
            
            await message.AddReactionAsync(react);
        }
    }
}
