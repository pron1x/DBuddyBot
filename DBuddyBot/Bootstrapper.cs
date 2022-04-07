using DBuddyBot.Data;
using Serilog;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;

namespace DBuddyBot;

public static class Bootstrapper
{
    #region constants
    private static readonly string _version = "1.1";
    private static readonly string _configFilePath = Path.Combine(Environment.CurrentDirectory, "Config", "BotConfig.json");
    private static readonly string _defaultDatabase = $@"Data Source={Path.Combine(Environment.CurrentDirectory, "Data", "buddybotdb.sqlite")};Version=3;Pooling=True;Max Pool Size=50;";
    #endregion constants

    #region backingfields
    private static string _discordToken;
    private static IDatabaseService _database;
    private static string _databaseConnectionString;
    private static string _databaseFilePath;
    #endregion backingfields

    #region properties
    public static string DiscordToken { get => _discordToken; }
    public static IDatabaseService Database { get => _database; }
    #endregion properties


    #region publicmethods
    public static void Setup()
    {
        PrintHeader();
        SetupLogger();

        if (File.Exists(_configFilePath))
        {
            Log.Logger.Information("Config file exists, reading settings");
            try
            {
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
                    SetupDatabase();
                }
                else
                {
                    _databaseConnectionString = _defaultDatabase;
                    Log.Logger.Warning("Config is missing database section. Using default SQLite database.");
                }

                if (json.RootElement.TryGetProperty("categories", out JsonElement categories))
                {
                    GetCategories(categories);
                }
                else
                {
                    Log.Logger.Warning("Config is missing categories section. No categories will be created on startup.");
                }
            }
            catch
            {
                Log.Logger.Fatal($"Error reading the config file. Creating a new one and shutting down...");
                CreateNewConfigFile();
                Environment.Exit(78);
            }
        }
        else
        {
            Log.Logger.Fatal("No Config file found, creating empty one and shutting down...");
            CreateNewConfigFile();
            Environment.Exit(78);
        }
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
            .MinimumLevel.Information()
            .WriteTo.File("log.txt", fileSizeLimitBytes: 10000000, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
#endif
    }


    private static void SetupDatabase()
    {
        bool sqliteDatabase = !_databaseConnectionString.ToLower().Contains("uid");
        DatabaseType databaseType = sqliteDatabase ? DatabaseType.SQLite : DatabaseType.Sql;
        if (sqliteDatabase)
        {
            Log.Logger.Information($"SQLite database detected.");
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
                Log.Logger.Information("New SQLite database file created.");
            }
        }
        Log.Logger.Information("Setting up database with required tables.");
        using IDbConnection connection = databaseType switch
        {
            DatabaseType.SQLite => new SQLiteConnection(_databaseConnectionString),
            DatabaseType.Sql => new SqlConnection(_databaseConnectionString),
            _ => null
        };
        using IDbCommand createCategories = CreateCommand(SqlStrings.CreateTableCategories, connection, databaseType);
        using IDbCommand createRoles = CreateCommand(SqlStrings.CreateTableRoles, connection, databaseType);
        using IDbCommand createCategorRoles = CreateCommand(SqlStrings.CreateTableCategoriesRoles, connection, databaseType);
        using IDbCommand createChannels = CreateCommand(SqlStrings.CreateTableChannels, connection, databaseType);
        try
        {
            connection.Open();
            createCategories.ExecuteNonQuery();
            createRoles.ExecuteNonQuery();
            createCategorRoles.ExecuteNonQuery();
            createChannels.ExecuteNonQuery();
            connection.Close();
        }
        catch (Exception e)
        {
            Log.Logger.Fatal($"Could not connect to database. Shutting down...\nError: {e.Message}");
            Environment.Exit(74);
        };
        _database = new DatabaseService(_databaseConnectionString, databaseType);
        Log.Logger.Information("Database set up.");
    }


    private static IDbCommand CreateCommand(string query, IDbConnection connection, DatabaseType type)
    {
        IDbCommand command = type switch
        {
            DatabaseType.SQLite => new SQLiteCommand(query),
            DatabaseType.Sql => new SqlCommand(query),
            _ => null
        };
        command.Connection = connection;
        return command;
    }


    private static void CreateNewConfigFile()
    {
        Directory.CreateDirectory(@".\Config");
        string nl = Environment.NewLine;
        File.WriteAllText(@".\Config\BotConfig.json", $"{{{nl}" +
            $"\t\"discord\": {{{nl}" +
            $"\t\t\"discord_token\": \"\"{nl}" +
            $"\t}},{nl}" +
            $"\t\"database\": {{{nl}" +
            $"\t\t\"connection_string\": \"connection\"{nl}" +
            $"\t}},{nl}" +
            $"\t\"categories\": [{nl}" +
            $"\t\t{{{nl}" +
            $"\t\t\t\"name\": \"\",{nl}" +
            $"\t\t\t\"channel\": ,{nl}" +
            $"\t\t\t\"color\": \"\",{nl}" +
            $"\t\t\t\"description\": \"\"{nl}" +
            $"\t\t}}{nl}" +
            $"\t]{nl}" +
            $"}}{nl}");
        Log.Logger.Information($"New database file created in directory {_configFilePath}.");
    }


    private static void ReadDiscordConfig(JsonElement discord)
    {
        Log.Logger.Debug("Parsing discord section.");
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
    }


    private static void ReadDatabaseConfig(JsonElement database)
    {
        Log.Logger.Debug("Parsing database section.");
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


    private static void GetCategories(JsonElement categories)
    {
        Log.Logger.Debug("Parsing categories section.");
        foreach (JsonElement element in categories.EnumerateArray())
        {
            if (element.TryGetProperty("name", out JsonElement name) && element.TryGetProperty("channel", out JsonElement channel))
            {
                if (channel.TryGetInt64(out long channelId))
                {
                    string color = element.TryGetProperty("color", out JsonElement c) ? c.GetString() : "#000000";
                    string description = element.TryGetProperty("description", out JsonElement d) ? d.GetString() : string.Empty;
                    DSharpPlus.Entities.DiscordColor discordColor;
                    try
                    {
                        discordColor = new(color);
                    }
                    catch
                    {
                        discordColor = new("#000000");
                        Log.Logger.Warning($"Ignoring faulty color for category {name.GetString()}");
                    }
                    Database.AddCategory(name.GetString(), description, (ulong)channelId, discordColor.Value);
                }
                else
                {
                    Log.Logger.Warning("Could not parse channel id. Skipping category.");
                }
            }
            else
            {
                Log.Logger.Warning($"Skipping a faulty category specified in config.");
            }
        }
    }

    private static void PrintHeader()
    {
        Console.WriteLine("  _____  ____            _     _       ____        _   \r\n" +
            " |  __ \\|  _ \\          | |   | |     |  _ \\      | |  \r\n" +
            " | |  | | |_) |_   _  __| | __| |_   _| |_) | ___ | |_ \r\n" +
            " | |  | |  _ <| | | |/ _` |/ _` | | | |  _ < / _ \\| __|\r\n" +
            " | |__| | |_) | |_| | (_| | (_| | |_| | |_) | (_) | |_ \r\n" +
            " |_____/|____/ \\__,_|\\__,_|\\__,_|\\__, |____/ \\___/ \\__|\r\n" +
            "                                  __/ |                \r\n" +
            "                                 |___/                 ");
        Console.WriteLine($"DBuddyBot version {_version}");
    }

    #endregion privatemethods
}
