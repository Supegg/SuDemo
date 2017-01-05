using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SuUtil
{
    public class DBConfig
    {
        private static readonly string config = AppDomain.CurrentDomain.BaseDirectory + "Config\\DBConfig.xml";

        public static string GetConnStr(string nodeName)
        {
            string name = "";
            string nodeValue = "";
            
            XmlDocument doc = new XmlDocument();
            doc.Load(config);
            XmlNodeReader reader = new XmlNodeReader(doc);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        name = reader.Name;
                        break;
                    case XmlNodeType.Text:
                        if (name.Equals(nodeName))
                        {
                            nodeValue = reader.Value;
                        }
                        break;
                }
            }

            return nodeValue;
        }
    }
}
