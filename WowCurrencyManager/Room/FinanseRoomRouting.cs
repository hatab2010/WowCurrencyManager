using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WowCurrencyManager.Room
{
    public class FinanseRoomRouting
    {
        private static FinanseRoomRouting _instance;
        public Data Cash = new Data();
        private string _cashPath = Directory.GetCurrentDirectory()+"/cash.bin";

        public static FinanseRoomRouting Get()
        {
            if (_instance == null)
            {
                _instance = new FinanseRoomRouting();
                _instance.DataCheck();
                FinanceClient.Changed += _instance.SaveData;
            }

            return _instance;
        }

        static FinanseRoomRouting()
        {
            
            
        }

        private void DataCheck()
        {
            if (File.Exists(_cashPath))
            {
                LoadData();
            }
            else
            {
                Cash = new Data();
            }
        }

        public void CreateRoom(FinanceRoom room)
        {
            Cash.Rooms.Add(room);
        }

        private void SaveData()
        {
            using (Stream stream = File.Open(_cashPath, FileMode.Create))
            {
                var foramter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                foramter.Serialize(stream, Cash);
                stream.Close();
            }
        }

        private void LoadData()
        {
            using (Stream stream = File.Open(_cashPath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                Cash = (Data)binaryFormatter.Deserialize(stream);
            }
        }

        [Serializable]
        public class Data
        {            
            public List<FinanceRoom> Rooms = new List<FinanceRoom>();
        }
    }

    [Serializable]
    public class FinanceClient
    {
        static public Action Changed;
        private int balance;

        public int Balance
        {
            get => balance; private
            set
            {
                balance = value;
                Changed?.Invoke();
            }
        }
        public ulong Id { private set; get; }
        public string Name { private set; get; }
        public FinanceClient(ulong id, string name)
        {
            Id = id;
            Name = name;
        }

        internal void AddBalance(int value)
        {
            Balance += value;
        }

        public void RefrashBalance()
        {
            Balance = 0;
        }

        internal void RemoveBalance(int value)
        {
            Balance -= value;
        }

        public Embed SellEmbedBuild(int value)
        {
            var builder = new EmbedBuilder();

            builder.WithDescription($"{Name} провел операцию: {value}");
            builder.Color = Color.Blue;

            return builder.Build();
        }

        public Embed RemoveEmbedBuilder(int value)
        {
            var builder = new EmbedBuilder();

            builder.WithDescription($"{Name} отменил операцию: {value}");
            builder.Color = Color.DarkBlue;

            return builder.Build();
        }

        public Embed PayEmbedBuild()
        {
            var builder = new EmbedBuilder();
            builder.WithDescription($"{Name} итоговый дебет: {Balance}");
            return builder.Build();
        }
    }

    [Serializable]
    public class FinanceRoom
    {
        public ulong Id { private set; get; }
        public List<FinanceClient> Clients { private set; get; } = new List<FinanceClient>();

        public FinanceRoom(ulong id)
        {
            Id = id;
        }

        internal void AddClient(FinanceClient client)
        {
            Clients.Add(client);
        }

        public List<Embed> PayOperation()
        {
            List<Embed> result = new List<Embed>();

            foreach (var item in Clients)
            {
                result.Add(item.PayEmbedBuild());
                item.RefrashBalance();
            }

            return result;
        }
    }
}
