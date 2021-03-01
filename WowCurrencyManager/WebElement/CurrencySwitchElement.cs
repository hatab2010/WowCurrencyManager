using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace WowCurrencyManager.WebElement
{
    public class CurrencySwitchElement
    {
        public string Value => _element.Text;

        private IWebElement _element = null;
        private IWebDriver _driver;
        private bool isOpen = false;
        

        private CurrencySwitchElement(IWebDriver driver)
        {
            _driver = driver;
            _element = driver.WaitElement(By.ClassName("header__country-text"));
        }

        public static CurrencySwitchElement Find(IWebDriver driver)
        {
            return new CurrencySwitchElement(driver);
        }

        private void Open()
        {
            _element.Click();
            isOpen = true;
        }

        public void SelectCurrency(string currency)
        {
            if (!isOpen) Open();

            var XpathPattern = $"//option[contains(@value, '{currency}')]";

            var option = _driver.WaitElement(By.XPath(XpathPattern));
            option.Click();
            var acceptButton = _driver.WaitElement(By.CssSelector(".header__country-btn .btn"));
            acceptButton.Click();
        }
    }
}
