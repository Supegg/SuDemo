using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuUtil;
using MySql.Data.MySqlClient;
using System.IO;
using System.Diagnostics;
using HashHelper;
using System.Xml;
using System.Xml.Linq;

namespace SuDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = "hehheh11111111111111111111111111111111111111111111111111111111111111111111";
            byte[] bs= Encoding.UTF8.GetBytes(s);

            //minWin("QXDM*");
            //testMysql();
            //ipChangeList();

            foreach(var f in Directory.GetFiles(@"E:\Flair\Lte"))
            {
                testMd5(f);
            }
            testMd5(@"C:\Users\Supegg\Desktop\AS\SAA5B27A22_生产环境_20160615.rar");
            
            //通过WMI获取COM端口
            string[] ss = PcInfo.MulGetHardwareInfo(HardwareEnum.Win32_PnPEntity, "Name");

            bs = SuEncipher.Encipher(bs);
            bs  = SuEncipher.Decipher(bs);
            string s1 = Encoding.UTF8.GetString(bs);

            bs=SuEncipher.Encipher(bs);
            bs=SuEncipher.Decipher(bs);
            string s2 = Encoding.UTF8.GetString(bs); 

        }

        private static void testMd5(string file)
        {
            string hash = Md5Helper.Md5Encrypt(file);
            Console.WriteLine("Hash:\t{0}", hash);
            XDocument doc = XDocument.Load("hash.xml");
            doc.Element("root").Attribute("hash").Value = hash;
            doc.Element("root").Value = file;
            doc.Save("hash.xml");
            
            doc = XDocument.Load("hash.xml");
            hash = doc.Element("root").Attribute("hash").Value;
            Console.WriteLine("Checked:\t{0}", Md5Helper.Md5Check(file, hash));
        }

        private static void ipChangeList()
        {
            Stopwatch watch = new Stopwatch();
            string ip = string.Empty;
            Dictionary<DateTime, string> changes = new Dictionary<DateTime, string>();
            int count = 0;
            string[] buf;

            watch.Start();
            foreach (var line in File.ReadAllLines("loongson_ip.csv"))
            {
                buf = line.Split(',');
                Console.WriteLine("{0}\t{1}", buf[0], buf[1]);
                if(ip!= buf[1])
                {
                    ip = buf[1];
                    changes.Add(DateTime.Parse(buf[0]), ip);
                }
                count++;
            }

            Console.WriteLine("Total:{0}, Changes:{1}", count, changes.Count);
            StringBuilder builder = new StringBuilder();
            DateTime last = DateTime.Now ;
            foreach (var c in changes)
            {
                builder.AppendLine(string.Format("{0}\t{1}\t{2}", c.Key, c.Value, c.Key-last));
                last = c.Key;
            }
            ConsoleWithColor(builder.ToString(), ConsoleColor.Magenta);
            Console.WriteLine("Total time: {0}s", watch.ElapsedMilliseconds / 1000);
        }

        private static void testMysql()
        {
            string connStr = "server=23.105.220.215;database=test;uid=root;pwd=mysql2016;";
            string ip = string.Empty;
            string when = string.Empty;
            Dictionary<string, string> changes = new Dictionary<string, string>();
            int count = 0;

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandTimeout = 100;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "select * from loongson_ip";
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    when = reader["when"].ToString();
                    if (ip != reader["ip"].ToString())
                    {
                        ip = reader["ip"].ToString();
                        changes.Add(when, ip);
                    }
                    Console.WriteLine("{0}\t{1}", when, ip);
                    count++;
                }
                reader.Dispose();
            }

            Console.WriteLine("Total:{0}, Changes:{1}", count, changes.Count);
            StringBuilder builder = new StringBuilder();
            foreach (var c in changes)
            {
                builder.AppendLine(string.Format("{0}\t{1}", c.Key, c.Value));
            }
            ConsoleWithColor(builder.ToString(), ConsoleColor.Magenta);
        }

        static void ConsoleWithColor(string message, ConsoleColor color= ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void minWin(string captain)
        {
            const int BM_CLICK = 0xF5;
            IntPtr maindHwnd = WinAPI.FindWindow(null, "c#颜色对照表"); //获得QQ登陆框的句柄  
            //IntPtr maindHwnd = WinAPI.FindWindow("Notepad", null); //获得QQ登陆框的句柄  
            if (maindHwnd != IntPtr.Zero)
            {
                IntPtr childHwnd = WinAPI.FindWindowEx(maindHwnd, IntPtr.Zero, null, "登录");   //获得按钮的句柄  
                if (childHwnd != IntPtr.Zero)
                {
                    WinAPI.SendMessage(childHwnd, BM_CLICK, 0, 0);     //发送点击按钮的消息  
                }
                else
                {
                    Console.WriteLine("没有找到子窗口");
                }
            }
            else
            {
                Console.WriteLine("没有找到窗口");
            } 
        }
    }
}
