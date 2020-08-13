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

        public List<FinanceClient> PayOperation()
        {
            List<FinanceClient> result = new List<FinanceClient>();

            foreach (var item in Clients)
            {
                if (item.Balance == 0 && item.USDBalance == 0)
                    continue;
                result.Add(item);          
            }

            return result;
        }
    }
}
