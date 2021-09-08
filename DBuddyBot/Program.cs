using System;

namespace DBuddyBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new("token", new[] { "?" }); // TODO: Move this somewhere else to an external config file.

            bot.StartAsync().Wait(-1);
            Console.Read();
        }
    }
}
