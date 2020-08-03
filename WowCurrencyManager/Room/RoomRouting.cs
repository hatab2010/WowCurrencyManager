﻿using Discord.Commands;
using Discord.Rest;
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
        private List<DiscordRoom> _rooms = new List<DiscordRoom>();

        public static RoomRouting GetRoomRouting()
        {
            if (instance == null)
                instance = new RoomRouting();

            return instance;
        }

        //internal void SetBalance(SocketCommandContext context)
        //{
        //    var room = GetRoom(context.Channel.Name);
        //    var client = room.GetClient(context.User);
        //}

        public DiscordRoom GetRoom(string channelName)
        {
            var room = _rooms.FirstOrDefault(_ => _.Name == channelName);
            if (room == null)
            {
                room = new DiscordRoom(channelName);
                _rooms.Add(room);
                return room;
            }
            else
            {
                return room;
            }
        }

    }
}
