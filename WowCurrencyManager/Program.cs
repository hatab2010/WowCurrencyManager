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
using WowCurrencyManager.Bot;
using System.Threading;
using System.Text.RegularExpressions;

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
            DriversManager.Init();

            var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
            _client = new DiscordSocketClient(_config);
            _commands = new CommandService();                   
            _services = new ServiceCollection()             
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            var token = "NzQzMjQ4MjYwOTQwMTY5MjU3.XzR54g.5mhIDcIIWwEcnklBxJp68Pf8Ta4";

            _client.Log += _client_Log;
            _client.Ready += OnReady;

            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            Connection();


            await Task.Delay(-1);
        }

        public async void Connection()
        {
            var connTime = 0;
            while (true)
            {
                if (_client.ConnectionState == ConnectionState.Disconnected)
                {
                    connTime++;
                    if (connTime > 10)
                    {
                        // if only we could call the log event method ourselves without compile error.
                        //await _client.Log.Invoke(new LogMessage(LogSeverity.Info, "Gateway", "Has not been 
                        //connected for 30 seconds. Assuming deadlock in connector.")).ConfigureAwait(false);
                        await _client.StartAsync().ConfigureAwait(false);
                    }
                }
                else if (_client.ConnectionState == ConnectionState.Connected)
                {
                    connTime = 0;
                }

                await Task.Delay(1000).ConfigureAwait(false);
            }
        }

        private async Task OnReady()
        {
            var manager = FarmRoomManager.GetRoomRouting();
            manager.Load(_client);
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

        private async Task HandleReactionAddedAsync(
            Cacheable<IUserMessage, ulong> arg1, 
            ISocketMessageChannel arg2, 
            SocketReaction arg3)
        {
            var manager = FarmRoomManager.GetRoomRouting();
            var room = manager.GetRoom(arg2);
            var user = room.Cash.Clinets.FirstOrDefault(_ => _.Id == arg3.UserId);

            if (arg3.Emote.ToString() != "💰" || user == null)
            {
                return;
            };

            var lMessage = await arg1.GetOrDownloadAsync();
            var orderId = lMessage.Embeds.FirstOrDefault()
                .Fields.FirstOrDefault(_=>_.Name == "Order")
                .Value;
            var curOrder = room.Orders.FirstOrDefault(_ => _.OrderId.Contains(orderId));

            if (curOrder == null && curOrder?.Performer != null)
            {
                Console.WriteLine($"{DateTime.Now}: Order adoption canceled \n" +
                    $"Order performer alredy exist");
                return;
            }

            curOrder.SetPerformer(user);
            lMessage.ModifyAsync(msg => msg.Embed = curOrder.GetOrderEmbed()).Wait();
            Thread.Sleep(2000);
            await  lMessage.RemoveAllReactionsAsync();
            OrderWatchManager.AddOperation(new AcceptOrder(curOrder));
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);

            if (message.Author.IsBot) return;

            if (!(message.Channel.Name.ToLower().Equals("minimal") ||
                message.Channel.Name.ToLower().Equals("wage")))
            {
                if (!FarmRoom.IsMath(message.Channel.Name))
                {
                    await context.Channel.SendMessageAsync("Вы ввели некорректное название канала. " +
                        "Проверьте очередность написание аргументом " +
                        "и отсутствие лишних символов. Формат [server_server]-[ud/eu]-[aliance/horde]-[main/classic]");
                    return;
                }
            }



            int argPos = 0;

            if (message.HasStringPrefix("/", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }
    }
}
