using DBuddyBot.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DBuddyBot
{
    public static class Bootstrapper
    {
        #region constants
        private static readonly string _configFilePath = @".\Config\BotConfig.json";
        private static readonly string _databaseFilePath = @".\Data\buddybotdb.sqlite";
        private static readonly string _databaseConnectionString = @"Data Source=.\Data\buddybotdb.sqlite;Version=3;Pooling=True;Max Pool Size=20;";
        #endregion constants

        #region backingfields
        private static string _discordToken;
        private static List<string> _commandPrefixes;
        private static IAppDatabase _database;
        #endregion backingfields

        #region properties
        public static string DiscordToken { get => _discordToken; }
        public static string[] CommandPrefixes { get => _commandPrefixes.ToArray(); }
        public static IAppDatabase Database { get => _database; }
        #endregion properties

        #region publicmethods
        public static void Setup()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            if (File.Exists(_configFilePath))
            {
                Log.Logger.Information("Config file exists, reading settings");
                using JsonDocument json = JsonDocument.Parse(File.ReadAllText(_configFilePath));
                if (json.RootElement.TryGetProperty("discord", out JsonElement discord))
                {
                    if (discord.TryGetProperty("discord_token", out JsonElement token))
                    {
                        _discordToken = token.GetString();
                        if (string.IsNullOrWhiteSpace(_discordToken))
                        {
                            Log.Logger.Fatal("Empty discord token in the config. Shutting down...");
                            Environment.Exit(78);
                        }
                    }
                    else
                    {
                        Log.Logger.Fatal("Config is missing discord_token element, shutting down...");
                    }
                    _commandPrefixes = new();
                    if (discord.TryGetProperty("command_prefixes", out JsonElement prefixes))
                    {
                        _commandPrefixes = prefixes.EnumerateArray().Select(x => x.GetString()).ToList();
                        if (_commandPrefixes.Count == 0 || (_commandPrefixes.Count == 1 && _commandPrefixes.Contains(string.Empty)))
                        {
                            Log.Logger.Warning("No command prefix found. Using standard '?' prefix.");
                            _commandPrefixes.Add("?");
                        }
                    }
                    else
                    {
                        Log.Logger.Warning("Config is missing command_prefixes element, using standard '?' prefix.");
                        _commandPrefixes.Add("?");
                    }
                }
            }
            else
            {
                Log.Logger.Fatal("No Config file found, creating empty one and shutting down...");
                Directory.CreateDirectory(@".\Config");
                string nl = Environment.NewLine;
                File.WriteAllText(@".\Config\BotConfig.json", $"{{{nl}\t\"discord\": {{{nl}\t\t\"discord_token\": \"\",{nl}\t\t\"command_prefixes\": [ \"\" ]{nl}\t}}{nl}}}{nl}");

                Environment.Exit(78);
            }

            bool newSetup = !File.Exists(_databaseFilePath);
            if (newSetup)
            {
                Log.Logger.Warning("No database found. Creating new SQLite database");
                Directory.CreateDirectory(@".\Data");
                SQLiteConnection.CreateFile(_databaseFilePath);
            }

            _database = new DatabaseService(_databaseConnectionString);

            if (newSetup)
            {
                _database.SetupDatabase();
            }
        }
        #endregion publicmethods
    }
}
