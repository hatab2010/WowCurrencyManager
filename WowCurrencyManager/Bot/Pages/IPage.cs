using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowCurrencyManager.Bot.Pages
{
    public enum WorldPart
    {
        EU, US
    }

    public enum OperationType
    {
        EditOrder, WhaitOrder, LookOrders
    }

    public interface IOperationParams
    {
        OperationType operationType { get; set; }
        WorldPart ServerWordPart { get; set; }
        GameVersion WowServerType { get; set; }        
    }

    public interface IEquals
    {
        bool Eq(IOperationParams a, IOperationParams b);
    }

    public interface IUrl
    {
        string Url { get; set; }
    }
}
