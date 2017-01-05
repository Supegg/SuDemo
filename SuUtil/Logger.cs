using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SuUtil.ExMethod;

namespace SuUtil
{
    public abstract class Logger
    {
        private static string config = AppDomain.CurrentDomain.BaseDirectory + "Config\\Log.ini";

        public static void Bard(object message)
        {
            if (!message.IsNull())
            {
                bard(message.ToString());
            }else
            {
                bard("bard a null");
            }
        }
        private static void bard(string message)
        {
            StringBuilder val = new StringBuilder(500);
            long r = WinAPI.GetPrivateProfileString("config", "useLog", "1", val, val.Capacity, config);

            if (val.ToString() != "1")
            {
                return;
            }

            r = WinAPI.GetPrivateProfileString("config", "file", "", val, val.Capacity, config);
            string file = AppDomain.CurrentDomain.BaseDirectory + val.ToString();

            try
            {
                using (StreamWriter sw = new StreamWriter(file, true))
                {
                    sw.WriteLine(DateTime.Now + ":\t" + message);
                }
            }
            catch
            {
                return;
            }
        }

        public static void Daily(object message)
        {
            if (!message.IsNull())
            {
                daily(message.ToString());
            }
            else
            {
                daily(DateTime.Now + ":\t" + "daily a null");
            }
        }
        private static void daily(string message)
        {
            StringBuilder val = new StringBuilder(500);
            long r = WinAPI.GetPrivateProfileString("config", "useLog", "1", val, val.Capacity, config);

            if (val.ToString() != "1")
            {
                return;
            }

            string dir = AppDomain.CurrentDomain.BaseDirectory + "Log\\";
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string file = dir + DateTime.Now.ToString("yyyyMMdd") + ".daily";

            try
            {
                using (StreamWriter sw = new StreamWriter(file, true))
                {
                    sw.WriteLine(DateTime.Now + ":\t" + message);
                }
            }
            catch
            {
                return;
            }
        }
    }
}
