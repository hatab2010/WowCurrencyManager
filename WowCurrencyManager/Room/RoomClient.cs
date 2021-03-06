﻿using System;

namespace WowCurrencyManager.Room
{
    [Serializable]
    public class RoomClient : ClientBase
    {        
        public string AvatarUrl { private set; get; }        

        public RoomClient(ulong id, string name, string avatarUrl) 
        {
            Id = id;
            Name = name;
            AvatarUrl = avatarUrl;
        }
    }
}
