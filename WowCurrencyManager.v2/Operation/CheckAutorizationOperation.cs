using OpenQA.Selenium;
using System;
using WebBotCore;
using WebBotCore.Interfase;
using WebBotCore.Operation;

namespace WowCurrencyManager.v2.Operation
{
    public class CheckAutorizationOperation : OperationBase
    {
        public CheckAutorizationOperation(ISiteProfile profile)
        {
            OperationPage = profile.Page;
        }       

        protected override object Action()
        {
            driver.Navigate().GoToUrl(OperationPage.AbsoluteUri);
            var isLoginButtonExist = driver.IsElementExist(By.XPath("//a[contains(@href, '/sso/login')]"));

            return isLoginButtonExist == false;
        }
    }
}
