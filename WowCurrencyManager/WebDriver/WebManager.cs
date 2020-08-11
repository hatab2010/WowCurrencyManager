using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

            try
            {
                //Open new order
                var newOrders = driver.FindElements(By.ClassName("sales-history__table-row-unread"));

                //Check for room exist
                var root = RoomRouting.GetRoomRouting();

                IWebElement currentOrder = null;
                DiscordRoom currentRoom = null;

                foreach (var el in newOrders)
                {
                    foreach (var room in root.Rooms)
                    {
                        var formatedLabel = Regex.Replace(el.FindElement(By.ClassName("sales-history__product-name")).Text.ToLower(), "[’]", "");

                        if (formatedLabel.Contains(room.Server) && formatedLabel.Contains(room.Fraction))
                        {
                            currentOrder = el;
                            currentRoom = room;

                            goto RoomExist;
                        };
                        
                    }
                }

                return;

                RoomExist:

                var operationLink = currentOrder.FindElement(By.ClassName("sales-history__product-id"));
                operationLink.Click();

                //View order details
                driver.WaitElement(By.ClassName("progress_gr"))
                    .Click();

                //Parse order info
                var orderServer = driver.WaitElement(By.XPath(BuildXpathString("Server"))).Text;
                var fraction = driver.FindElement(By.XPath(BuildXpathString("Faction"))).Text;
                var buyer = driver.FindElement(By.ClassName("seller__name")).Text;

                var OrderNumberEl = driver.FindElement(By.CssSelector(".trade__order__top-num span"));
                var orderNumber = Regex.Match(OrderNumberEl.Text, @"№\d*").Value;
                var amount = int.Parse(driver.FindElement(By.XPath("//td[@class = 'sales-history__table-quantity' and contains(@data-th, 'QTY.')]")).Text);

                string BuildXpathString(string fieldName)
                {
                    return $"//span[contains(@class, 'game-info__title') and contains(text(), '{fieldName}')]/../span[contains(@class, 'game-info__info')]";
                }

                driver.WaitElement(By.XPath("//a[contains(@class, 'progress_gr') and contains(text(), 'Start Trading')]"))
                    .Click();

                //TODO проверить на принятие ордера
                driver.WaitElement(By.ClassName("trade__field-input")).SendKeys(amount.ToString());

                //Create order in discord room
                var myOrder = new Model.G2gOrder()
                {
                    Buyer = buyer,
                    OrderId = orderNumber,
                    Amount = amount,
                    Server = orderServer,
                    Fraction = fraction
                };

                currentRoom.SetOrder(myOrder);

                while (myOrder.Performer == null)
                {
                    Thread.Sleep(500);
                    //TODO добавить выход из цикла, если никто не примет ордер
                }
                
                driver.WaitElement(By.CssSelector(".list-action.trade__list-action3 a")).Click();

                var okButton = driver.WaitElement(By.CssSelector(".btn.trade-history__btn"));

                while (!okButton.Displayed)
                {
                    Thread.Sleep(300);
                }

                okButton.Click();

                var messageButton = driver.WaitElement(By.XPath("//a[contains(@target, 'g2gcw') and contains(@class, 'list-action__btn-default')]"));
                var chatUrl = messageButton.GetAttribute("href");
                driver.Navigate().GoToUrl(chatUrl);

                //Send message to byer
                var messageStr = "hello friend, gold sent expect 1 hour and get your order please " +
                    "give a good rating and follow me we have the fastest delivery and cheaper gold, 200% " +
                    "safe gold, handmade. We do not buy gold on other sites or from other sellers! even if" +
                    " gold is not available, you can write to us and we completed your order as soon as possible" +
                    "waiting for you again ";

                driver.WaitElement(By.TagName("textarea")).SendKeys(messageStr);
                driver.WaitElement(By.TagName("textarea")).SendKeys(Keys.Enter);                
            }
            catch (Exception)
            {
                
            }    
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

            var currentCurrencyLavel = driver.FindElement(By.ClassName("header__country-text"));

            //if (currentCurrencyLavel.Text.Contains("EUR"))
            //{
            //    var inCur = driver.WaitElement(By.Id("reg_cur"));
            //    inCur.Click();
            //    inCur
            //}

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
