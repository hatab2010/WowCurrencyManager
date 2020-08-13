using System;
using System.IO;

namespace WowCurrencyManager.WebDriver
{
    public static class Global
    {
        public static string RESERVED_PROFILE = Directory.GetCurrentDirectory() + "/ReservWatchService";
        public static string MANAGER_PROFILE = Directory.GetCurrentDirectory() + "/Data";
        public static string INTERNAL_OREDER_EU = "https://www.g2g.com/sell/manage?service=1&game=27815";
        public static string INTERNAL_ORDER_US = "https://www.g2g.com/sell/manage?service=1&game=27816";

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

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
