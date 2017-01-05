using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuUtil.ExMethod;

namespace SuUtil
{
    /// <summary>
    /// 工具方法
    /// </summary>
    public class IniHelper
    {
        private static System.Threading.ReaderWriterLock rwLock = new System.Threading.ReaderWriterLock();

        public static void IniWrite(string iniPath, string section, string key, object val, bool isAsciiEncoded = false)
        {
            rwLock.AcquireWriterLock(1000);

            long rtn = WinAPI.WritePrivateProfileString(section, key, isAsciiEncoded ? ASCIIEncoding.ASCII.GetBytes(val.ToString()).ToString(".") : val.ToString(), iniPath);
            if (rtn == 0)
            {
                Logger.Bard(string.Format("IniWrite {0} {1} {2} {3}", iniPath, section, key, val));
            }
            rwLock.ReleaseLock();
        }

        public static string IniRead(string iniPath, string section, string key, bool isAsciiEncoded = false)
        {
            rwLock.AcquireReaderLock(1000);
            StringBuilder val = new StringBuilder(500);
            long rtn = WinAPI.GetPrivateProfileString(section, key, "", val, 500, iniPath);
            rwLock.ReleaseLock();
            if (rtn == 0)
            {
                Logger.Bard(string.Format("IniRead={0} {1} {2} {3} {4}", rtn, iniPath, section, key, val));
            }

            if (isAsciiEncoded)
            {
                return ASCIIEncoding.ASCII.GetString(ExByteArray.ToByteArray(val.ToString()));
            }
            else
            {
                return val.ToString();
            }
        }

        public static int IniReadInt(string iniPath, string section, string key)
        {
            string s = IniRead(iniPath, section, key);
            return int.Parse(s.ToString().Trim());
        }

        public static double IniReadDouble(string iniPath, string section, string key)
        {
            string s = IniRead(iniPath, section, key);
            return double.Parse(s.ToString().Trim());
        }

    }
}
