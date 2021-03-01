using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using WowCurrencyManager.ExceptionModule;
using WowCurrencyManager.WebElement;

namespace WowCurrencyManager
{
    public static class Extensions
    {
        public static string FirstCharUp(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }

        public static void WaitAjaxFinish(this IWebDriver driver)
        {
            var js = (IJavaScriptExecutor)driver;

            while (true)
            {
                var ajaxCompleted = (bool)js.ExecuteScript("return jQuery.active == 0");
                Thread.Sleep(300);
                if (ajaxCompleted) break;
            }           
        }

        public static IWebElement WaitElement(this IWebDriver driver, By findeOption, int second = 15)
        {
            var timer = second * 1000;

            while (true)
            {
                try
                {
                    return driver.FindElement(findeOption);
                }
                catch (System.Exception ex)
                {
                    Thread.Sleep(1000);
                    timer -= 1000;

                    if (timer < 0)
                    {
                        throw new ExceptionBase(ex, ExceptionType.Default);
                    }
                }
            }
        }

        public static PinnedElement GetPinnedElement(this IWebDriver driver, By findeOption)
        {
            return new PinnedElement(driver, findeOption);
        }

        public static string FormatesServerName(this string value)
        {
            var result = Regex.Replace(value, "[’']?", "").ToLower();
            return result;
        }

        public static Products FindProductEl(this IWebDriver driver, string server, string fraction)
        {
            Products result = null;
            try
            {
                result = new Products(driver, server, fraction);
            }
            catch (System.Exception)
            {
                Console.WriteLine($"Products {server} {fraction} not found in page {driver.Url}");
            }

            return result;
        }

        public static void WaitToRefrash(this IWebDriver driver)
        {
            var overScreenClass = "CustomeOverscreen654687";
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("var div = document.createElement('div'); div.className = " +
                "'CustomeOverscreen654687'; div.setAttribute('style', 'width: 99000px; " +
                "height: 9999999px; position: absolute; top: 0; left: 0; z-index: 999; " +
                "background-color: blak; opacity: 0.8'); document.body.prepend(div);");
            var timer = 0;

            while (true)
            {
                try
                {
                    var el = driver.FindElement(By.ClassName(overScreenClass));
                    Thread.Sleep(500);
                    timer += 500;

                    if (timer > 20000)
                    {
                        driver.Navigate().GoToUrl(driver.Url);

                        if (timer > 40000)
                            goto Exception;
                    }                        
                }
                catch (System.Exception)
                {
                    break;
                }               
            }

            return;

            Exception:
             throw new ExceptionBase($"{driver.Url} адресс недоступен");
        }

        public static IReadOnlyCollection<IWebElement> WaitElements(this IWebDriver driver, By findeOption)
        {
            var timer = 15000;

            while (true)
            {
                try
                {
                    return driver.FindElements(findeOption);
                }
                catch (System.Exception ex)
                {
                    Thread.Sleep(1000);
                    timer -= 1000;

                    if (timer < 0)
                    {
                        throw new ExceptionBase("В заданный интервал не произошла перезагрузка страницы",
                            ExceptionType.Default);
                    }
                }
            }

        }
    }
}
