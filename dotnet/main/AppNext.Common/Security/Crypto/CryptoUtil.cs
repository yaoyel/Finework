using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AppBoot.Security.Crypto
{
    public static class CryptoUtil
    {

        private const string Chars = "0123456789abcdefghijklmnopqrstuvwxyz";

        #region EncryptText/DecriptText


        public static byte[] Encrypt(String data, ICryptoTransform transform)
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                {
                    byte[] toEncrypt = new UnicodeEncoding().GetBytes(data);
                    cs.Write(toEncrypt, 0, toEncrypt.Length);
                    cs.FlushFinalBlock();
                }

                return ms.ToArray();

            }
        }

        public static byte[] Encrypt(Stream stream, ICryptoTransform transform)
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                {
                    byte[] toEncrypt = new byte[stream.Length];
                    stream.Read(toEncrypt, 0, toEncrypt.Length);
                    stream.Seek(0, SeekOrigin.Begin);

                    cs.Write(toEncrypt, 0, toEncrypt.Length);
                    cs.FlushFinalBlock();
                }

                return ms.ToArray();

            }
        }



        public static String Decrypt(byte[] data, ICryptoTransform transform)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Read))
                {
                    var sr = new StreamReader(cs, new UnicodeEncoding());
                    return sr.ReadLine();
                }
            }
        }

        #endregion

        public static Byte[] CreateRandomBytes(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException("length", "The length must be greater than 0.");

            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[length];
            rng.GetBytes(buff);

            return buff;
        }

        #region CreateRandomText

        public static String CreateRandomText(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException("length", "The length must be greater than 0.");

            var buff = CreateRandomBytes(length);
            return Convert.ToBase64String(buff);
        }


        public static List<string> CreateRandomText(int len, int count)
        {
           return GetRandString(len, count);
        }


        #endregion

        #region CalculateHash

        /// <summary> 计算文本的 hash 值. </summary>
        public static String CalculateHash(this HashAlgorithm hashAlgorithm, String text)
        {
            if (hashAlgorithm == null) throw new ArgumentNullException("hashAlgorithm");
            Byte[] bytes = Encoding.UTF8.GetBytes(text);
            Byte[] hashedBytes = hashAlgorithm.ComputeHash(bytes);
            String hash = Convert.ToBase64String(hashedBytes);
            return hash;
        }

        public static String CalculateHash(this HashAlgorithm hashAlgorithm, Stream stream)
        {
            if (hashAlgorithm == null) throw new ArgumentNullException("hashAlgorithm");

            Byte[] hashedBytes = hashAlgorithm.ComputeHash(stream);
            String hash = Convert.ToBase64String(hashedBytes);
            return hash;
        }

        #endregion

        private static List<string> SortByRandom(List<string> charList)
        {
            Random rand = new Random();
            for (int i = 0; i < charList.Count; i++)
            {
                int index = rand.Next(0, charList.Count);
                string temp = charList[i];
                charList[i] = charList[index];
                charList[index] = temp;
            }

            return charList;
        }

        private static List<string> GetRandString(int len, int count)
        {
            double max_value = Math.Pow(36, len);
            if (max_value > long.MaxValue)
            {
                return null;
            }

            long all_count = (long) max_value;
            long stepLong = all_count/count;
            if (stepLong > int.MaxValue)
            {
                stepLong = int.MaxValue;
            }
            int step = (int) stepLong;
            if (step < 3)
            {
                return null;
            }
            long begin = 0;
            List<string> list = new List<string>();
            Random rand = new Random();
            while (true)
            {
                long value = rand.Next(1, step) + begin;
                begin += step;
                list.Add(GetChart(len, value));
                if (list.Count == count)
                {
                    break;
                }
            }

            list = SortByRandom(list);

            return list;
        }

        private static string GetChart(int len, long value)
        {
            StringBuilder str = new StringBuilder();
            while (true)
            {
                str.Append(Chars[(int)(value % 36)]);
                value = value / 36;
                if (str.Length == len)
                {
                    break;
                }
            }

            return str.ToString();
        }

    }
}