using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using System.Threading;
using WowCurrencyManager.ExceptionModule;

namespace WowCurrencyManager.Bot
{
    public class ManagerBase
    {
        protected IWebDriver _driver;
        protected List<IOperation> _opertions = new List<IOperation>();

        private string _dirPath;

        Stopwatch timer = new Stopwatch();

        protected void CreateDriver(string dirPath)
        {
            _dirPath = dirPath;
            var options = new ChromeOptions();
            var services = ChromeDriverService.CreateDefaultService();

            options.AddArgument($"--user-data-dir={dirPath}");
            _driver = new ChromeDriver(services, options, TimeSpan.FromSeconds(600));
            _driver.Manage().Timeouts().ImplicitWait = new TimeSpan(15000); //TODO нужно проверить таймы на разрыв соединения
        }

        protected void Process()
        {
            while (true)
            {
                try
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
                            var isFinished = false;

                            do
                            {
                                try
                                {
                                    curOperation.Execute(_driver);
                                    isFinished = true;
                                }
                                catch (ExceptionBase ex)
                                {
                                     
                                    if (ex.Range == ExceptionType.Constant)
                                    {
                                        isFinished = true;
                                    }
                                    else
                                    {
                                        Task.Delay(15000).Wait();
                                        RestartValidation();
                                    }
                                }

                            } while (!isFinished);


                            lock (_opertions)
                            {
                                _opertions.Remove(curOperation);
                            }
                        }
                    }
                    else
                    {
                        EmptyOperationsList();

                        if (_opertions.Count == 0)
                        {
                            Thread.Sleep(3000);
                        }                                            
                    }

                    if (timer.IsRunning)
                    {
                        timer.Stop();
                        timer.Reset();
                    }
                }
                catch (ExceptionBase ex)
                {
                    RestartValidation();
                }
            }
        }

        void RestartValidation()
        {
            if (!timer.IsRunning)
            {
                timer.Start();
            }
            else if (timer.ElapsedMilliseconds > 60000)
            {
                timer.Stop();
                timer.Reset();

                RestartDriver();
            }
        }

        protected virtual void EmptyOperationsList()
        {
        }

        protected virtual void RestartDriver()
        {
            _driver?.Quit();
            CreateDriver(_dirPath);
        }
    }
}
