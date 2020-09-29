using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowCurrencyManager.Room
{
    public abstract class ClientBase
    {
        public string Name { protected set; get; }
        public ulong Id { protected set; get; }
        public int Balance { protected set; get; }
        public void SetGoldAmount(int value)
        {
            Balance = value;
        }

        public void SetName(string name)
        {
            Name = name;
        }
    }
}
