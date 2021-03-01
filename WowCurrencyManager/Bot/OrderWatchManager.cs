using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WowCurrencyManager.Room;
using WowCurrencyManager.WebElement;

namespace WowCurrencyManager.Bot
{
    public class OrderWatchManager : ManagerBase
    {
        private static OrderWatchManager _instance;

        OrderWatchManager()
        {
            CreateDriver(Global.RESERVED_PROFILE);
            Task.Run(Process);
        }

        internal static OrderWatchManager InitManager()
        {
            if (_instance == null)
            {
                var manager = new OrderWatchManager();
                _instance = manager;
                return manager;
            }
            else
            {
                return _instance;
            }            
        }       

        public static void CreateProfile()
        {
            Global.Copy(Global.MANAGER_PROFILE, Global.RESERVED_PROFILE);
        }

        protected override void EmptyOperationsList()
        {
            lock (_opertions)
            {
                _opertions.Add(new WaitOrderOperation());
            }
        }

        public static void AddOperation(IOperation operation)
        {
            lock (_instance._opertions)
            {
                _instance._opertions.Add(operation);
            }
        }
    }

    public class RoomReserv
    {
        public string Name;
        public int Reserve;
    }
}
