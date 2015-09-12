using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using PosInfoCollection;
using System.Diagnostics;
using PosInfoCollection.Entry;
using PosInfoCollection.Libs;

namespace PosInfoCollection.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ScanAction action = ScanAction.Singleton();

            new Timer(ActionCallback, action, 3000, 15000);
            new Timer(ClearCallBack, action, 5000, 15000);
            //Console.WriteLine(System.AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE"));

            while (true)
            {
                string input = Console.ReadLine().ToLower();
                if (input == "quit" || input == "exit")
                {
                    break;
                }
            }
        }


        static void ActionCallback(object sender)
        {
            ScanAction action = (ScanAction)sender;
            ScanAction.ActionResult result = action.ActionDo();
            Console.WriteLine("扫描次数: {0}", result.ScanTime);
            foreach (string file in result.SuccessList)
            {
                Console.WriteLine("   ==> 成功文件: {0}", file);
            }
            foreach (string file in result.FailureList)
            {
                Console.WriteLine("   ==> 失败文件: {0}", file);
            }
        }

        static void ClearCallBack(object sender)
        {
            ScanAction action = (ScanAction)sender;
            action.ClearArchive();
            Console.WriteLine("清理文件");
        }

    }
}
