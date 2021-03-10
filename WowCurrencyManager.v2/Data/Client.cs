﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowCurrencyManager.v2.Data
{
    public class Client
    {
        public ulong Id { set; get; }
        public string Name { set; get; }
        public string AvatarUrl { set; get; }

        public List<Purse> Purses { set; get; }
        public List<Order> Orsers { set; get; }
    }
}
