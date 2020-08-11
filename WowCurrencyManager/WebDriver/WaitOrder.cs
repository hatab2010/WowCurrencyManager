using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using WowCurrencyManager.Room;

namespace WowCurrencyManager.WebDriver
{
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

                        if (formatedLabel.Contains(room.Server) 
                            && formatedLabel.Contains(room.Fraction)
                            && room.IsOperationAllowed == true)
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
                var btn = driver.WaitElement(By.ClassName("progress_gr"));
                Thread.Sleep(1000);
                btn.Click();

                //Parse order info
                var orderServer = driver.WaitElement(By.XPath(BuildXpathString("Server"))).Text;
                var fraction = driver.WaitElement(By.XPath(BuildXpathString("Faction"))).Text;
                var buyer = driver.WaitElement(By.ClassName("seller__name")).Text;

                var OrderNumberEl = driver.WaitElement(By.CssSelector(".trade__order__top-num span"));
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
}
