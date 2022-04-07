using DBuddyBot.Commands;
using DBuddyBot.EventHandlers;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Threading.Tasks;

namespace DBuddyBot;

public class Bot
{
    #region properties
    public DiscordClient Client { get; }
    #endregion properties


    #region constructors
    public Bot(string token)
    {
        DiscordConfiguration config = new()
        {
            Token = token,
#if DEBUG
            MinimumLogLevel = LogLevel.Debug,
#else
            MinimumLogLevel = LogLevel.Information,
#endif
            LoggerFactory = new LoggerFactory().AddSerilog()
        };

        Client = new DiscordClient(config);

        ServiceProvider services = new ServiceCollection()
            .AddSingleton(Bootstrapper.Database)
            .BuildServiceProvider();

        SlashCommandsExtension slashCommands = Client.UseSlashCommands(new SlashCommandsConfiguration
        {
            Services = services
        });


        slashCommands.RegisterCommands<RoleSlashCommands>();
        slashCommands.RegisterCommands<CategorySlashCommands>();

        Client.GuildDownloadCompleted += ClientGuildEventHandler.SendRoleMessages;
        Client.GuildRoleUpdated += ClientGuildEventHandler.UpdateRoleInDatabase;
        Client.GuildRoleDeleted += ClientGuildEventHandler.DeleteRoleFromDatabase;
        Client.MessageDeleted += ClientMessageEventHandler.RemoveRoleMessage;
        Client.ChannelDeleted += ClientGuildEventHandler.DeleteChannelFromDatabase;
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


    public Task StopAsync()
    {
        return Client.DisconnectAsync();
    }

    #endregion publicmethods
}
