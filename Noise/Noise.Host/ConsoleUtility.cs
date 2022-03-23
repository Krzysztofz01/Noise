using System;
using System.Text;

namespace Noise.Host
{
    internal static class ConsoleUtility
    {
        public static string ReadSecret(string message = null)
        {
            if (message != null) Console.Write(message);

            var sb = new StringBuilder();

            while (true)
            {
                var key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        Console.WriteLine(Environment.NewLine);
                        return sb.ToString();

                    case ConsoleKey.Backspace:
                        if (sb.Length == 0) continue;

                        sb.Length--;
                        Console.Write("\b \b");
                        continue;

                    default:
                        sb.Append(key.KeyChar);
                        Console.Write("*");
                        continue;
                }
            }
        }
    }
}
