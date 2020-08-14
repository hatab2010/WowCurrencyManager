using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WowCurrencyManager.Room;
using WowCurrencyManager.WebElement;

namespace WowCurrencyManager.WebDriver
{
    public class ReservedWatchManager : ManagerBase
    {
        private static ReservedWatchManager _instance;
        private List<FarmRoom> _roomExceptions = new List<FarmRoom>();
        private List<FarmRoom> _activeRoom;

        ReservedWatchManager()
        {
            CreateDriver(Global.RESERVED_PROFILE);
            Task.Run(Process);
            WaitOrder.OrderFound += OnOrderFound;
            WaitOrder.OrderCompleted += OnOrderCompeted;
        }

        internal static ReservedWatchManager InitManager()
        {
            if (_instance == null)
            {
                return new ReservedWatchManager();
            }
            else
            {
                return _instance;
            }            
        }
        

        private void OnOrderFound(FarmRoom room)
        {
            _roomExceptions.Add(room);
        }

        private void OnOrderCompeted(FarmRoom room)
        {
            var removeRoom = _roomExceptions.FirstOrDefault(_ => _ == room);

            if (removeRoom != null)
            {
                _roomExceptions.Remove(removeRoom);
            }
        }

        public static void CreateProfile()
        {
            Global.Copy(Global.MANAGER_PROFILE, Global.RESERVED_PROFILE);
        }

        void Process()
        {
            while (true)
            {
                _activeRoom = FarmRoomRouting.GetRoomRouting().Rooms;

                if (_activeRoom.Count == 0) continue;

                var worldParts = _activeRoom
                    .Select(_ => _.WordPart)
                    .Distinct();

                foreach (var part in worldParts)
                {
                    var activeRoomInPage = _activeRoom.Where(_ => _.WordPart == part);

                    switch (part)
                    {
                        case "eu":
                            _driver.Navigate().GoToUrl(Global.INTERNAL_OREDER_EU);
                            break;

                        case "us":
                            _driver.Navigate().GoToUrl(Global.INTERNAL_ORDER_US);
                            break;
                    }


                    foreach (var room in activeRoomInPage)
                    {
                        var product = _driver.FindProductEl(room.Server, room.Fraction);

                        if (product == null)
                            continue;


                        if (_roomExceptions.Contains(room) == false)
                        {
                            if (product.Stock != (product.Reserved + room.Balance))
                            {
                                product.SetAmount(room.Balance);
                            }
                        }
                        else
                        {
                            
                        }
                    }

                }

                Thread.Sleep(15000);
            }
        }

    }
}
