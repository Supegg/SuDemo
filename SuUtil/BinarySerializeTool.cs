using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.IO;

namespace SuUtil
{
    public abstract class BinarySerializeTool
    {
        /// <summary>
        /// Serialize a serializable object
        /// </summary>
        /// <param name="c"></param>
        /// <param name="path">a file who saves the object</param>
        public static void ToFile(object c,string path)
        {
            using (FileStream file = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter b = new BinaryFormatter();
                b.Serialize(file, c);
            }
        }

        /// <summary>
        /// Deserialize to a serializable Class
        /// </summary>
        /// <typeparam name="T">class type</typeparam>
        /// <param name="path">a file who saves the object</param>
        /// <returns></returns>
        public static T ToClass<T>(string path)
        {
            using (FileStream file = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter b = new BinaryFormatter();
                return (T)b.Deserialize(file);
            }
        }
    }
}
