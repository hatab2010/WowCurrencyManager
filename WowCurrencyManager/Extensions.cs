using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using WowCurrencyManager.WebElement;

namespace WowCurrencyManager
{
    public static class Extensions
    {
        static int _time;

        public static string ToUppercase(this string value)
        {
            var builder = new StringBuilder();
            var firstChar = value[0];
            var other = value.Remove(value[0]);
            builder.Append(firstChar.ToString().ToUpper());
            builder.Append(other);
            return builder.ToString();
        }

        public static IWebElement WaitElement(this IWebDriver driver, By findeOption)
        {
            var timer = 15000;

            while (true)
            {
                try
                {
                    return driver.FindElement(findeOption);
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                    timer -= 1000;

                    if (timer < 0)
                    {
                        throw;
                    }
                }
            }
        }

        public static PinnedElement GetPinnedElement(this IWebDriver driver, By findeOption)
        {
            return new PinnedElement(driver, findeOption);
        }

        public static Products GetProductsEl(this IWebDriver driver, string server, string fraction)
        {
            return new Products(driver, server, fraction);
        }

        public static void WaitToRefrash(this IWebDriver driver)
        {
            var overScreenClass = "CustomeOverscreen654687";
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("var div = document.createElement('div'); div.className = 'CustomeOverscreen654687'; div.setAttribute('style', 'width: 99000px; height: 9999999px; position: absolute; top: 0; left: 0; z-index: 999; background-color: blak; opacity: 0.8'); document.body.prepend(div);");
            var timer = 0;

            while (true)
            {
                try
                {
                    var el = driver.FindElement(By.ClassName(overScreenClass));
                    Thread.Sleep(500);
                    timer += 500;

                    if (timer > 15000)
                    {
                        goto Exception;
                    }                        
                }
                catch (Exception)
                {
                    break;
                }               
            }

            return;

            Exception:
             throw new Exception();
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
                catch (Exception)
                {
                    Thread.Sleep(1000);
                    timer -= 1000;

                    if (timer < 0)
                    {
                        throw;
                    }
                }
            }

        }
    }
}
