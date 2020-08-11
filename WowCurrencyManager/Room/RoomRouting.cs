using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowCurrencyManager.Room
{
    public class RoomRouting
    {
        public RestUserMessage LastBalanceInfoMessage;
        private static RoomRouting instance;

        public List<DiscordRoom> Rooms { private set; get; } = new List<DiscordRoom>();

        public static RoomRouting GetRoomRouting()
        {
            if (instance == null)
                instance = new RoomRouting();

            return instance;
        }

        //TODO убрать создание комнаты. Гет метод неяяно меняет состояние объекта
        public DiscordRoom GetRoom(ISocketMessageChannel channel)
        {
            var room = Rooms.FirstOrDefault(_ => _.Name == channel.Name);
            if (room == null)
            {
                room = new DiscordRoom(channel);
                Rooms.Add(room);
                return room;
            }
            else
            {
                return room;
            }
        }

        public List<DiscordRoom> GetRooms()
        {
            return Rooms;
        }
    }
}
