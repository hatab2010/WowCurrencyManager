using OpenQA.Selenium;
using WowCurrencyManager.Room;

namespace WowCurrencyManager.Bot
{
    public interface IOperation
    {
        //Action<IWebDriver> Start { get; }
        FarmRoom Sender { get; }

        //string PassThePage(IWebDriver driver);

        void Execute(IWebDriver driver);
    }
}
