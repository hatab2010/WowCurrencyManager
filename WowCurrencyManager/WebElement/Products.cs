using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace WowCurrencyManager.WebElement
{
    public class Products
    {
        private IWebDriver _driver;
        private string _server;
        private string _fraction;

        private IWebElement _instance;

        public Products(IWebDriver driver, string server, string fraction)
        {
            _driver = driver;
            _server = server;
            _fraction = fraction;

            findEl();
        }

        void findEl()
        {
            var Orders = _driver.WaitElements(By.ClassName("manage__table-row"));

            foreach (var parentEL in Orders)
            {
                var serverName = parentEL.FindElement(By.ClassName("products__name")).Text;
                var formatLabel = Regex.Replace(serverName, "[’]", "").ToLower();

                var isCurrent = Regex.IsMatch(formatLabel, $@"{_server} \[\w*\] - {_fraction}");

                if (isCurrent)
                {
                    _instance = parentEL;
                    //parentEL.FindElement(By.ClassName("g2g_actual_quantit")).Click();

                    //var StockInput = _driver.WaitElement(By.CssSelector(".editable-input > input"));
                    //StockInput.Clear();
                    //StockInput.SendKeys(Sender.Balance.ToString());
                    break;
                }
            }
        }

        private void Interaction(Action<IWebElement> action)
        {
            var timer = 0;

            while (true)
            {
                try
                {
                    action?.Invoke(_instance);
                    break;
                }
                catch (Exception)
                {                    
                    Thread.Sleep(500);
                    findEl();
                    timer += 500;
                    if (timer > 30000)
                    {
                        throw;
                    }
                }
            }
        }

        public bool CheckActive()
        {
            bool isActive = false;
            Interaction((el) =>
            {
                var redButoons = el.FindElements(By.CssSelector(".manage-listing__actions-list a"));

                foreach (var item in redButoons)
                {
                    if (item.Text.Contains("Deactivate"))
                        isActive = true;
                }
            });

            return isActive;
        }

        public void SetAmount(int value)
        {
            Interaction((el) =>
            {
                var stockButton = _instance.FindElement(By.ClassName("g2g_actual_quantity "));
                stockButton.Click();

                var StockInput = _driver.WaitElement(By.CssSelector(".editable-input > input"));
                StockInput.Clear();
                StockInput.SendKeys(value.ToString());

                _driver.FindElement(By.CssSelector("btn.btn--green.editable-submit")).Click;
            });
        }
    }
}
