﻿using System;
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
        private int _reserved;

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
                string serverName;
                try
                {
                    serverName = parentEL.FindElement(By.ClassName("products__name")).Text;
                }
                catch (Exception)
                {
                    continue;
                }
                
                var formatLabel = Regex.Replace(serverName, "[’]", "").ToLower();

                var isCurrent = Regex.IsMatch(formatLabel, $@"{_server} \[\w*\] - {_fraction}");

                if (isCurrent)
                {
                    _instance = parentEL;
                    break;
                }
           
            }

            var reservedEl = _instance.FindElements(By.ClassName("products__description-item"))
                    .FirstOrDefault(_ => _.Text.Contains("Reserved"))
                    .FindElement(By.ClassName("products__description-info"));

            _reserved = int.Parse(reservedEl.Text);
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
            var visibleButtons = GetOrderStateActionButtons();

            isActive = visibleButtons.FirstOrDefault(_ => _.Text == "Deactivate") != null;

            return isActive;
        }

        public void SetPrice(decimal value)
        {
            Interaction((el) =>
            {
                var priceButton = el.FindElement(By.ClassName("g2g_products_price"));
                priceButton.Click();

                var input = _driver.WaitElement(By.ClassName("input-large"));
                input.Clear();
                var resukt = value - (decimal)0.00001;
                input.SendKeys((resukt).ToString().Replace(",", "."));
                _driver.FindElement(By.CssSelector(".btn.btn--green.editable-submit")).Click();
            });
        }

        public void SetAmount(int value)
        {
            if (value > 400)
            {
                SetStateOrder(true);

                Interaction((el) =>
                {
                    var stockButton = _instance.FindElement(By.ClassName("g2g_actual_quantity"));
                    stockButton.Click();

                    var StockInput = _driver.WaitElement(By.CssSelector(".editable-input > input"));
                    StockInput.Clear();
                    StockInput.SendKeys((value + _reserved).ToString());

                    _driver.FindElement(By.CssSelector(".btn.btn--green.editable-submit")).Click();
                });
            }
            else
            {
                SetStateOrder(false);
            }
        }

        private void SetStateOrder(bool isActive)
        {
            if (isActive)
            {
                var curState = CheckActive();

                if (!curState)
                { 
                    var relistButton = GetOrderStateActionButtons().First(_=>_.Text == "Relist");
                    relistButton.Click();
                    _driver.FindElement(By.CssSelector(".btn.btn--green.product-action-page")).Click();
                }
            }
            else
            {
                var curState = CheckActive();

                if (curState == true)
                {
                    if (curState)
                    {
                        var relistButton = GetOrderStateActionButtons().First(_ => _.Text == "Deactivate");
                        relistButton.Click();
                        _driver.FindElement(By.CssSelector(".btn.btn--green.product-action-page")).Click();
                    }
                    return;
                }
            }
        }

        private IReadOnlyCollection<IWebElement> GetOrderStateActionButtons()
        {
            IReadOnlyCollection<IWebElement> buttons = null;
            Interaction((el) => 
            {
                buttons = el.FindElements(By.CssSelector(".manage-listing__actions-list a"));
            });

            return buttons;
        }
    }
}