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
using System.Threading;

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

            var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
            _client = new DiscordSocketClient(_config);
            _commands = new CommandService();                   
            _services = new ServiceCollection()             
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();
            
            string token = "";

            _client.Log += _client_Log;
            _client.Ready += OnReady;

            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();           

            
            await Task.Delay(-1);
        }

        private async Task OnReady()
        {
            var manager = FarmRoomManager.GetRoomRouting();
            manager.LoadCashRooms(_client);
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
            {
                Console.WriteLine($"{DateTime.Now}: Order adoption canceled \n" +
                    $"{arg3.User.Value.Username} is bot");
                return Task.CompletedTask;
            };            

            var lMessage = arg1.GetOrDownloadAsync();
            lMessage.Wait();

            var orderId = lMessage.Result.Embeds.FirstOrDefault().Fields.FirstOrDefault(_=>_.Name == "Order").Value;
            var routing = FarmRoomManager.GetRoomRouting();
            FarmRoom room = routing.GetRoom(arg2);
            var curOrder = room.Orders.FirstOrDefault(_ => _.OrderId.Contains(orderId));
            if (curOrder?.Performer != null)
            {
                Console.WriteLine($"{DateTime.Now}: Order adoption canceled \n" +
                    $"Order performer alredy exist");
                return Task.CompletedTask;
            }

            curOrder.SetPerformer(room.GetClient(arg3.User.Value));
            lMessage.Result.ModifyAsync(msg => msg.Embed = curOrder.GetOrderEmbed()).Wait();
            Thread.Sleep(2000);
            lMessage.Result.RemoveAllReactionsAsync().Wait();
            OrderWatchManager.AddOperation(new AcceptOrder(curOrder));
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
