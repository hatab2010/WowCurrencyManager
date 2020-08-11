using System;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using WowCurrencyManager.Room;
using WowCurrencyManager.WebElement;

namespace WowCurrencyManager.WebDriver
{
    public class EditOrderOperaion : IOperation
    {
        public DiscordRoom Sender { private set; get; }

        public EditOrderOperaion(DiscordRoom room)
        {
            Sender = room;
        }

        public void Start(IWebDriver driver)
        {
            //get the sell operation options
            var name = Sender.Channel.Name;

            var parseStrgs = name.Split('-');

            var worldPart = $"[{parseStrgs[1]}]".ToLower();
            var fraction = parseStrgs[2].ToLower();

            //Wath the current price
            switch (worldPart)
            {
                case "[eu]":
                    driver.Navigate().GoToUrl("https://www.g2g.com/wow-classic-eu/gold-27815-27817");
                    break;
                case "[us]":
                    driver.Navigate().GoToUrl("https://www.g2g.com/wow-classic-us/gold-27816-27825");
                    break;
            }

            var currentCurrencyLavel = driver.FindElement(By.ClassName("header__country-text"));

            //if (currentCurrencyLavel.Text.Contains("EUR"))
            //{
            //    var inCur = driver.WaitElement(By.Id("reg_cur"));
            //    inCur.Click();
            //    inCur
            //}

            selectOption(driver.WaitElement(By.CssSelector("#select2-server-container")), Sender.Server);
            selectOption(driver.WaitElement(By.CssSelector("#select2-faction-container")), fraction);

            var sortetLinkEl = driver.GetPinnedElement(By.CssSelector(".sort__link"));

            sortetLinkEl.Interaction((el) =>
            {
                if (!el.Text.ToLower().Contains("lowest price"))
                {
                    el.Click();
                    var sortedItemEl = driver.WaitElement(By.XPath("//a[contains(text(), 'Lowest Price')]"));
                    sortedItemEl.Click();
                    driver.WaitToRefrash();
                }
            });

            var PriveEl = driver
                .WaitElement(By.CssSelector(".products__cells .products__exch-rate"));

            var lowPriceVlue = decimal.Parse(Regex.Match(PriveEl.Text, @"\d*[.]\d*").Value.Replace(".", ","));

            //Go to the my order page
            switch (worldPart)
            {
                case "[eu]":
                    driver.Navigate().GoToUrl("https://www.g2g.com/sell/manage?service=1&game=27815");
                    break;
                case "[us]":
                    driver.Navigate().GoToUrl("https://www.g2g.com/sell/manage?service=1&game=27816");
                    break;
            }

            //Finde room order
            Products order = null;

            try
            {
                order = driver.GetProductsEl(Sender.Server, fraction);                
            }
            catch (Exception)
            {
                return;
            }

            order.SetAmount(Sender.Balance);
            if (lowPriceVlue > Sender.MinLos)
            {
                order.SetPrice(lowPriceVlue);
            }
            else
            {
                order.SetPrice(Sender.MinLos);
            }
            
            //var Orse = new Products(,);
            void selectOption(IWebElement handler, string selectItem)
            {
                var curServer = Regex.Replace(handler.Text, "[’]", "");
                if (!curServer.ToLower().Contains(selectItem))
                {
                    handler.Click();

                    var serverListItems = driver.WaitElements(By.CssSelector("ul.select2-results__options li"));

                    foreach (var item in serverListItems)
                    {
                        var formatedName = Regex.Replace(item.Text, "[’]", "").ToLower();

                        if (formatedName.Contains(selectItem))
                        {
                            item.Click();
                            break;
                        }
                    }                    
                }
            }
        }
    }
}
