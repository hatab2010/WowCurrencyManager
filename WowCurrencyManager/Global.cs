using System;
using System.Collections.Generic;
using System.IO;
using WowCurrencyManager.Bot.Pages;

namespace WowCurrencyManager.Bot
{
    public static class Global
    {
        public static string RESERVED_PROFILE = Directory.GetCurrentDirectory() + "/ReservWatchService";
        public static string MANAGER_PROFILE = Directory.GetCurrentDirectory() + "/Data";
        public static string INTERNAL_OREDER_CLASSIC_EU = "https://www.g2g.com/sell/manage?service=1&game=27815";
        public static string INTERNAL_ORDER_CLASSIC_US = "https://www.g2g.com/sell/manage?service=1&game=27816";
        public static string INTERNAL_OREDER_EU = "https://www.g2g.com/sell/manage?service=1&game=2522";
        public static string INTERNAL_ORDER_US = "https://www.g2g.com/sell/manage?service=1&game=2299";

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static List<Page> DefaultSitePage = new List<Page>()
        {
            new Page()
            {
                operationType = OperationType.LookOrders,
                ServerWordPart = WorldPart.EU,
                Url = "https://www.g2g.com/wow-classic-eu/gold-27815-27817",
                WowServerType = GameVersion.Classic
            },
            new Page()
            {
                operationType = OperationType.LookOrders,
                ServerWordPart = WorldPart.US,
                Url = "https://www.g2g.com/wow-classic-us/gold-27816-27825",
                WowServerType = GameVersion.Classic
            },
            new Page()
            {
                operationType = OperationType.LookOrders,
                ServerWordPart = WorldPart.EU,
                Url = "https://www.g2g.com/wow-eu/gold-2522-19248",
                WowServerType = GameVersion.Main
            },
            new Page()
            {
                operationType = OperationType.LookOrders,
                ServerWordPart = WorldPart.US,
                Url = "https://www.g2g.com/wow-us/gold-2299-19249",
                WowServerType = GameVersion.Main
            },
            new Page()
            {
                operationType = OperationType.EditOrder,
                ServerWordPart = WorldPart.EU,
                Url = "https://www.g2g.com/sell/manage?service=1&game=27815",
                WowServerType = GameVersion.Classic
            },
            new Page()
            {
                operationType = OperationType.EditOrder,
                ServerWordPart = WorldPart.US,
                Url = "https://www.g2g.com/sell/manage?service=1&game=27816",
                WowServerType = GameVersion.Classic
            },
            new Page()
            {
                operationType = OperationType.EditOrder,
                ServerWordPart = WorldPart.EU,
                Url = "https://www.g2g.com/sell/manage?service=1&game=2522",
                WowServerType = GameVersion.Main
            },
            new Page()
            {
                operationType = OperationType.EditOrder,
                ServerWordPart = WorldPart.US,
                Url = "https://www.g2g.com/sell/manage?service=1&game=2299",
                WowServerType = GameVersion.Main
            },

        };

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}
