using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowCurrencyManager.Room
{
    public abstract class PurseBase
    {
        public int Balance { private set; get; }
        public void SetBalance(int value)
        {
            Balance = value;
        }
    }
}
