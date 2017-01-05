using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SuUtil
{
    public abstract class ZipHelper
    {
        // Fields  
        private static string _7zInstallPath = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("7-Zip").GetValue("Path").ToString() + "7z.exe";// @"C:\Program Files\7-Zip\7z.exe";
        private static StringBuilder temp = new StringBuilder();

        /// <summary>
        /// 查找压缩包中是否有name的文件
        /// 如有，返回完整的 Name 属性
        /// 如否，返回 Null
        /// </summary>
        /// <param name="zipFile">压缩包文件</param>
        /// <param name="name">查找的文件名</param>
        /// <returns></returns>
        public static string GetFullName(string zipFile, string name)
        {
            temp.Clear();

            Process process = new Process();
            process.StartInfo.FileName = _7zInstallPath;
            process.StartInfo.Arguments = string.Format(" l \"{0}\"", zipFile);//??" l \"{0}\" >'7z.lst'"
            //隐藏DOS窗口  
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            
            process.StartInfo.UseShellExecute = false;
            //process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Close();

            //Console.WriteLine(temp);

            MatchCollection mc = Regex.Matches(temp.ToString(), @"(?<DateTime>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2})\s(?<Attr>.{5})\s(?<Size>.{12})\s(?<Compressed>.{12})\s{2}(?<Name>.+)\s$", RegexOptions.Multiline);
            foreach (Match m in mc)
            {
                if (m.Success && m.Groups["Name"].Value.Contains(name))
                {
                    return m.Groups["Name"].Value;
                }
            }
            return null;
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            //Console.WriteLine(e.Data);
            temp.AppendLine(e.Data);
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            //Console.WriteLine(e.Data);
            temp.AppendLine(e.Data);
        }

        /// <summary>  
        /// 压缩目录  
        /// </summary>  
        /// <param name="strInDirectoryPath">指定需要压缩的目录，如C:\test\，将压缩test目录下的所有文件</param>  
        /// <param name="strOutFilePath">压缩后压缩文件的存放目录</param>  
        public static void CompressDirectory(string strInDirectoryPath, string strOutFilePath)
        {
            Process process = new Process();
            process.StartInfo.FileName = _7zInstallPath;
            process.StartInfo.Arguments = string.Format(" u -tzip \"{0}\" \"{1}\" -r", strOutFilePath, strInDirectoryPath);
            //隐藏DOS窗口  
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
            process.Close();
        }

        /// <summary>  
        /// 压缩文件  
        /// </summary>  
        /// <param name="strInFilePath">指定需要压缩的文件，如C:\test\demo.xlsx，将压缩demo.xlsx文件</param>  
        /// <param name="strOutFilePath">压缩后压缩文件的存放目录</param>  
        public static void CompressFile(string strInFilePath, string strOutFilePath)
        {
            Process process = new Process();
            process.StartInfo.FileName = _7zInstallPath;
            process.StartInfo.Arguments = string.Format(" u -tzip \"{0}\" \"{1}\" ", strOutFilePath, strInFilePath);
            //隐藏DOS窗口  
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
            process.Close();
        }

        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="strInFilePath">压缩文件的路径</param>
        /// <param name="strOutDirectoryPath">解压缩后文件的目录</param>
        public static void DecompressToDestDirectory(string strInFilePath, string strOutDirectoryPath)
        {
            Process process = new Process();
            process.StartInfo.FileName = _7zInstallPath;
            process.StartInfo.Arguments = string.Format(" x \"{0}\" -o\"{1}\" -r -aoa", strInFilePath, strOutDirectoryPath);
            //隐藏DOS窗口
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
            process.Close();
        }

        /// <summary>  
        /// 解压指定文件到指定目录
        /// </summary>  
        /// <param name="strInFilePath">压缩文件的路径</param>  
        /// <param name="name">压缩文件中的Name</param>  
        /// <param name="strOutDirectoryPath">解压缩后文件的目录</param>  
        public static void DecompressFileToDestDirectory(string strInFilePath, string name, string strOutDirectoryPath)
        {
            Process process = new Process();
            process.StartInfo.FileName = _7zInstallPath;
            process.StartInfo.Arguments = string.Format(" e \"{0}\" \"{1}\" -o\"{2}\" -aoa", strInFilePath, name, strOutDirectoryPath);
            //隐藏DOS窗口  
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
            process.Close();
        }

    }
}
