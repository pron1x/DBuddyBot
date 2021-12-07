﻿using DBuddyBot.Commands;
using DBuddyBot.EventHandlers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Threading.Tasks;

namespace DBuddyBot
{
    public class Bot
    {
        #region properties
        public DiscordClient Client { get; }
        public CommandsNextExtension Commands { get; }
        #endregion properties


        #region constructors
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
                .AddSingleton(Bootstrapper.Database)
                .BuildServiceProvider();

            CommandsNextConfiguration commandsConfig = new()
            {
                StringPrefixes = prefixes,
                EnableDms = false,
                Services = services
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            SlashCommandsExtension slashCommands = Client.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = new ServiceCollection().AddSingleton(Bootstrapper.Database).BuildServiceProvider()
            });

            Commands.RegisterCommands<UserCommands>();

            slashCommands.RegisterCommands<RoleCommandsGroupContainer>();
            slashCommands.RegisterCommands<CategoryCommandsGroupContainer>();

            Client.GuildDownloadCompleted += ClientGuildEventHandler.SendRoleMessages;
            Client.ComponentInteractionCreated += ComponentInteractionHandler.HandleComponentInteraction;

        }

        #endregion constructors


        #region publicmethods
        public async void StartAsync()
        {
            try
            {
                await Client.ConnectAsync();
            }
            catch (System.Exception e)
            {
                Log.Logger.Fatal($"Bot could not connect to the discord server. {e.Message}");
                System.Environment.Exit(1);
            }
        }


        public Task StopAsync() => Client.DisconnectAsync();

        #endregion publicmethods
    }
}
