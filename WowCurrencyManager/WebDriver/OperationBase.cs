using OpenQA.Selenium;
using WowCurrencyManager.Room;

namespace WowCurrencyManager.WebDriver
{
    public abstract class OperationBase
    {
        public string Url { protected set; get; }
        public int Priorety { protected set; get; }
        public DiscordRoom Sender { protected set; get; }

        public string PassThePage(IWebDriver driver)
        {
            driver.Navigate().GoToUrl(Url);
            return Url;                
        }
    }
}
