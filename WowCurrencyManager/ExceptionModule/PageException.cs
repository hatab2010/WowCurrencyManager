using WowCurrencyManager.Bot;
using WowCurrencyManager.Bot.Pages;

namespace WowCurrencyManager.ExceptionModule
{
    public class PageException : ExceptionBase
    {
        public PageException(OperationType operationType, WorldPart worldPart, GameVersion version) : base
            (
            "Отсутствуют URL адрес удовлетворяющий условию поиска. \n" +
            $"Операция: {operationType.ToString()}\n" +
            $"Расположение сервера: {worldPart.ToString()}\n" +
            $"Версия игры: {version}\n", ExceptionType.Constant
            )
        {
        }

        public PageException(IOperationParams values) : base
            (
            "Отсутствуют URL адрес удовлетворяющий условию поиска. \n" +
            $"Операция: {values.operationType.ToString()}\n" +
            $"Расположение сервера: {values.ServerWordPart.ToString()}\n" +
            $"Версия игры: {values.WowServerType}\n", ExceptionType.Constant
            )
        {
        }
    }
}
