using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBotCore.Model;

namespace WowCurrencyManager.v2.Model
{
    public static class Core
    {
        private static List<Worker> Workers;
        private static string profileFolderPerfix => "Profile__";
        private const int mainProfile = 0;

        public static void CreateWorkers(int workersCount)
        {
            Workers = new List<Worker>();

            for (int i = 0; i < workersCount; i++)
            {
                switch (i)
                {
                    case 0:
                        
                        break;
                }
            }
        }
    }
}
