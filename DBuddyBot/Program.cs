using System;

namespace DBuddyBot;

public class Program
{
    public static void Main()
    {
        Bootstrapper.Setup();
        Bot bot = new(Bootstrapper.DiscordToken);

        bot.StartAsync();
        Console.Read();
        bot.StopAsync();
    }
}
