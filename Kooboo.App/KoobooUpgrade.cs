//Copyright (c) 2018 Yardi Technology Limited. Http://www.kooboo.com 
//All rights reserved.
using System;
using Kooboo.Lib.Helper;
using System.IO.Compression;
using Kooboo.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.IO;

namespace Kooboo.App
{
    public class KoobooUpgrade
    {
        static KoobooUpgrade()
        {      
            string path = System.IO.Path.Combine(Data.AppSettings.RootPath, "upgrade");  
            KoobooZipFullName = System.IO.Path.Combine(path, "Kooboo.zip");   
            UpgradeExeFullName = System.IO.Path.Combine(path, "Kooboo.Upgrade.exe"); 
            InitAutoUpgrade(); 
        }

        public static bool IsRunning { get; set; } = false;
        private static object _locker = new object();

        private static int LastCheckDay { get; set; }

        public static bool IsAutoUpgrade;     

        public static void SetAutoUpgrade(bool auto)
        {
            string filepath = System.IO.Path.Combine(Data.AppSettings.RootPath, "_admin", "AutoUpgrade.txt");
            Lib.Helper.IOHelper.EnsureFileDirectoryExists(filepath);   
            File.WriteAllText(filepath, auto.ToString());
            IsAutoUpgrade = auto; 
        }

        public static void InitAutoUpgrade()
        {
            // init auto upgrade.
            string filepath = System.IO.Path.Combine(Data.AppSettings.RootPath, "_admin", "AutoUpgrade.txt");

            if (File.Exists(filepath))
            {
                var result = File.ReadAllText(filepath);
                bool.TryParse(result, out IsAutoUpgrade);
            }
        }
                                              
        private static string KoobooZipFullName { get; set; }

        public static string UpgradeExeFullName { get; set; }

        public static async Task<Version> GetLatestVersion()
        {
            var version = new Version("0.0.0.0");
            try
            {
                string serverUrl = AppSettings.ConvertApiUrl + "/_api/converter/GetLatestVersion";
                var versionStr = await HttpHelper.GetAsync<string>(serverUrl);
                if (!string.IsNullOrWhiteSpace(versionStr))
                {
                    version = new Version(versionStr);
                }
            }
            catch (Exception ex)
            { 
            }
            return version;
        }

        private static async Task<byte[]> DownloadKoobooZip(System.Net.DownloadProgressChangedEventHandler downloadProgressChanged)
        {
            string serverUrl = AppSettings.ConvertApiUrl + "/_api/converter/DownloadUpgradePackage";
            var client = new System.Net.WebClient();
            if(downloadProgressChanged!=null)
                client.DownloadProgressChanged += downloadProgressChanged;

            Uri uri = new Uri(serverUrl);
            var bytes= await client.DownloadDataTaskAsync(uri);
            if (client.ResponseHeaders.AllKeys.Contains("filehash"))
            {
                var hash = client.ResponseHeaders["filehash"];
                if (hash != null)
                {
                    Guid hashguid = default(Guid);

                    if (Guid.TryParse(hash, out hashguid))
                    {
                        var newhash = Lib.Security.Hash.ComputeGuid(bytes);
                        if (hashguid == newhash)
                        {
                            return bytes;
                        }
                    }
                }
            }
            return null;
        }

        public static async Task Upgrade(System.Net.DownloadProgressChangedEventHandler downloadProgressChanged=null)
        {
            if (LastCheckDay == DateTime.Now.DayOfYear || IsRunning)
            {
                return;
            }

            bool hasupgrade = false; 

            Monitor.Enter(_locker);

            if (LastCheckDay == DateTime.Now.DayOfYear || IsRunning)
            {
                return;
            }

            if (!IsRunning)
            {
                IsRunning = true;
                LastCheckDay = DateTime.Now.DayOfYear; 

                try
                {
                    var newVersion = await GetLatestVersion();

                    if (newVersion > AppSettings.Version)
                    {  
                        var package = await DownloadKoobooZip(downloadProgressChanged);
                        if (package != null)
                        {
                            hasupgrade = true;

                            Lib.Helper.IOHelper.EnsureFileDirectoryExists(KoobooZipFullName); 

                            System.IO.File.WriteAllBytes(KoobooZipFullName, package);

                            string upgradeExeFileName = "Kooboo.Upgrade.exe"; 

                            byte[] upgradeexe = Kooboo.Data.Upgrade.UpgradeHelper.ExtractFileFromZip(package, upgradeExeFileName);
                           
                            if (upgradeexe == null)
                            {
                                var koobooexebytes = Data.Upgrade.UpgradeHelper.ExtractFileFromZip(package, "Kooboo.exe"); 
                                if (koobooexebytes !=null)
                                {
                                    upgradeexe = Data.Upgrade.UpgradeHelper.GetManifestResourceFile(koobooexebytes, upgradeExeFileName);    
                                }    
                            }

                            if (upgradeexe !=null)
                            {
                                hasupgrade = true;
                                System.IO.File.WriteAllBytes(UpgradeExeFullName, upgradeexe); 
                            }       
                        }

                    }

                }
                catch (Exception ex)
                {
                }
            }

            IsRunning = false;

            Monitor.Exit(_locker);

            if (hasupgrade)
            { 
                Process.Start(UpgradeExeFullName);
            }

        }

    }

}
