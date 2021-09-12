using System;

namespace DBuddyBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Setup();
            Bot bot = new(Bootstrapper.DiscordToken, Bootstrapper.CommandPrefixes);

            bot.StartAsync();
            Console.Read();
            bot.StopAsync();
        }
    }
}
