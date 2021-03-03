using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBotCore.Data;
using WebBotCore.Operation;
using WowCurrencyManager.v2.Operation;

namespace WowCurrencyManager.v2.Model
{
    public static class Cash
    {

    }
    
    public static class G2gCore
    {
        private static readonly List<WorkerRole> RolesList = new List<WorkerRole>()
        { 
            WorkerRole.Edit, WorkerRole.Video, WorkerRole.Watch
        };

        private static List<G2gWorker> Workers = new List<G2gWorker>();        

        public static void Init()
        {
            var siteProfile = new SiteProfile()
            {
                Page = new Uri("https://www.g2g.com/")
            };

            var chromeProfile = new ChromeProfile("main");
            var initWorker = new G2gWorker(chromeProfile);           
            var checkLogin = new CheckAutorizationOperation(siteProfile);
            var authorization = new AutorizationOperation();
            initWorker.OperationExecuted += onOperacionExecuted;

            initWorker.Start();
            initWorker.AddOperation(checkLogin);

            void onOperacionExecuted(OperationResult operationResult)
            {
                if (operationResult.CurrentOpeartion 
                    is CheckAutorizationOperation)
                {
                    var isSuccessAutorization = (bool)operationResult.Value;

                    if (isSuccessAutorization == false)
                    {
                        initWorker.AddOperation(authorization);
                    }
                    else
                    {
                        initWorkers(false);
                    }
                }

                if (operationResult.CurrentOpeartion 
                    is AutorizationOperation)
                {
                    initWorkers(true);
                }
            }


            void initWorkers(bool isCopyProfile)
            {
                initWorker.OperationExecuted -= onOperacionExecuted;
                //TODO рекурсия
                initWorker.StopAsync().Wait();

                for (int i = 0; i < RolesList.Count; i++)
                {
                    var newCromeId = $"{chromeProfile.Id}__{i}";
                    var newProfile = isCopyProfile ? 
                        chromeProfile.CopyProfileDirectory(newCromeId) : new ChromeProfile(newCromeId);
                    var newWorker = new G2gWorker(newProfile) { Role = RolesList[i] };

                    newWorker.Start();
                    Workers.Add(newWorker);
                }
            }
        }
    }
}
