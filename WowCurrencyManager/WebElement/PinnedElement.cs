using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace WowCurrencyManager.WebElement
{
    public class PinnedElement
    {
        private IWebDriver driver;
        private IWebElement instance;
        private By findeOption;

        public PinnedElement(IWebDriver driver, By findeOption)
        {
            this.findeOption = findeOption;
            this.driver = driver;

            FindeElement();
        }

        private void FindeElement()
        {
            instance =  driver.WaitElement(findeOption);
        }

        public void Interaction(Action<IWebElement> action)
        {
            while (true)
            {
                try
                {
                    action?.Invoke(instance);
                    break;
                }
                catch (Exception)
                {
                    FindeElement();
                }
            }

        }
    }
}
