using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Reflection;
using WowCurrencyManager.v2.Model;
using System;
using Microsoft.Extensions.DependencyInjection;
using WowCurrencyManager.v2.Data;
using WowCurrencyManager.v2.Modules;
using System.Collections.Generic;
using Data;

namespace WowCurrencyManager.v2
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
            //G2gCore.Init();

            var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
            _client = new DiscordSocketClient(_config);
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            var token = "NzQzMjQ4MjYwOTQwMTY5MjU3.XzR54g._ZnLPb0dersJmfnauxNBinyphnE";

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
            _client.ReactionRemoved += HandleReactionRemovedAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private Task HandleReactionRemovedAsync(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            throw new NotImplementedException();
        }

        private async Task HandleReactionAddedAsync(
            Cacheable<IUserMessage, ulong> arg1,
            ISocketMessageChannel arg2,
            SocketReaction arg3)
        {
            Client user = null;
            Channel channel = null;

            using (var db = new MobileContext())
            {
                user = db.Clients.FirstOrDefault(_ => _.Id == arg3.UserId);
                channel = db.Channels.FirstOrDefault(_ => _.Id == arg2.Id);
            }

            if (arg3.Emote.ToString() != "💰" 
                || user == null
                || channel == null)
            {
                return;
            };

            var lMessage = await arg1.GetOrDownloadAsync();
            var orderId = ulong.Parse(lMessage.Embeds.FirstOrDefault()
                .Fields.FirstOrDefault(_ => _.Name == "Order")
                .Value);

            Order currentOrder = null;

            using (var db = new MobileContext())
            {
                currentOrder = db.Orders.FirstOrDefault(_ => _.Id == orderId);
            }

            if (currentOrder == null && currentOrder?.Clients != null)
            {
                Console.WriteLine($"{DateTime.Now}: Order adoption canceled \n" +
                    $"Order performer alredy exist");
                return;
            }

            currentOrder.SetPerformer(user);
            lMessage.ModifyAsync(msg => msg.Embed = currentOrder.GetOrderEmbed()).Wait();
            await lMessage.RemoveAllReactionsAsync();

            G2gCore.AcceptOrder(currentOrder);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            var openDialog = Dialog.OpenDialogs.FirstOrDefault(_ => _.Channel.Id == context.Channel.Id);
            int argPos = 0;

            if (message.Author.IsBot) return;
            
            if (openDialog != null)
            {
                var FarmRoomDialog = openDialog as RegistrationFarmRoomDialog;
                if (FarmRoomDialog != null)
                {
                    try
                    {
                        await FarmRoomDialog.SendAnswer(message);
                    }
                    catch (Exception ex)
                    {
                        await context.Channel.SendMessageAsync(ex.Message);                     
                    }                    
                }

                return;
            }           

            if (message.HasStringPrefix("/", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {                    
                    Console.WriteLine(result.ErrorReason);
                }
                else
                {
                    await message.DeleteAsync();
                }
            }
        }

        private void RouteCommand()
        {

        }
    }
}
