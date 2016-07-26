using System;
using System.Globalization;

namespace Queue
{
    static class Utilities
    {
        public static void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(Format(format, args));
        }

        public static void Write(string format, params object[] args)
        {
            Console.Write(Format(format, args));
        }

        public static string Format(string format, params object[] args)
        {
            return String.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static void WaitKey(ConsoleKey consoleKey, bool vebose = true)
        {
            if (vebose)
            {
                Write("Press [{0}] to exit...", consoleKey);
            }
            var cki = Console.ReadKey(true);
            while (consoleKey != cki.Key)
            {
                cki = Console.ReadKey(true);
            }
        }
        
        public static void WaitKey()
        {
            Console.ReadKey(true);
        }
    }
}
