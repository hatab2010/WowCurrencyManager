using System;
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

namespace WowCurrencyManager.Bot
{
    public class WebManager : ManagerBase
    {
        public static Action<ProfileStatus> Logged;
        private static WebManager _instance;        

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
                    OrderWatchManager.CreateProfile();
                    OrderWatchManager.InitManager();
                    CreateDriver(Global.MANAGER_PROFILE);
                    break;
                case ProfileStatus.Old:
                    OrderWatchManager.InitManager();
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
}
