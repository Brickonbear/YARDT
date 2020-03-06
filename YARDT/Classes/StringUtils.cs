using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace YARDT
{
    class StringUtils
    {
        /// <summary>
        /// Removes illigal characters from string
        /// </summary>
        /// <param name="dirtyString"></param>
        /// <returns></returns>
        public static string SanitizeString(string dirtyString)
        {
            return new string(dirtyString.Where(Char.IsLetterOrDigit).ToArray());
        }

        /// <summary>
        /// Calculate MD5 checksum of file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string CalculateMD5(string filename)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
        }

    }
}
