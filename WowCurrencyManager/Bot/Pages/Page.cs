using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowCurrencyManager.Bot.Pages
{
    public class Page : IOperationParams, IUrl
    {
        public OperationType operationType { get; set; }
        public WorldPart ServerWordPart { get; set; }
        public GameVersion WowServerType { get; set; }
        public string Url { get; set; }

        public static bool Eq(IOperationParams a, IOperationParams b)
        {

            if (a.operationType == b.operationType &&
                a.ServerWordPart == b.ServerWordPart &&
                a.WowServerType == b.WowServerType)
                return true;

            return false;
        }

        public bool Eq(IOperationParams b)
        {
            var a = (IOperationParams)this;
            return Eq(a, b);
        }
    }
}
