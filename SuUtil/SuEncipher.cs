using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SuUtil
{
    public class SuEncipher
    {
        private static byte[] key = new byte[] { 86, 97, 108, 97, 114, 32, 77, 111, 114, 103, 104, 117, 108, 105, 115, 44, 118, 97, 108, 97, 114, 32, 100, 111, 104, 97, 101, 114, 105, 115, 46 };

        public static byte[] Encipher(byte[] raw)
        {
            if (raw == null || raw.Length == 0)
            {
                throw new ArgumentException("Encrypte failed. Data is broken.");
            }

            byte[] salt = new byte[16 + raw.Length];

            salt[0] = 0x03;
            salt[1] = 0x06;
            Random r = new Random();
            for (int i = 2; i < 16; i++)
            {
                salt[i] = (byte)(r.Next() % key[i]);
            }
            Array.Copy(raw, 0, salt, 16, raw.Length);

            xor(salt);

            byte[] saltHash = null;
            using(MD5 md5 = MD5.Create())
            {
                saltHash = md5.ComputeHash(salt);
            }
            xor(saltHash);

            byte[] garbled = new byte[16 + salt.Length];

            Array.Copy(saltHash, garbled, saltHash.Length);
            Array.Copy(salt, 0, garbled, 16, salt.Length);

            return garbled;
        }

        public static byte[] Decipher(byte[] garbled)
        {
            if (garbled == null || garbled.Length <= 32)
            {
                throw new ArgumentException("Decrypte failed. Data is broken.");
            }

            byte[] hash = new byte[16];
            byte[] salt = new byte[garbled.Length - 16];
            Array.Copy(garbled, hash, hash.Length);
            Array.Copy(garbled, 16, salt, 0, salt.Length);
            xor(hash);

            byte[] saltHash = null;
            using (MD5 md5 = MD5.Create())
            {
                saltHash = md5.ComputeHash(salt);
            }

            for(int i =0;i<hash.Length;i++)
            {
                if(hash[i]!=saltHash[i])
                {
                    throw new Exception("Decrypte failed.Data is modified.");
                }
            }

            xor(salt);
            byte[] raw = new byte[salt.Length - 16];
            Array.Copy(salt, 16, raw, 0, raw.Length);
            return raw;
        }

        private static void xor(byte[] raw)
        {
            int index = 0;
            do
            {
                raw[index] = (byte)(raw[index] ^ key[index % key.Length]);
                if (++index == raw.Length)
                {
                    break;
                }
            } while (true);
        }
    }
}
