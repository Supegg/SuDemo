using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SuUtil
{
    public class WinAPI
    {
        public const int BM_CLICK = 0xF5;  //click message

        #region r/w ini
        /// <summary>
        /// 写ini文件
        /// </summary>
        /// <param name="section">INI文件中的段落</param>
        /// <param name="key">INI文件中的关键字</param>
        /// <param name="val">INI文件中关键字的数值</param>
        /// <param name="filePath">INI文件的完整的路径和名称</param>
        /// <returns>非零表示成功，零表示失败</returns>
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        /// <summary>
        /// 读ini文件
        /// </summary>
        /// <param name="section">INI文件中的段落名称</param>
        /// <param name="key">INI文件中的关键字</param>
        /// <param name="def">无法读取时候时候的缺省数值</param>
        /// <param name="retVal">返回值</param>
        /// <param name="size"></param>
        /// <param name="filePath">INI文件的完整路径和名称</param>
        /// <returns>非零表示成功，零表示失败</returns>
        [DllImport("kernel32")]
        public static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion

        [DllImport("User32.dll")]          
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

        [DllImport("User32.DLL")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
    }
}
