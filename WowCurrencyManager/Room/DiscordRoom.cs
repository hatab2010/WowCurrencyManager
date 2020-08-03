using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WowCurrencyManager.Room
{
    public enum BalanceState
    {
        Good, Bad
    }

    public class Order
    {
        public int Value;
        public RoomClient Performer;
    }

    public class DiscordRoom
    {
        public List<RoomClient> Clients = new List<RoomClient>();
        public string Name { private set; get; }
        public int RoomBalance { private set; get; }
        public int Balance { private set; get; }
        public RestUserMessage LastBalanceMessage { get; internal set; }
        public Order ActiveOrder;

        private bool _isValid = true;

        public DiscordRoom(string name)
        {
            Name = name;
        }

        public RoomClient GetClient(ulong id, string name)
        {
            RoomClient client = Clients.FirstOrDefault(_ => _.Id == id);

            if (client == null)
            {
                client = new RoomClient(id, name);
                Clients.Add(client);
                return client;
            }
            else
            {
                return client;
            }
        }

        public void RemoveClient(ulong id)
        {
            var client = Clients.FirstOrDefault(_ => _.Id == id);

            if (client == null) return;
            Clients.Remove(client);

            UpdateBalance();
        }

        public void UpdateBalance()
        {
            int result = 0;

            if (Clients == null || Clients.Count == 0)
                return;

            foreach (var item in Clients)
            {
                result += item.Balance;
            }

            Balance = result;
        }
    }

    public class RoomClient : PurseBase
    {
        public ulong Id { private set; get; }
        public string Name { private set; get; }

        public RoomClient(ulong id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
