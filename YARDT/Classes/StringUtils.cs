using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace YARDT
{
    class StringUtils
    {
        public static string SanitizeString(string dirtyString)
        {
            return new string(dirtyString.Where(Char.IsLetterOrDigit).ToArray());
        }

        public static string CalculateMD5(string filename)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

    }
}
