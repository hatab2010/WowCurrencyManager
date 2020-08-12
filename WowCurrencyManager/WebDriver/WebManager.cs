using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WowCurrencyManager.Room;

namespace WowCurrencyManager.WebDriver
{
    enum OperationPage
    {
        MyOrders, Currency, ExternalOrder, OrderProcess, SendMessage
    }

    public interface IBalance
    {
        int Balance { get; }
    }

    public class WebManager
    {
        private static WebManager instance;
        private IWebDriver _driver;
        private List<IOperation> _opertions = new List<IOperation>();

        public WebManager()
        {
            InitDriver();
            DiscordRoom.Changed += OnBalanceChanged;

            Task.Run(Process);
        }

        private void Process()
        {
            while (true)
            {
                if (_opertions.Count != 0)
                {
                    IOperation curOperation;

                    lock (_opertions)
                    {
                        curOperation = _opertions.FirstOrDefault();
                    }

                    if (curOperation != null)
                    {
                        var isSuccess = false;

                        do
                        {
                            try
                            {
                                curOperation.Start(_driver);
                                isSuccess = true;
                            }
                            catch (Exception ex)
                            {
                                Task.Delay(15000).Wait();
                            }

                        } while (!isSuccess);
                        

                        lock (_opertions)
                        {
                            _opertions.Remove(curOperation);
                        }
                    }
                }
                else
                {
                    lock (_opertions)
                    {
                        _opertions.Add(new WaitOrder());
                    }

                }                
            }
        }

        private void OnBalanceChanged(DiscordRoom room)
        {
            var prevBalanceOperation = _opertions.FirstOrDefault(_ => _.Sender == room && _ is EditOrderOperaion);

            if (prevBalanceOperation == null)
            {
                lock (_opertions)
                {
                    _opertions.Add(new EditOrderOperaion(room));
                }                
            }            
        }

        public static WebManager InitManager()
        {      
            if (instance == null)
            {
                return new WebManager();
            }
            else
            {
                return instance;
            }
        }

        private void InitDriver()
        {
            var dataPath = $"{Directory.GetCurrentDirectory()}/Data";
            var options = new ChromeOptions();

            options.AddArgument($"--user-data-dir={dataPath}");
            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = new TimeSpan(15000);
            _driver.Navigate().GoToUrl("https://www.g2g.com/order/sellOrder?status=5");

            while (true)
            {
                Thread.Sleep(5000);
                if (!_driver.Url.Contains("login") || !_driver.Url.Contains("sso/device"))
                    break;
            }

        }
    }

    public class WebBot : ChromeDriver
    {
        public WebBot()
        {
            
        }
    }
}
