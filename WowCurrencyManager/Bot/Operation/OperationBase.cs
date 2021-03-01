using OpenQA.Selenium;
using System;
using WowCurrencyManager.ExceptionModule;
using WowCurrencyManager.Room;

namespace WowCurrencyManager.Bot
{
    public abstract class OperationBase : IOperation
    {
        public FarmRoom Sender { get; protected set; }

        protected virtual void DriverAction(IWebDriver driver)
        {

        }

        public void Execute(IWebDriver driver)
        {
            try
            {
                DriverAction(driver);
            }
            catch (ExceptionBase ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
