using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using WowCurrencyManager.Room;
using WowCurrencyManager.WebElement;
using System.Linq;
using WowCurrencyManager.Bot.Pages;
using WowCurrencyManager.ExceptionModule;
using WowCurrencyManager.Bot.Operation;

namespace WowCurrencyManager.Bot
{
    public enum GameVersion
    {
        Classic, Main
    }

    public class EditOrderOperaion : OperationBase
    {
        public EditOrderOperaion(FarmRoom room)
        {
            Sender = room;
        }

        protected override void DriverAction(IWebDriver driver)
        {
            var formatedWordPart = $"[{Sender.WordPart}]".ToLower();
            var operationInfo = new OperationInfo() {
                operationType = OperationType.LookOrders,
                ServerWordPart = Sender.WordPart,
                WowServerType = Sender.WowServerType
            };

            var lookOrderPage = Global.DefaultSytePage
                .FirstOrDefault(_ => _.Eq(operationInfo));

            if (lookOrderPage == null)
            {
                throw new PageException(operationInfo);
            }

            driver.Navigate().GoToUrl(lookOrderPage.Url);

            var currencyElement = CurrencySwitchElement.Find(driver);
            if (!currencyElement.Value.ToLower().Contains("us"))
            {
                currencyElement.SelectCurrency("USD");
            }

            selectOption(driver.WaitElement(By.CssSelector("#select2-server-container")), Sender.Server);
            selectOption(driver.WaitElement(By.CssSelector("#select2-faction-container")), Sender.Fraction);

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

            //Finde lowest price
            var externalOrderEls = driver.WaitElements(By.ClassName("products__row"));
            var profileUsername = driver.WaitElement(By.ClassName("header__profile-name")).Text.Trim().ToLower();
            IWebElement priceEl = null;

            foreach (var externalOrder in externalOrderEls)
            {
                var sellerName = externalOrder
                    .FindElement(By.ClassName("seller__name")).Text
                    .Trim()
                    .ToLower();

                var isMe = sellerName.Contains(profileUsername);

                if (isMe)
                {
                    continue;
                }
                else
                {
                    priceEl = externalOrder.FindElement(By.ClassName("products__exch-rate"));
                    break;
                }
            }

            var lowPriceVlue = Decimal.Parse(Regex.Match(priceEl.Text, @"\d*[.]\d*").Value,
                NumberStyles.Currency,
                CultureInfo.InvariantCulture);

            Sender.LastMinimalPrice = lowPriceVlue;

            operationInfo.operationType = OperationType.EditOrder;
            var editOrderPage = Global.DefaultSytePage
                .FirstOrDefault(_ => Page.Eq(_, operationInfo));

            if (editOrderPage == null)
            {
                throw new PageException(operationInfo);
            }

            //Go to the my orders page
            driver.Navigate().GoToUrl(editOrderPage.Url);


            //Finde room order
            Products order = null;

            order = driver.FindProductEl(Sender.Server, Sender.Fraction);
            if (order == null)
                return;

            order.SetAmount(Sender.Balance);
            Thread.Sleep(2000);
            driver.WaitAjaxFinish();

            Console.WriteLine($"Time: {DateTime.UtcNow}\n" +
                $"Server: {Sender.Server} \n" +
                $"Minimal price in G2G: {lowPriceVlue} \n" +
                $"Current min los: {Sender.MinLos}");
           
            var ms = "Set minimal price: ";
            if (lowPriceVlue > Sender.MinLos)
            {               
                var result = lowPriceVlue - (decimal)0.00001;
                order.SetPrice(result);
                Console.WriteLine(ms + result);
            }
            else
            {
                order.SetPrice(Sender.MinLos);
                Console.WriteLine(ms + Sender.MinLos);
            }
            Thread.Sleep(4000);
            driver.WaitAjaxFinish();

            void selectOption(IWebElement handler, string selectItem)
            {
                var curServer = Regex.Replace(handler.Text, "[’']", "");
                if (!curServer.ToLower().Contains(selectItem))
                {
                    handler.Click();

                    var serverListItems = driver.WaitElements(By.CssSelector("ul.select2-results__options li"));

                    foreach (var item in serverListItems)
                    {
                        var formatedName = Regex.Replace(item.Text, "[’']", "").ToLower();

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
