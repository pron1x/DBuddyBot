using DBuddyBot.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        private static readonly string _defaultDatabase = @"Data Source=.\Data\buddybotdb.sqlite;Version=3;Pooling=True;Max Pool Size=20;";
        #endregion constants

        #region backingfields
        private static string _discordToken;
        private static List<string> _commandPrefixes;
        private static IAppDatabase _database;
        private static string _databaseConnectionString;
        private static string _databaseFilePath;
        #endregion backingfields

        #region properties
        public static string DiscordToken { get => _discordToken; }
        public static string[] CommandPrefixes { get => _commandPrefixes.ToArray(); }
        public static IAppDatabase Database { get => _database; }
        #endregion properties

        #region publicmethods
        public static void Setup()
        {
            SetupLogger();

            if (File.Exists(_configFilePath))
            {
                Log.Logger.Information("Config file exists, reading settings");
                using JsonDocument json = JsonDocument.Parse(File.ReadAllText(_configFilePath));
                if (json.RootElement.TryGetProperty("discord", out JsonElement discord))
                {
                    ReadDiscordConfig(discord);
                }
                else
                {
                    Log.Logger.Fatal("Config is missing discord section. Creating new config and shutting down...");
                    CreateNewConfigFile();
                    Environment.Exit(78);
                }

                if (json.RootElement.TryGetProperty("database", out JsonElement database))
                {
                    ReadDatabaseConfig(database);
                }
                else
                {
                    _databaseConnectionString = _defaultDatabase;
                    Log.Logger.Warning("Config is missing database section. Using default SQLite database.");
                }
            }
            else
            {
                Log.Logger.Fatal("No Config file found, creating empty one and shutting down...");
                CreateNewConfigFile();
                Environment.Exit(78);
            }
            SetupDatabase();
        }
        #endregion publicmethods


        #region privatemethods
        private static void SetupLogger()
        {
#if DEBUG
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
#else
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.File("log.txt", fileSizeLimitBytes: 10000000, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
#endif
        }

        private static void SetupDatabase()
        {
            bool sqliteDatabase = !_databaseConnectionString.ToLower().Contains("uid");
            if (sqliteDatabase)
            {
                try
                {
                    SQLiteConnectionStringBuilder connStringBuilder = new(_databaseConnectionString);
                    _databaseFilePath = connStringBuilder.DataSource;
                }
                catch (Exception e)
                {
                    Log.Logger.Fatal($"SQLite connection string is malformed. Please enter a correct SQLite connection string.\nError: {e.Message}");
                    Environment.Exit(78);
                }
                if (!File.Exists(_databaseFilePath))
                {
                    Log.Logger.Warning("No database found. Creating new SQLite database.");
                    Directory.CreateDirectory(Path.GetDirectoryName(_databaseFilePath));
                    SQLiteConnection.CreateFile(_databaseFilePath);

                    using SQLiteConnection conn = new(_databaseConnectionString);
                    Log.Logger.Information($"Setting up SQLite database {conn.DataSource}.");
                    using SQLiteCommand command = new("CREATE TABLE IF NOT EXISTS games (id INT PRIMARY KEY , name TEXT, subscribers INT);", conn);

                    conn.Open();
                    command.ExecuteNonQueryAsync();
                    conn.Close();
                }
                using SQLiteConnection connection = new(_databaseConnectionString);
                try
                {
                    connection.Open();
                    connection.Close();
                }
                catch (Exception e)
                {
                    Log.Logger.Fatal($"Could not connect to SQLite database. Shutting down...\nError: {e.Message}");
                    Environment.Exit(74);
                }
                _database = new SQLiteDatabaseService(_databaseConnectionString);
            }
            else
            {
                using SqlConnection connection = new(_databaseConnectionString);
                try
                {
                    using SqlCommand command = new("CREATE TABLE IF NOT EXISTS games (id INT PRIMARY KEY , name TEXT, subscribers INT);", connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception e)
                {
                    Log.Logger.Fatal($"Could not connect to SQL database. Shutting down...\nError: {e.Message}");
                    Environment.Exit(74);
                }
                _database = new SQLDatabaseService(_databaseConnectionString);
            }
        }

        private static void CreateNewConfigFile()
        {
            Directory.CreateDirectory(@".\Config");
            string nl = Environment.NewLine;
            File.WriteAllText(@".\Config\BotConfig.json", $"{{{nl}\t\"discord\": {{{nl}\t\t\"discord_token\": \"\",{nl}\t\t\"command_prefixes\": [ \"\" ]{nl}\t}},{nl}\t\"database\": {{{nl}\t\t\"connection_string\": \"connection\"{nl}\t}}{nl}}}{nl}");
        }

        private static void ReadDiscordConfig(JsonElement discord)
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
                Log.Logger.Fatal("Config is missing discord_token element, creating new config and shutting down...");
                CreateNewConfigFile();
                Environment.Exit(78);
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

        private static void ReadDatabaseConfig(JsonElement database)
        {
            if (database.TryGetProperty("connection_string", out JsonElement connection))
            {
                _databaseConnectionString = connection.GetString();
                if (string.IsNullOrWhiteSpace(_databaseConnectionString))
                {
                    _databaseConnectionString = _defaultDatabase;
                    Log.Logger.Warning("Database connection string found in config is empty. Using default SQLite database.");
                }
                else
                {
                    Log.Logger.Information("Using specified database connection string.");
                }
            }
            else
            {
                _databaseConnectionString = _defaultDatabase;
                Log.Logger.Warning("Config is missing connection_string element. Using default SQLite database.");
            }
        }

        #endregion privatemethods
    }
}
