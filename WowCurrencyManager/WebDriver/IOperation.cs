using OpenQA.Selenium;
using WowCurrencyManager.Room;

namespace WowCurrencyManager.WebDriver
{
    public interface IOperation
    {
        //Action<IWebDriver> Start { get; }
        FarmRoom Sender { get; }

        //string PassThePage(IWebDriver driver);

        void Start(IWebDriver driver);
    }
}
