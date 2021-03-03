using OpenQA.Selenium;
using System;
using System.Threading;
using WebBotCore;
using WebBotCore.Interfase;
using WebBotCore.Operation;

namespace WowCurrencyManager.v2.Operation
{
    public class AutorizationOperation : OperationBase
    {
        protected override object Action()
        {
            var xPathLoginButton = "//a[contains(@href, '/sso/login')]";
            driver.Navigate().GoToUrl("https://www.g2g.com/");
            var loginButton = driver.WaitElement(By.XPath(xPathLoginButton), 30000);
            loginButton.Click();

            while (!driver.Url.Contains("https://www.g2g.com/") ||
                   !driver.IsElementExist(By.ClassName("header__profile-name")))
            {
                Thread.Sleep(500);
            }

            Console.WriteLine("Login in g2g success");
            return null;
        }
    }
}
