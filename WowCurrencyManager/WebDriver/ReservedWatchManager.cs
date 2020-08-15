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
        private List<FarmRoom> _activeRooms = new List<FarmRoom>();
        private List<RoomReserv> _reserves = new List<RoomReserv>();

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
                try
                {                    
                    _activeRooms = FarmRoomRouting.GetRoomRouting().Rooms;

                    if (_activeRooms.Count == 0) continue;

                    var worldParts = _activeRooms
                        .Select(_ => _.WordPart)
                        .Distinct();

                    foreach (var part in worldParts)
                    {
                        var activeRoomInPage = _activeRooms.Where(_ => _.WordPart == part);

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

                            if (product != null)
                            {
                                var cashRoomReserv = _reserves.FirstOrDefault(_ => _.Name == product.Title);

                                if (cashRoomReserv == null)
                                {
                                    _reserves.Add(new RoomReserv() { Name = product.Title, Reserve = product.Reserved });
                                }
                                else
                                {
                                    if (cashRoomReserv.Reserve > product.Reserved)
                                    {
                                        product.SetAmount(room.Balance);
                                        cashRoomReserv.Reserve = product.Reserved;
                                    }
                                }
                            }                                                     
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }                

                Thread.Sleep(10000);
            }
        }

    }

    public class RoomReserv
    {
        public string Name;
        public int Reserve;
    }
}
