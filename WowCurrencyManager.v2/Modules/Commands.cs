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
using WowCurrencyManager.v2.Data;
using WowCurrencyManager.v2.Model;

namespace WowCurrencyManager.v2.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("init")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Init(string channelRole)
        {
            ChannelRole result;
            var isRoleExist = Enum.TryParse<ChannelRole>(channelRole, true, out result);

            if (!isRoleExist)
            {
                var builder = new StringBuilder();

                foreach (var item in Enum.GetValues(typeof(ChannelRole)))
                {
                    builder.Append($"[{(ChannelRole)item}] ");
                }

                await Context.Channel.SendMessageAsync("Выбранной роли для комнаты не существует.\n" +
                    $" Доступные роли: {builder}");
                return;                
            }

            switch (result)
            {
                case ChannelRole.Farm:
                    Dialog.OpenDialogs.Add(new RegistrationFarmRoomDialog(Context.Channel));
                    break;
                case ChannelRole.SaleInfo:
                    break;
            }
        }

        [Command("ping")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Ping(string channelRole)
        {
            await Context.Channel.SendMessageAsync("Ping");
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
