using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WowCurrencyManager.Room;
using WowCurrencyManager.WebElement;

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

    public abstract class OperationBase
    {
        public string Url { protected set; get; }
        public int Priorety { protected set; get; }
        public DiscordRoom Sender { protected set; get; }

        public string PassThePage(IWebDriver driver)
        {
            driver.Navigate().GoToUrl(Url);
            return Url;                
        }
    }

    public interface IOperation
    {
        //Action<IWebDriver> Start { get; }
        DiscordRoom Sender { get; }

        //string PassThePage(IWebDriver driver);

        void Start(IWebDriver driver);
    }

    //public class WatchSellPriceOperation : OperationBase, IOperation
    //{
    //    public Action<IWebDriver> Start { private set; get; }


    //    public WatchSellPriceOperation(DiscordRoom sender)
    //    {
    //        Url = "";
    //        Sender = sender;
    //    }
    //}

    public class WaitOrder : IOperation
    {
        private IWebDriver _driver;
        static Stopwatch _lastWatchTime = new Stopwatch();
        public DiscordRoom Sender { private set; get; }
        private string _url = "https://www.g2g.com/order/sellOrder?status=5";

        public void Start(IWebDriver driver)
        {
            _driver = driver;

            if (driver.Url != _url)
            {
                Update();
            }

            if (_lastWatchTime.ElapsedMilliseconds > 60000)
            {
                Update();
            }
            else
            {
                Thread.Sleep(1000);
            }

            _driver.FindElement()            
        }

        private void Update()
        {
            _lastWatchTime.Restart();
            _driver.Navigate().GoToUrl(_url);
        }
    }

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

            var server = Regex.Replace(parseStrgs[0], "[_]", " ").ToLower();
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

            selectOption(driver.WaitElement(By.CssSelector("#select2-server-container")), server);
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

            var PriveEl = driver
                .WaitElement(By.CssSelector(".products__cells .products__exch-rate"));

            var lowPriceVlue = decimal.Parse(Regex.Match(PriveEl.Text, @"\d*[.]\d*").Value.Replace(".", ","));

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
            var order = driver.GetProductsEl(server, fraction);
            order.SetAmount(Sender.Balance);
            if (lowPriceVlue > Sender._minLos)
            {
                order.SetPrice(lowPriceVlue);
            }
            else
            {
                order.SetPrice(Sender._minLos);
            }
            
            //var Orse = new Products(,);
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

    public class WebManager
    {
        private static WebManager instance;
        private IWebDriver _driver;
        private List<IOperation> _opertions = new List<IOperation>();

        public WebManager()
        {
            InitDriver();
            DiscordRoom.BalanceChanged += OnBalanceChanged;

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

        private void FindeOrder()
        {

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
        }
    }

    public class WebBot : ChromeDriver
    {
        public WebBot()
        {
            
        }
    }
}
