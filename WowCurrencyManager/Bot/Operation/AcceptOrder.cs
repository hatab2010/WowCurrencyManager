﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using WowCurrencyManager.Model;
using WowCurrencyManager.Room;

namespace WowCurrencyManager.Bot
{
    public class AcceptOrder : OperationBase
    {
        private G2gOrder _order;

        public AcceptOrder(G2gOrder order)
        {
            _order = order;
            Sender = order.RoomSender;
        }

        protected override void DriverAction(IWebDriver driver)
        {

            Sender.OrderAccept(_order);
            driver.Navigate().GoToUrl(_order.OrderPage);
            driver.WaitElement(By.ClassName("trade__field-input")).SendKeys(_order.Amount.ToString());
            driver.WaitElement(By.CssSelector(".list-action.trade__list-action3 a")).Click();
            var okButton = driver.WaitElement(By.CssSelector(".btn.trade-history__btn"));

            while (!okButton.Displayed)
            {
                Thread.Sleep(300);
            }

            okButton.Click();

            var messageButton = driver.WaitElement(By.XPath("//a[contains(@class, 'list-action__btn-default') and contains(@href, 'chat')]"));
            var chatUrl = messageButton.GetAttribute("href");
            driver.Navigate().GoToUrl(chatUrl);

            //Send message to byer
            var messageStr = "hello friend, Gold has been sent expect 1 hour and get your order please give a Thumbs " +
                "UP and follow me we have the fastest delivery and cheaper gold, 200 % safe gold, handmade. We do not buy" +
                " gold on other sites or from other sellers!even if gold is not available, you can write to us and we completed " +
                "your order as soon as possible waiting for you again";

            var secondMessage = "Hello my dear friend. Gold has been sent, wait an hour. If you like our services," +
                " please give us a Thumbs UP and follow us. Come back soon. Good luck to you";

            driver.WaitElement(By.TagName("textarea"), 60 * 7);


            Thread.Sleep(5000); //TODO сделать динамическую проверку чата

            try
            {
                //TODO доработать нотификацию ошибки
                driver.FindElement(By.XPath("//div[contains(@class, 'message-box__text')]/div[contains(text(), 'Gold has been sent')]"));
                driver.WaitElement(By.TagName("textarea")).SendKeys(secondMessage);
            }
            catch (Exception ex)
            {
                driver.WaitElement(By.TagName("textarea")).SendKeys(messageStr);
                Thread.Sleep(2000);
            }

            driver.WaitElement(By.TagName("textarea")).SendKeys(Keys.Enter);
        }
    }
}
