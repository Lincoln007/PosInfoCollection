using System;
using System.Text;
using System.IO;

namespace PosInfoCollection.Libs
{
    public class ConfigSettings
    {
        public string FtpHost { set; get; }
        public string FtpUsername { set; get; }
        public string FtpPassword { set; get; }
        public string FtpUpload { set; get; }
        public string LocalPath { set; get; }
        public long ScanPeriod { set; get; }
        public string ArchiveDirectory { set; get; }
        public int ArchiveLife { set; get; }

        /// <summary>
        /// 扫描的目录
        /// </summary>
        public DirectoryInfo ScanLocation { set; get; }
        /// <summary>
        /// 扫描后归档的目录
        /// </summary>
        public DirectoryInfo ScanArchive { set; get; }

    }
}
