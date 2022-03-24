using System.Linq;

namespace Noise.Host.Exceptions
{
    internal static class StringArrayExtensions
    {
        public static bool FirstIs(this string[] args, string value)
        {
            if (args.Length == 0)
                return false;

            return args.First().ToLower() == value.ToLower();
        }
    }
}
