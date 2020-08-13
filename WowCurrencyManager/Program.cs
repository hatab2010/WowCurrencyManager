using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Reflection;
using WowCurrencyManager.Room;
using Discord.Rest;
using WowCurrencyManager.WebDriver;

namespace WowCurrencyManager
{
    class Program
    {
        static void Main(string[] args) => new Program()
            .RunBotAsync()
            .GetAwaiter()
            .GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task RunBotAsync()
        {
            WebManager.InitManager();

            _client = new DiscordSocketClient();
            _commands = new CommandService();
                   
            _services = new ServiceCollection()             
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string token = "NzM5NjI1NzYyNDk5NTkyMjEy.XydMKw.-MR9R0c9Cj-978wSpLpda-pjuOM";

            _client.Log += _client_Log;
            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            _client.ReactionAdded += HandleReactionAddedAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private Task HandleReactionAddedAsync(
            Cacheable<IUserMessage, ulong> arg1, 
            ISocketMessageChannel arg2, 
            SocketReaction arg3)
        {
            if (arg3.Emote.ToString() != "💰"
                || arg3.User.Value.IsBot)
                return Task.CompletedTask;            

            var lMessage = arg1.GetOrDownloadAsync();
            lMessage.Wait();
            var routing = RoomRouting.GetRoomRouting();
            DiscordRoom room = routing.GetRoom(arg2);            

            if (room.Order.Performer != null)
            {
                return Task.CompletedTask;
            }

            room.Order.SetPerformer(room.GetClient(arg3.User.Value));
            lMessage.Result.ModifyAsync(msg => msg.Embed = room.Order.GetOrderEmbed());
            lMessage.Result.RemoveAllReactionsAsync();

            room.OrderSuccess();

            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);            

            if (message.Author.IsBot) return;            

            int argPos = 0;

            if (message.HasStringPrefix("/", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }
    }
}
