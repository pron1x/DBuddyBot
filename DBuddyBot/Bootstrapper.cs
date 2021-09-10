using Serilog;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DBuddyBot
{
    public static class Bootstrapper
    {
        private static string _discordToken;
        private static string[] _commandPrefixes;

        public static string DiscordToken { get => _discordToken; }
        public static string[] CommandPrefixes { get => _commandPrefixes; }

        public static void Setup()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            string filePath = @".\Config\BotConfig.json";
            if (File.Exists(filePath))
            {
                Log.Logger.Information("Config file exists, reading settings");
                using JsonDocument json = JsonDocument.Parse(File.ReadAllText(filePath));
                if (json.RootElement.TryGetProperty("discord", out JsonElement discord))
                {
                    if (discord.TryGetProperty("discord_token", out JsonElement token))
                    {
                        _discordToken = token.GetString();
                    }
                    if (discord.TryGetProperty("command_prefixes", out JsonElement prefixes))
                    {
                        _commandPrefixes = prefixes.EnumerateArray().Select(x => x.GetString()).ToArray();
                    }

                }
            }
            else
            {
                Log.Logger.Error("No Config file found, shutting down...");
                System.Environment.Exit(78);
            }
        }
    }
}
