using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using PosInfoCollection.Libs;

namespace PosInfoCollection.Entry
{
    public class ScanAction
    {
        private const string ConfigFileName = "config.xml";
        private static ScanAction obj;

        private long scanTimeNum;           // 扫描的次数
        private ConfigSettings config;
        private FtpLib ftpClient;
        private SimplifiedLogger logger;

        private ScanAction(DirectoryInfo appPath)
        {
            config = new ConfigSettings();

            FileInfo configFile = new FileInfo(appPath + "\\" + ConfigFileName);
            if (!configFile.Exists)
            {
                throw new Exception("配置文件不存在! " + configFile.FullName);
            }

            NameValueCollection appSettings = new NameValueCollection();
            XmlDocument dom = new XmlDocument();
            dom.Load(configFile.FullName);
            XmlNodeList appSettingList = dom.SelectNodes("//appSettings/add");
            foreach (XmlNode node in appSettingList)
            {
                appSettings.Add(node.Attributes["key"].Value, node.Attributes["value"].Value);
            }

            config.FtpHost = appSettings["FtpHost"];
            config.FtpUsername = appSettings["FtpUsername"];
            config.FtpPassword = appSettings["FtpPassword"];
            config.FtpUpload = appSettings["FtpUpload"];
            config.LocalPath = appSettings["LocalPath"];
            config.ArchiveDirectory = appSettings["ArchiveDirectory"];
            long periodInterval = 600;
            Int64.TryParse(appSettings["ScanPeriod"], out periodInterval);
            config.ScanPeriod = periodInterval;
            int lifeDay = 1;
            Int32.TryParse(appSettings["ArchiveLife"], out lifeDay);
            config.ArchiveLife = lifeDay;

            ftpClient = new FtpLib(config.FtpHost, config.FtpUsername, config.FtpPassword);

            DirectoryInfo logDir = new DirectoryInfo(appPath.FullName + "\\" + "logs");
            logger = SimplifiedLogger.Singleton(logDir);
        }

        public static ScanAction Singleton(DirectoryInfo appPath)
        {
            if (obj == null)
            {
                obj = new ScanAction(appPath);
            }
            return obj;
        }

        public static ScanAction Singleton()
        {
            string appPathStr = System.Environment.CurrentDirectory;
            return Singleton(new DirectoryInfo(appPathStr));
        }

        public ConfigSettings Config
        {
            get
            {
                return config;
            }
        }

        public SimplifiedLogger Logger
        {
            get
            {
                return logger;
            }
        }

        private bool scanLocalDir(string scanDir)
        {
            bool result = false;
            try
            {
                string[] driversStr = System.IO.Directory.GetLogicalDrives();
                foreach (string dStr in driversStr)
                {
                    DriveInfo drive = new DriveInfo(dStr);
                    if (drive.DriveType == DriveType.Fixed)
                    {
                        string scanPath = drive.Name + scanDir.TrimStart('\\');
                        DirectoryInfo dir = new DirectoryInfo(scanPath);
                        if (dir.Exists)
                        {
                            config.ScanLocation = dir;
                            // 配置归档目录配置
                            config.ScanArchive = new DirectoryInfo(dir.FullName + "\\" + config.ArchiveDirectory);
                            if (!config.ScanArchive.Exists)
                            {
                                config.ScanArchive.Create();
                            }
                            result = true;
                        }
                    }
                }
            }
            catch (IOException)
            {
                logger.Error(String.Format("扫描目录 {0} 错误", scanDir));
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return result;
        }


        public class ActionResult
        {
            public long ScanTime;
            public List<string> SuccessList = new List<string>();
            public List<string> FailureList = new List<string>();
        }

        /// <summary>
        /// 扫描 -> 上传 -> 归档
        /// </summary>
        /// <returns></returns>
        public ActionResult ActionDo()
        {
            ++scanTimeNum;
            ActionResult result = new ActionResult();
            result.ScanTime = scanTimeNum;
            if (config.ScanLocation != null || scanLocalDir(config.LocalPath))
            {
                try
                {
                    // 检查远程文件夹是否存在
                    if (ftpClient.FileExist(config.FtpUpload))
                    {
                        // 获取目录下的所有文件
                        FileInfo[] files = config.ScanLocation.GetFiles();
                        foreach (FileInfo file in files)
                        {
                            try
                            {
                                ftpClient.UploadFile(file, config.FtpUpload);
                                FileInfo distFile = new FileInfo(config.ScanArchive.FullName + "\\" + file.Name);
                                if (distFile.Exists)
                                {
                                    distFile.Delete();
                                }
                                file.MoveTo(distFile.FullName);
                                logger.Info(String.Format("{0}上传成功, 文件大小{1}.", file.Name, file.Length));
                                result.SuccessList.Add(file.FullName);
                            }
                            catch (WebException ex)
                            {
                                FtpWebResponse response = (FtpWebResponse)ex.Response;
                                logger.Error(String.Format("文件上传错误, {0}, {1} => {2}", response.StatusCode, file.FullName, config.FtpUpload));
                                result.FailureList.Add(file.FullName);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Scan Local Files : " + ex.ToString());
                                result.FailureList.Add(file.FullName);
                            }
                        }
                    }
                    else
                    {
                        logger.Error(String.Format("{0}远程目录不存在!", config.FtpUpload));
                    }
                }
                catch (WebException ex)
                {
                    FtpWebResponse response = (FtpWebResponse)ex.Response;
                    logger.Error(String.Format("Ftp检查远程目录错误, {0}.", response.StatusCode));
                }
                catch (Exception ex)
                {
                    logger.Error("Check Ftp Directory : " + ex.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// 清理归档文件夹
        /// </summary>
        public void ClearArchive()
        {
            DirectoryInfo archiveDir = config.ScanArchive;
            if (archiveDir != null)     // 可能尚未扫描过
            {
                FileInfo[] files = archiveDir.GetFiles();
                DateTime now = DateTime.Now;
                List<string> clearList = new List<string>();
                foreach (FileInfo file in files)
                {
                    try
                    {
                        TimeSpan diffTime = now - file.CreationTime;
                        if (diffTime.Days >= config.ArchiveLife)
                        {
                            file.Delete();
                            clearList.Add(file.FullName);
                        }
                    }
                    catch (IOException)
                    {
                        logger.Error("文件清理失败 : " + file.FullName);
                    }
                }
                if (clearList.Count > 0)
                {
                    logger.Info("清理文件 : " + String.Join(" ; ", clearList.ToArray()));
                }
            }
        }
    }
}
