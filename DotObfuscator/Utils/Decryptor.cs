using System;
using System.Text;

namespace DotObfuscator_Framework.Utils
{
    public static class Decryptor
    {
        public static string DecryptString(string input)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(input));
        }
    }
}
