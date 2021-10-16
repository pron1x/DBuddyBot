using DBuddyBot.Models;
using Serilog;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DBuddyBot.Data
{
    public class SQLiteDatabaseService : IDatabaseService
    {
        #region backingfields
        private readonly string _connectionString;
        #endregion backingfields


        #region constructors
        public SQLiteDatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }
        #endregion constructors


        #region publicmethods
        public Role GetRole(string name)
        {
            Role role = null;
            using (SQLiteConnection connection = new(_connectionString))
            {
                using SQLiteCommand command = new("SELECT * FROM roles WHERE lower(name) = $name;", connection);
                command.Parameters.AddWithValue("$name", name.ToLower());

                connection.Open();
                using SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    role = new((ulong)reader["id"], (string)reader["name"], (ulong)reader["emote"], (bool)reader["game"]);
                }
                connection.Close();
            }
            return role;
        }


        public Role GetRole(ulong id)
        {
            Role role = null;
            using (SQLiteConnection connection = new(_connectionString))
            {
                using SQLiteCommand command = new("SELECT * FROM roles WHERE id = $id;", connection);
                command.Parameters.AddWithValue("$id", id);

                connection.Open();
                using SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    role = new((ulong)reader["id"], (string)reader["name"], (ulong)reader["emote"], (bool)reader["game"]);
                }
                connection.Close();
            }
            return role;
        }


        public bool TryGetRole(string name, out Role role)
        {
            role = GetRole(name);
            return role != null;
        }


        public bool TryGetRole(ulong id, out Role role)
        {
            role = GetRole(id);
            return role != null;
        }


        public void AddRole(Role role, int categoryId)
        {
            if (GetRole(role.Id) == null)
            {
                using SQLiteConnection connection = new(_connectionString);
                using SQLiteCommand command = new("INSERT INTO roles(id, name, emote, game, category_id) VALUES ($roleId, $roleName, $roleEmote, $roleIsGame, $categoryId)", connection);
                command.Parameters.AddWithValue("$roleId", role.Id);
                command.Parameters.AddWithValue("$roleName", role.Name);
                command.Parameters.AddWithValue("$roleEmote", role.EmoteId);
                command.Parameters.AddWithValue("$roleIsGame", role.IsGame);
                command.Parameters.AddWithValue("$categoryId", categoryId);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }


        public void RemoveRole(ulong id)
        {
            using SQLiteConnection connection = new(_connectionString);
            using SQLiteCommand command = new("DELETE FROM roles WHERE id = $id;", connection);
            command.Parameters.AddWithValue("$id", id);

            connection.Open();
            command.ExecuteNonQueryAsync();
            connection.Close();
        }

        public Category GetCategory(int id)
        {
            Category category = null;
            Channel channel = null;
            List<Role> roles = new();
            RoleMessage message = null;
            using (SQLiteConnection connection = new(_connectionString))
            {
                using SQLiteCommand command = new("SELECT categories.id, categories.name, channels.id as channelId, channels.name as channelName, roles.id as roleId, roles.name as roleName, roles.emote as roleEmote, roles.game as roleGame, role_messages.id as messageId"
                                                    + "FROM categories, channels, roles, role_messages WHERE channels.category_id = categories.id AND roles.category_id = categories.id and role_messages.channel_id = channels.id and categories.id = $id;", connection);
                command.Parameters.AddWithValue("$id", id);
                connection.Open();
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (category == null)
                    {
                        category = new((int)reader["id"], (string)reader["name"], new Channel((ulong)reader["channelId"], (string)reader["channelName"]));
                    }
                    if (channel == null)
                    {
                        channel = new((ulong)reader["channelId"], (string)reader["channelName"]);
                    }
                    if (message == null)
                    {
                        message = new((ulong)reader["messageId"]);
                    }
                    roles.Add(new((ulong)reader["roleId"], (string)reader["roleName"], (ulong)reader["roleEmote"], (bool)reader["roleGame"]));
                }
                connection.Close();
            }
            category.Channel.Messages.Add(message);
            category.Roles.AddRange(roles);

            return category;
        }

        public Category GetCategory(string name)
        {
            Category category = null;
            List<Role> roles = new();
            RoleMessage message = null;
            using (SQLiteConnection connection = new(_connectionString))
            {
                using SQLiteCommand command = new("SELECT ca.id AS categoryId, ca.name AS categoryName, ch.id AS channelId, ch.name AS channelName, ro.id AS roleId, ro.name AS roleName, ro.emote AS roleEmote, ro.game AS roleGame, rm.id AS messageId "
                                                    + "FROM categories ca LEFT JOIN channels ch ON ca.id = ch.category_id LEFT JOIN role_messages rm ON ch.id = rm.channel_id LEFT JOIN roles ro ON ca.id = ro.category_id WHERE lower(categoryName) = $name;", connection);
                command.Parameters.AddWithValue("$name", name.ToLower());
                connection.Open();
                SQLiteDataReader reader = command.ExecuteReader();
                Log.Logger.Debug($"Command executed, reading response... Rows? {reader.HasRows}");
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int categoryId = reader.IsDBNull(0) ? -1 : reader.GetInt32(0);
                        Log.Logger.Debug($"categorId done... Is: {categoryId}");
                        string categoryName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        Log.Logger.Debug($"categoryName done... Is: {categoryName}");
                        ulong channelId = reader.IsDBNull(2) ? 0 : (ulong)reader.GetInt64(2);
                        Log.Logger.Debug($"channelId done... Is: {channelId}");
                        string channelName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                        Log.Logger.Debug($"channelName done... Is: {channelName}");
                        ulong roleId = reader.IsDBNull(4) ? 0 : (ulong)reader.GetInt64(4);
                        Log.Logger.Debug($"roleId done... Is: {roleId}");
                        string roleName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                        Log.Logger.Debug($"roleName done... Is: {roleName}");
                        ulong roleEmote = reader.IsDBNull(6) ? 0 : (ulong)reader.GetInt64(6);
                        Log.Logger.Debug($"roleEmote done... Is: {roleEmote}");
                        bool roleIsGame = reader.IsDBNull(7) ? false : reader.GetBoolean(7);
                        Log.Logger.Debug($"roleIsGame done... Is: {roleIsGame}");
                        ulong messageId = reader.IsDBNull(8) ? 0 : (ulong)reader.GetInt64(8);
                        Log.Logger.Debug($"messageId done... Is: {messageId}");
                        Log.Logger.Debug($"All fields parsed...");

                        if (category == null && categoryId != -1 && categoryName != string.Empty)
                        {
                            Channel channel = null;
                            if (channelId != 0)
                            {
                                channel = new(channelId, channelName);
                            }
                            category = new(categoryId, categoryName, channel);
                        }
                        if (message == null && messageId != 0)
                        {
                            message = new(messageId);
                        }
                        if (roleId != 0)
                        {
                            Role role = new(roleId, roleName, roleEmote, roleIsGame);
                            roles.Add(role);
                        }
                    }
                    category?.Channel?.Messages.Add(message);
                    category?.Roles.AddRange(roles);
                }
                connection.Close();
            }
            return category;
        }

        public Channel GetChannel(ulong channelId)
        {
            Channel channel = null;
            using SQLiteConnection connection = new(_connectionString);
            using SQLiteCommand command = new("SELECT * FROM channels WHERE id=$id;", connection);
            command.Parameters.AddWithValue("$id", channelId);
            connection.Open();
            using SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                ulong id = (ulong)reader.GetInt64(0);
                string name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                channel = new(id, name);
            }
            return channel;
        }

        public int AddCategory(string name)
        {
            if (GetCategory(name) == null)
            {
                using SQLiteConnection connection = new(_connectionString);
                using SQLiteCommand command = new("INSERT INTO categories(name) VALUES ($name);", connection);
                command.Parameters.AddWithValue("$name", name.ToTitleCase());
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            Category category = GetCategory(name);
            return category == null ? -1 : category.Id;
        }

        public void AddChannel(ulong channelId, int categoryId)
        {
            if (GetChannel(channelId) == null)
            {
                using SQLiteConnection connection = new(_connectionString);
                using SQLiteCommand command = new("INSERT INTO channels(id, category_id) VALUES ($channelId, $categoryId);", connection);
                command.Parameters.AddWithValue("$channelId", channelId);
                command.Parameters.AddWithValue("$categoryId", categoryId);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void Save(Category category)
        {
            throw new System.NotImplementedException();
        }

        #endregion publicmethods
    }
}
