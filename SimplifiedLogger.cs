using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FtpClient
{
    class SimplifiedLogger
    {
        // 日志保存文件的周期
        public enum LogPeriod
        {
            YEAR,
            MONTH,
            DAY
        }

        private enum LogType
        {
            ERROR,
            DEBUG,
            INFO
        }

        private static SimplifiedLogger obj;
        private DirectoryInfo logDir;
        private FileInfo logFile;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="period">保存周期</param>
        /// <param name="postfix">文件后缀</param>
        private SimplifiedLogger(LogPeriod period, string postfix)
        {
            // 当前程序目录
            string logPath = System.Environment.CurrentDirectory + "\\" + "logs";
            logDir = new DirectoryInfo(logPath);
            if (!logDir.Exists)
            {
                logDir.Create();
            }

            DateTime now = DateTime.Now;
            StringBuilder fileName = new StringBuilder();
            fileName.Append("log_").Append(now.Year);
            string monthStr = now.Month.ToString().PadLeft(2, '0');
            string dayStr = now.Day.ToString().PadLeft(2, '0');
            switch (period)
            {
                case LogPeriod.YEAR:
                    fileName.Append("0000");
                    break;
                case LogPeriod.MONTH:
                    fileName.Append(monthStr).Append("00");
                    break;
                case LogPeriod.DAY:
                    fileName.Append(monthStr).Append(dayStr);
                    break;
            }
            if (postfix != String.Empty)
            {
                fileName.Append("_").Append(postfix);
            }
            fileName.Append(".txt");
            logFile = new FileInfo(logPath + "\\" + fileName);
        }


        public static SimplifiedLogger Singleton(LogPeriod period, string postfix)
        {
            if (obj == null)
            {
                obj = new SimplifiedLogger(period, postfix);
            }
            return obj;
        }

        public static SimplifiedLogger Singleton(LogPeriod period)
        {
            return Singleton(period, String.Empty);
        }

        public static SimplifiedLogger Singleton(string postfix)
        {
            return Singleton(LogPeriod.DAY, postfix);
        }

        public static SimplifiedLogger Singleton()
        {
            return Singleton(LogPeriod.DAY, String.Empty);
        }


        private bool logAct(LogType type, string content)
        {
            bool result = false;
            StringBuilder text = new StringBuilder();
            string typeText = Enum.GetName(typeof(LogType), type);
            text.Append("-------- ").Append(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")).Append("  @ ").Append(typeText).AppendLine(" --------");
            text.AppendLine(content);

            try
            {
                using (FileStream fs = logFile.Open(FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.Default))
                    {
                        sw.WriteLine(text.ToString());
                        result = true;
                    }
                }
            }
            catch
            {
                // 忽略
            }
            return result;
        }

        public bool Debug(string content)
        {
            return logAct(LogType.DEBUG, content);
        }

        public bool Error(string content)
        {
            return logAct(LogType.ERROR, content);
        }

        public bool Info(string content)
        {
            return logAct(LogType.INFO, content);
        }

    }
}
