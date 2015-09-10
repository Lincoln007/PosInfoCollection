using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FtpClient
{
    // https://github.com/muriarte/FtpUtil
    public class FtpFileInfo
    {
        private string[] fileLineArr;
        private string name;
        private long size;
        private bool folder;
        private int permission;
        private string owner;       // 所有者
        private string group;       // 用户组
        private bool parseDate = false;
        private DateTime modifyDate;

        public string Name
        {
            get { return name; }
        }
        public long Size
        {
            get { return size; }
        }
        public bool IsFolder
        {
            get { return folder; }
        }
        public string Owner
        {
            get { return owner; }
        }
        public string Group
        {
            get { return group; }
        }
        public int Permission
        {
            get
            {
                if (permission == 0)
                {
                    string ruleStr = fileLineArr[0].Substring(1);
                    if (ruleStr.Length == 9)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            int p = 0;
                            int rate = Convert.ToInt32(Math.Pow(10d, (2d - i)));
                            // r:4, w:2, x:1  http://www.codeceo.com/article/linux-chmod-command.html
                            p += ruleStr[i + 0] == '-' ? 0 : 4;
                            p += ruleStr[i + 1] == '-' ? 0 : 2;
                            p += ruleStr[i + 2] == '-' ? 0 : 1;
                            permission += p * rate;
                        }
                    }
                }
                return permission;
            }
        }
        public DateTime ModifyDate
        {
            get
            {
                if (!parseDate)
                {
                    string monthStr = fileLineArr[5].ToLower();
                    int day = Convert.ToInt32(fileLineArr[6]);
                    int year;
                    Regex regex = new Regex("^\\d{4}$");
                    Match match = regex.Match(fileLineArr[7]);
                    if (match.Success)
                    {
                        year = Convert.ToInt32(fileLineArr[7]);
                    }
                    else
                    {
                        year = DateTime.Now.Year;
                    }
                    int month = 1;
                    switch (monthStr)
                    {
                        case "jan":
                            month = 1;
                            break;
                        case "feb":
                            month = 2;
                            break;
                        case "mar":
                            month = 3;
                            break;
                        case "apr":
                            month = 4;
                            break;
                        case "may":
                            month = 5;
                            break;
                        case "jun":
                            month = 6;
                            break;
                        case "jul":
                            month = 7;
                            break;
                        case "aug":
                            month = 8;
                            break;
                        case "sep":
                            month = 9;
                            break;
                        case "oct":
                            month = 10;
                            break;
                        case "nov":
                            month = 11;
                            break;
                        case "dec":
                            month = 12;
                            break;
                    }
                    modifyDate = new DateTime(year, month, day);
                    parseDate = true;
                }
                return modifyDate;
            }
        }


        public FtpFileInfo(string lineStr)
        {
            fileLineArr = lineStr.Split(new char[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
            if (fileLineArr.Length == 9)
            {
                //isFolder = fileLineArr[0].StartsWith("d");
                folder = fileLineArr[1] == "4";
                owner = fileLineArr[2];
                group = fileLineArr[3];
                Int64.TryParse(fileLineArr[4], out size);
                name = fileLineArr[8];
            }
            else
            {
                folder = false;
            }
        }

    }

}
