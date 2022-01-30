using System;
using System.Text;

namespace Noise.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
        }

        public static string FromUtf8ToBase64(this string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        public static string FromBase64ToUtf8(this string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
    }
}
