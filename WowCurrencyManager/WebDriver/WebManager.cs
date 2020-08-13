using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WowCurrencyManager.Modules;
using WowCurrencyManager.Room;
using WowCurrencyManager.WebElement;

namespace WowCurrencyManager.WebDriver
{
    enum OperationPage
    {
        MyOrders,
        Currency,
        ExternalOrder,
        OrderProcess,
        SendMessage
    }

    public interface IBalance
    {
        int Balance { get; }
    }

    public class ManagerBase
    {
        protected IWebDriver _driver;

        protected void CreateDriver(string dirPath)
        {
            var options = new ChromeOptions();

            options.AddArgument($"--user-data-dir={dirPath}");
            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = new TimeSpan(15000);
        }

    }

    public class WotchReserved
    {
        enum WordPartType
        {
            EU, US
        }

        private List<FarmRoom> activeRooms = new List<FarmRoom>();

        public void Start(IWebDriver driver)
        {
            activeRooms = FarmRoomRouting.GetRoomRouting().Rooms;

            string[] watchetOrdersPages = { "https://www.g2g.com/sell/manage?service=1&game=27815",
                "https://www.g2g.com/sell/manage?service=1&game=27816" };

            foreach (var page in watchetOrdersPages)
            {
                //activeRooms[0].WordPart;
                //driver.Navigate().GoToUrl(page);                

                foreach (var activeRoom in activeRooms)
                {
                    
                }
            }                       
        }
    }

    public class WebManager : ManagerBase
    {
        public static Action<ProfileStatus> Logged;
        private static WebManager _instance;
        private List<IOperation> _opertions = new List<IOperation>();

        public WebManager()
        {
            InitDriver();
            FarmRoom.Changed += OnOrderChanged;
            Commands.Stoped += OnStoped;

            Task.Run(Process);
        }

        void OnStoped()
        {
            _driver?.Dispose();
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

        private void OnOrderChanged(FarmRoom room)
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
            if (_instance == null)
            {
                return new WebManager();
            }
            else
            {
                return _instance;
            }
        }

        private void InitDriver()
        {
            CreateDriver(Global.MANAGER_PROFILE);
            _driver.Navigate().GoToUrl("https://www.g2g.com/order/sellOrder?status=5");
            var profileStatus = Authorization();

            switch (profileStatus)
            {
                case ProfileStatus.New:
                    _driver.WaitElement(By.ClassName("sales-history__title-text"));
                    var js = (IJavaScriptExecutor)_driver;
                    Thread.Sleep(15000);

                    while ((bool)js.ExecuteScript("return jQuery.active == 0") == false)
                    {
                        Thread.Sleep(300);
                    }

                    _driver.Quit();
                    ReservedWatchManager.CreateProfile();
                    ReservedWatchManager.InitManager();
                    CreateDriver(Global.MANAGER_PROFILE);
                    break;
                case ProfileStatus.Old:
                    ReservedWatchManager.InitManager();
                    break;
            }            
        }

        private ProfileStatus Authorization()
        {
            var profileStatus = ProfileStatus.Old;

            while (true)
            {
                if (!_driver.Url.Contains("login")
                    && !_driver.Url.Contains("sso/device")
                    && !_driver.Url.Contains("/captcha"))
                {
                    Logged?.Invoke(profileStatus);
                    break;
                }
                else
                {
                    profileStatus = ProfileStatus.New;
                }
            }

            return profileStatus;
        }
    }

    public enum ProfileStatus
    {
        New, Old
    }

    public class WebBot : ChromeDriver
    {
        public WebBot()
        {
            
        }
    }
}
