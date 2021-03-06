﻿using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowCurrencyManager.Room
{
    public class FarmRoomManager
    {
        private static string _cashPath = Directory.GetCurrentDirectory() + "/roomCash.bin";
        public RestUserMessage LastBalanceInfoMessage;
        private static FarmRoomManager instance;

        public List<FarmRoom> Rooms { private set; get; } = new List<FarmRoom>();

        public static FarmRoomManager GetRoomRouting()
        {
           if (instance == null)
                instance = new FarmRoomManager();

            return instance;
        }

        public void Load(DiscordSocketClient client)
        {
            var data = GetFarmRoomData();
            if (data == null) return;

            foreach (var item in data)
            {
                try
                {
                    var guild = client.GetGuild(item.GuildId);
                    SocketTextChannel channel = null;

                    if (guild == null) continue;
                    
                    channel =  client.GetGuild(item.GuildId).GetTextChannel(item.ChannelId);

                    if (channel == null) continue;

                    var room = GetRoom(channel);
                    room.SetCash(item);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void SaveCashRooms()
        {
            var roomsData = Rooms.Select(_ => _.Cash);

            if (roomsData.Count() == 0) return;

            lock (roomsData)
            {
                using (Stream stream = File.Open(_cashPath, FileMode.Create))
                {
                    var formater = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    formater.Serialize(stream, roomsData.ToList());
                    stream.Close();
                }
            }
        }

        private IEnumerable<FarmRoomData> GetFarmRoomData()
        {
            if (!File.Exists(_cashPath)) return null;
            IEnumerable<FarmRoomData> result;
            using (Stream stream = File.Open(_cashPath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                result = (IEnumerable<FarmRoomData>)binaryFormatter.Deserialize(stream);
                stream.Close();
            }

            return result;
        }

        //TODO убрать создание комнаты
        public FarmRoom GetRoom(ISocketMessageChannel channel)
        {
            var room = Rooms.FirstOrDefault(_ => channel.Id == _.Cash.ChannelId);
            if (room == null)
            {
                room = new FarmRoom(channel);
                Rooms.Add(room);
                return room;
            }
            else
            {
                return room;
            }
        }

        public List<FarmRoom> GetRooms()
        {
            return Rooms;
        }
    }
}
