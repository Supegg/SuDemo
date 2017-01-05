using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PSkull
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("Process terminator. Hail Skull.");
            Console.Write("Pls confirm,y/n?");
            string cmd = Console.ReadLine();
            if(cmd.ToUpper().Trim()=="Y")
            {
                string[] names = System.Configuration.ConfigurationManager.AppSettings["ps"].Split('|');
                foreach(var name in names)
                {
                    Process[] ps = Process.GetProcessesByName(name);
                    foreach(var p in ps)
                    {
                        p.Kill();
                        Console.WriteLine("Skulled {0}.exe", name);
                    }
                }

                Console.Write("Skull all...");
                Console.ReadKey();
            }

            return;
        }
    }
}
