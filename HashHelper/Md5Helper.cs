using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HashHelper
{
    public static class Md5Helper
    {
        private const string hashSalt = "Flairmicro237";

        public static string Md5Encrypt(string file)
        {
            byte[] salt = Encoding.ASCII.GetBytes(hashSalt);
            byte[] md5 = md5Encrypt(file);

            for(int i=0;i<md5.Length;i++)
            {
                md5[i] ^= salt[i % salt.Length];
            }

            return Convert.ToBase64String(md5);
        }

        public static bool Md5Check(string file, string hash)
        {
            return Md5Encrypt(file) == hash;
        }

        private static byte[] md5Encrypt(string file)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hash;
            using (BinaryReader br = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                hash = md5.ComputeHash(br.BaseStream);
            }
            return hash;
        }
    }
}
