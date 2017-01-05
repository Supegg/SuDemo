using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuUtil.ExMethod
{
    public static class ExByteArray
    {
        public static string ToString(this byte[] bs,string separator = "")
        {
            StringBuilder builder = new StringBuilder();
            foreach(var b in bs)
            {
                builder.Append(b.ToString("X2") + separator);
            }
            if (bs.Length > 0)
            {
                builder.Remove(builder.Length - separator.Length, separator.Length);
            }

            return builder.ToString();

            //string hex = BitConverter.ToString(bs);
            //return hex.Replace("-", "");
        }

        public static byte[] ToByteArray(string source,string separator = ".")
        {
            if (separator == string.Empty)
            {
                return Enumerable.Range(0, source.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(source.Substring(x, 2), 16))
                         .ToArray();
            }

            List<byte> bs = new List<byte>();
            string[] ss = source.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            foreach(var s in ss)
            {
                bs.Add(Convert.ToByte(s, 16));
            }

            return bs.ToArray();
        }
    }
}
