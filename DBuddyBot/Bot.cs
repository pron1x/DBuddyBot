using DBuddyBot.Commands;
using DBuddyBot.Data;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Threading.Tasks;

namespace DBuddyBot
{
    public class Bot
    {
        public DiscordClient Client { get; }
        public CommandsNextExtension Commands { get; }

        public Bot(string token, string[] prefixes)
        {
            DiscordConfiguration config = new()
            {
                Token = token,
                MinimumLogLevel = LogLevel.Debug,
                LoggerFactory = new LoggerFactory().AddSerilog()
            };

            Client = new DiscordClient(config);


            ServiceProvider services = new ServiceCollection()
                .AddSingleton<IAppDatabase>(new DatabaseService())
                .BuildServiceProvider();

            CommandsNextConfiguration commandsConfig = new()
            {
                StringPrefixes = prefixes,
                EnableDms = false,
                Services = services
            };


            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<UserCommands>();
            Commands.RegisterCommands<AdminCommands>();

        }

        public Task StartAsync() => Client.ConnectAsync();

        public Task StopAsync() => Client.DisconnectAsync();
    }
}
