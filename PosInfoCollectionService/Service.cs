using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.IO;
using Microsoft.Win32;
using PosInfoCollection.Libs;

namespace PosInfoCollection.Entry
{
    public partial class Service : ServiceBase
    {
        private Timer actionTimer;
        private Timer clearTimer;
        private ScanAction action;

        /// <summary>
        /// 获取服务安装路径
        /// http://blog.csdn.net/hnfeitianwugui/article/details/7622292
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private DirectoryInfo GetCurrentServiceInstallPath()
        {
            string key = @"SYSTEM\CurrentControlSet\Services\" + this.ServiceName;
            string path = Registry.LocalMachine.OpenSubKey(key).GetValue("ImagePath").ToString();
            //替换掉双引号   
            path = path.Replace("\"", string.Empty);
            FileInfo fi = new FileInfo(path);
            return fi.Directory;
        }

        public Service()
        {
            InitializeComponent();
            DirectoryInfo imagePath = GetCurrentServiceInstallPath();
            actionTimer = new Timer();
            clearTimer = new Timer();
            try
            {
                action = ScanAction.Singleton(imagePath);
#if DEBUG
                action.Logger.Debug("InitializeComponent " + imagePath.FullName);
#endif

                actionTimer.AutoReset = true;
                actionTimer.Enabled = false;
                actionTimer.Interval = action.Config.ScanPeriod * 1000;
                actionTimer.Elapsed += ActionTimerElapsed;

                clearTimer.AutoReset = true;
                clearTimer.Enabled = false;
                clearTimer.Interval = 86400000;
                clearTimer.Elapsed += ClearTimerElapsed;

#if DEBUG
                action.Logger.Debug("扫描间隔 " + actionTimer.Interval.ToString());
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                action.Logger.Debug("Init错误 " + ex.ToString());
#endif
            }
        }

        private void ActionTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                action.ActionDo();
#if DEBUG
                action.Logger.Debug("Action ~~");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                action.Logger.Debug("Action " + ex.ToString());
#endif
            }
        }

        private void ClearTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                action.ClearArchive();
#if DEBUG
                action.Logger.Debug("Clear ~~");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                action.Logger.Debug("Clear " + ex.ToString());
#endif
            }
        }

        protected override void OnStart(string[] args)
        {
            actionTimer.Start();
            clearTimer.Start();
#if DEBUG
            action.Logger.Debug("Start ...");
#endif
        }

        protected override void OnStop()
        {
            actionTimer.Stop();
            actionTimer.Start();
#if DEBUG
            action.Logger.Debug("Stop ...");
#endif
        }


    }
}
