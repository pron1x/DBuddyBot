using DBuddyBot.Data;
using Serilog;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DBuddyBot
{
    public static class Bootstrapper
    {
        private static string _discordToken;
        private static string[] _commandPrefixes;
        private static IAppDatabase _database;

        public static string DiscordToken { get => _discordToken; }
        public static string[] CommandPrefixes { get => _commandPrefixes; }
        public static IAppDatabase Database { get => _database; }

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
                Log.Logger.Fatal("No Config file found, shutting down...");
                System.Environment.Exit(78);
            }

            string connectionString = @"Data Source=.\Data\buddybotdb.sqlite;Version=3;Pooling=True;Max Pool Size=20;";
            bool newSetup = !File.Exists(@".\Data\buddybotdb.sqlite");
            if (newSetup)
            {
                Log.Logger.Information("No database found. Creating new SQLite database");
                Directory.CreateDirectory(@".\Data");
                SQLiteConnection.CreateFile(@".\Data\buddybotdb.sqlite");
            }
            _database = new DatabaseService(connectionString);

            if(newSetup)
            {
                _database.SetupDatabase();
            }
        }
    }
}
