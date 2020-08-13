﻿using System;
using System.Text.RegularExpressions;
using System.Threading;
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

            var selectCurrency = driver.WaitElement(By.ClassName("header__country-text"));

            if (!selectCurrency.Text.ToLower().Contains("us"))
            {
                selectCurrency.Click();
                var op = driver.WaitElement(By.XPath("//option[contains(@value, 'USD')]"));
                op.Click();
                var save = driver.WaitElement(By.CssSelector(".header__country-btn .btn"));
                save.Click();
                Thread.Sleep(150000);
            }

            var currentCurrencyLavel = driver.WaitElement(By.ClassName("header__country-text"));

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

            var lowPriceVlue = decimal.Parse(Regex.Match(priceEl.Text, @"\d*[.]\d*").Value.Replace(".", ","));

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
                var result = lowPriceVlue - (decimal)0.00001;
                order.SetPrice(result);
            }
            else
            {
                order.SetPrice(Sender.MinLos);
            }
            
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
