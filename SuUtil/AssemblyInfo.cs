using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuUtil
{
    public class AssemblyInfo
    {
        
        public const string Version = "V1.0.5";
        public readonly static DateTime ReleaseDate = new DateTime(2015, 8, 19);
        public const string Author = "Supegg.Rao";
        public static string AppPath = null;//init when app starts

        //for wpf app, Propertis.AssemblyInfo.cs
        //public readonly DateTime ReleaseDate = new DateTime(2016, 4, 19);//origin date
        //ReleaseDate = new DateTime(2016, 5, 9);//update date
        //Application.ResourceAssembly.GetName().Version = new Version(1, 0, 1);
        //this.Title += "_V" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
    }
}
