using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using OpenQA.Selenium;
using WowCurrencyManager.Room;

namespace WowCurrencyManager.Bot
{
    public class WaitOrderOperation : IOperation
    {
        public static event Action<FarmRoom> OrderFound;
        public static event Action<FarmRoom> OrderCompleted;
        public static event Action OrderException;

        private IWebDriver _driver;
        static Stopwatch _lastWatchTime = new Stopwatch();
        public FarmRoom Sender { private set; get; }
        private string _url = "https://www.g2g.com/order/sellOrder?status=5";

        public void Execute(IWebDriver driver)
        {
            AsyncStart(driver).Wait();
        }

        private async Task AsyncStart(IWebDriver driver)
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

            IWebElement currentOrder = null;
            FarmRoom currentRoom = null;

            try
            {
                //Open new order
                var newOrders = driver.FindElements(By.ClassName("sales-history__table-row-unread"));

                //Check for room exist
                var root = FarmRoomManager.GetRoomRouting();

                foreach (var el in newOrders)
                {
                    foreach (var room in root.Rooms)
                    {
                        var formatedLabel = Regex
                            .Replace(el.FindElement(By.ClassName("sales-history__product-name"))
                            .Text.ToLower(), "[’']", "");

                        if (formatedLabel.Contains(room.Server)
                            && formatedLabel.Contains(room.Fraction)
                            && room.IsOperationAllowed == true
                            && room.Balance != 0)
                        {
                            currentOrder = el;
                            currentRoom = room;

                            goto RoomExist;
                        };
                    }
                }

                return;

            RoomExist:

                OrderFound?.Invoke(currentRoom);
                var operationLink = currentOrder.FindElement(By.ClassName("sales-history__product-id"));
                operationLink.Click();

                //View order details
                var btn = driver.WaitElement(By.ClassName("progress_gr"));
                Thread.Sleep(1000);
                btn.Click();
                Thread.Sleep(1000);

                try
                {
                    var orderServer = driver.WaitElement(By.XPath(BuildXpathString("Server"))).Text;
                    driver.WaitElement(By.XPath("//a[contains(@class, 'progress_gr') and contains(text(), 'Start Trading')]"))
                       .Click();

                    //Parse order info

                    var fraction = driver.WaitElement(By.XPath(BuildXpathString("Faction"))).Text;
                    var buyer = driver.WaitElement(By.XPath(BuildXpathString("Character Name"))).Text;
                    var OrderNumberEl = driver.WaitElement(By.CssSelector(".trade__order__top-num"));
                    var amount = int.Parse(driver
                        .WaitElement(By.XPath("//td[@class = 'sales-history__table-quantity' and contains(@data-th, 'QTY.')]"))
                        .Text.Replace(",", ""));

                    var orderNumber = Regex.Match(OrderNumberEl.Text, @"SOLD ORDER №\d*").Value.Replace("SOLD ORDER", "");

                    string BuildXpathString(string fieldName)
                    {
                        return $"//span[contains(@class, 'game-info__title') and contains(text(), '{fieldName}')]/../span[contains(@class, 'game-info__info')]";
                    }


                    Thread.Sleep(1000);                    

                    //Create order in discord room
                    var titles = driver.WaitElement(By.ClassName("purchase-title")).Text.Split('>');
                    var myOrder = new Model.G2gOrder()
                    {
                        Buyer = buyer,
                        OrderId = orderNumber,
                        Amount = amount,
                        Server = $"{titles[1].FirstCharUp()} {titles[2].Replace("(GOLD)", "")}",
                        Fraction = fraction,
                        OrderPage = driver.Url,
                        RoomSender = currentRoom
                    };

                    currentRoom.AddOrder(myOrder);
                    currentRoom.SendOrderInChannle(myOrder);

                }
                catch (Exception ex)
                {
                    var admins = await GetAdmins();
                    if (admins != null)
                    {
                        foreach (var item in admins)
                        {
                            await item.SendMessageAsync($"Ошибка отправки ордера на :{currentRoom.Channel.Name}  (ex: {ex.Message})");
                        }
                    }

                    throw;
                }

            }
            catch (Exception ex)
            {
                var admins = await GetAdmins();
                if (admins != null)
                {
                    var servicesUser = admins.FirstOrDefault(_ => _.Username == "Hatab2010");

                    if (servicesUser != null)
                    {
                        await servicesUser.SendMessageAsync(ex.Message);
                    }
                }

                Console.WriteLine(ex);
            }

            async Task<IEnumerable<IUser>> GetAdmins()
            {
                if (currentRoom != null)
                {
                    var users = await currentRoom.Channel.GetUsersAsync().FlattenAsync();
                    return users.Where(_ => (((IGuildUser)_)
                       .GetPermissions((IGuildChannel)currentRoom.Channel))
                       .ToList().Contains(ChannelPermission.ManageChannels)
                       && _.IsBot == false);
                }
                else
                {
                    return null;
                }
            }
        }

        private void Update()
        {
            _lastWatchTime.Restart();
            _driver.Navigate().GoToUrl(_url);
        }
    }
}
