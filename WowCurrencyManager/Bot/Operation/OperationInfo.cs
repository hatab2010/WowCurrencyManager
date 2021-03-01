using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WowCurrencyManager.Bot.Pages;

namespace WowCurrencyManager.Bot.Operation
{
    public class OperationInfo : IOperationParams
    {
        public OperationType operationType { get; set; }
        public WorldPart ServerWordPart { get; set; }
        public GameVersion WowServerType { get; set; }
    }
}
