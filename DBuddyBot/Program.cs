using System;

namespace DBuddyBot
{
    class Program
    {
        static void Main()
        {
            Bootstrapper.Setup();
            Bot bot = new(Bootstrapper.DiscordToken);

            bot.StartAsync();
            Console.Read();
            bot.StopAsync();
        }
    }
}
