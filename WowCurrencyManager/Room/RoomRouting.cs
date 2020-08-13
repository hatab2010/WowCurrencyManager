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
    public class FarmRoomRouting
    {
        public RestUserMessage LastBalanceInfoMessage;
        private static FarmRoomRouting instance;

        public List<FarmRoom> Rooms { private set; get; } = new List<FarmRoom>();

        public static FarmRoomRouting GetRoomRouting()
        {
            if (instance == null)
                instance = new FarmRoomRouting();

            return instance;
        }

        //TODO убрать создание комнаты. Гет метод неяяно меняет состояние объекта
        public FarmRoom GetRoom(ISocketMessageChannel channel)
        {
            var room = Rooms.FirstOrDefault(_ => _.Name == channel.Name);
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
