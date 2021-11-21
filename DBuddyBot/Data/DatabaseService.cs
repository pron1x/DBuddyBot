using DBuddyBot.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;

using static DBuddyBot.Data.SqlStrings;

namespace DBuddyBot.Data
{
    public enum DatabaseType
    {
        SQLite,
        Sql
    }

    public class DatabaseService : IDatabaseService
    {
        #region backingfields
        private readonly string _connectionString;
        private readonly DatabaseType _databaseType;
        #endregion backingfields


        #region constructors
        public DatabaseService(string connectionString, DatabaseType type)
        {
            _connectionString = connectionString;
            _databaseType = type;
        }
        #endregion constructors


        #region publicmethods

        public int AddCategory(string name)
        {
            if (GetCategory(name) == null)
            {
                using IDbConnection connection = GetConnection(_connectionString);
                using IDbCommand command = GetCommand(InsertCategory, connection);
                command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$name", name.ToTitleCase()));
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            Category category = GetCategory(name);
            return category == null ? -1 : category.Id;
        }

        public void AddRole(Role role, int categoryId)
        {
            if (GetRole(role.Id) == null)
            {
                using IDbConnection connection = GetConnection(_connectionString);
                using IDbCommand commandRole = GetCommand(InsertRole, connection);
                using IDbCommand commandEmoji = GetCommand(InsertEmoji, connection);
                commandRole.Parameters.Add(GetParameterWithValue(commandRole.CreateParameter(), "$roleId", role.Id));
                commandRole.Parameters.Add(GetParameterWithValue(commandRole.CreateParameter(), "$roleName", role.Name));
                commandRole.Parameters.Add(GetParameterWithValue(commandRole.CreateParameter(), "$roleIsGame", role.IsGame));
                commandRole.Parameters.Add(GetParameterWithValue(commandRole.CreateParameter(), "$categoryId", categoryId));

                commandEmoji.Parameters.Add(GetParameterWithValue(commandEmoji.CreateParameter(), "$emojiId", role.Emoji.Id));
                commandEmoji.Parameters.Add(GetParameterWithValue(commandEmoji.CreateParameter(), "emojiName", role.Emoji.Name));
                commandEmoji.Parameters.Add(GetParameterWithValue(commandEmoji.CreateParameter(), "emojiRole", role.Id));
                connection.Open();
                commandRole.ExecuteNonQuery();
                commandEmoji.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void AddChannel(ulong channelId, int categoryId)
        {
            if (GetChannel(channelId) == null)
            {
                using IDbConnection connection = GetConnection(_connectionString);
                using IDbCommand command = GetCommand(InsertChannel, connection);
                command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$channelId", channelId));
                command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$categoryId", categoryId));
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void UpdateMessage(int categorId, ulong messageId)
        {
            using IDbConnection connection = GetConnection(_connectionString);
            using IDbCommand command = GetCommand(UpdateCategoryMessage, connection);
            command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$messageId", messageId == 0 ? null : messageId));
            command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$categoryId", categorId));
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

        public List<Category> GetAllCategories()
        {
            List<Category> categories = new();
            using (IDbConnection connection = GetConnection(_connectionString))
            {
                using IDbCommand command = GetCommand(SelectCategoryNames, connection);

                connection.Open();
                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string categoryName = reader.GetString(0);
                    categories.Add(GetCategory(categoryName));
                }
                connection.Close();
            }
            return categories;
        }

        public Category GetCategory(string name)
        {
            Category category = null;
            using (IDbConnection connection = GetConnection(_connectionString))
            {
                using IDbCommand command = GetCommand(SelectCategoryOnName, connection);
                command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$name", name.ToLower()));
                connection.Open();
                IDataReader reader = command.ExecuteReader();
                category = ParseCategory(reader);

                connection.Close();
            }
            return category;
        }

        public Category GetCategoryFromMessage(ulong id)
        {
            Category category = null;
            using (IDbConnection connection = GetConnection(_connectionString))
            {
                using IDbCommand command = GetCommand(SelectCategoryOnMessage, connection);
                command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "messageId", id));
                connection.Open();
                IDataReader reader = command.ExecuteReader();
                category = ParseCategory(reader);
                connection.Close();
            }
            return category;
        }

        public Role GetRole(string name)
        {
            Role role = null;
            using (IDbConnection connection = GetConnection(_connectionString))
            {
                using IDbCommand command = GetCommand(SelectRoleOnName, connection);
                command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$name", name.ToLower()));

                connection.Open();
                using IDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    Emoji emoji = new((ulong)reader.GetInt64(3), reader.GetString(4));
                    role = new((ulong)reader.GetInt64(0), reader.GetString(1), emoji, reader.GetBoolean(2));
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

        public Role GetRole(ulong id)
        {
            Role role = null;
            using (IDbConnection connection = GetConnection(_connectionString))
            {
                using IDbCommand command = GetCommand(SelectRoleOnId, connection);
                command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$id", id));

                connection.Open();
                using IDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    Emoji emoji = new((ulong)reader.GetInt64(3), reader.GetString(4));
                    role = new((ulong)reader.GetInt64(0), reader.GetString(1), emoji, reader.GetBoolean(2));
                }
                connection.Close();
            }
            return role;
        }

        public bool TryGetRole(ulong id, out Role role)
        {
            role = GetRole(id);
            return role != null;
        }

        public Role GetRoleFromEmote(string emojiName)
        {
            Role role = null;
            using (IDbConnection connection = GetConnection(_connectionString))
            {
                using IDbCommand command = GetCommand(SelectRoleOnEmoji, connection);
                command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$emojiName", emojiName));
                connection.Open();
                using IDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    Emoji emoji = new((ulong)reader.GetInt64(3), reader.GetString(4));
                    role = new((ulong)reader.GetInt64(0), reader.GetString(1), emoji, reader.GetBoolean(2));
                }
                connection.Close();
            }
            return role;
        }

        public Channel GetChannel(ulong channelId)
        {
            Channel channel = null;
            using IDbConnection connection = GetConnection(_connectionString);
            using IDbCommand command = GetCommand(SelectChannelOnId, connection);
            command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$id", channelId));
            connection.Open();
            using IDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                ulong id = (ulong)reader.GetInt64(0);
                channel = new(id);
            }
            return channel;
        }

        public void RemoveRole(ulong id)
        {
            using IDbConnection connection = GetConnection(_connectionString);
            using IDbCommand commandEmojis = GetCommand(DeleteEmojiOnRoleId, connection);
            using IDbCommand commandRoles = GetCommand(DeleteRoleOnId, connection);
            commandEmojis.Parameters.Add(GetParameterWithValue(commandEmojis.CreateParameter(), "$id", id));
            commandRoles.Parameters.Add(GetParameterWithValue(commandRoles.CreateParameter(), "$id", id));

            connection.Open();
            commandEmojis.ExecuteNonQuery();
            commandRoles.ExecuteNonQuery();
            connection.Close();
        }

        #endregion publicmethods


        #region privatemethods

        private IDbConnection GetConnection(string connectionString)
        {
            IDbConnection connection = _databaseType switch
            {
                DatabaseType.SQLite => new SQLiteConnection(connectionString),
                DatabaseType.Sql => new SqlConnection(connectionString),
                _ => null
            };
            return connection;
        }

        private IDbCommand GetCommand(string query, IDbConnection connection)
        {
            IDbCommand command = _databaseType switch
            {
                DatabaseType.SQLite => new SQLiteCommand(query),
                DatabaseType.Sql => new SqlCommand(query),
                _ => null
            };
            command.Connection = connection;
            return command;
        }

        private static IDbDataParameter GetParameterWithValue(IDbDataParameter parameter, string name, object value)
        {
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }

        private static Category ParseCategory(IDataReader reader)
        {
            Category category = null;
            RoleMessage message = null;
            List<Role> roles = new();
            while (reader.Read())
            {
                int categoryId = reader.IsDBNull(0) ? -1 : reader.GetInt32(0);
                string categoryName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                ulong messageId = reader.IsDBNull(2) ? 0 : (ulong)reader.GetInt64(2);
                ulong channelId = reader.IsDBNull(3) ? 0 : (ulong)reader.GetInt64(3);
                ulong roleId = reader.IsDBNull(4) ? 0 : (ulong)reader.GetInt64(4);
                string roleName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                bool roleIsGame = !reader.IsDBNull(6) && reader.GetBoolean(6);
                ulong emojiId = reader.IsDBNull(7) ? 0 : (ulong)reader.GetInt64(7);
                string emojiName = reader.IsDBNull(8) ? string.Empty : reader.GetString(8);

                if (category == null && categoryId != -1 && categoryName != string.Empty)
                {
                    Channel channel = null;
                    if (channelId != 0)
                    {
                        channel = new(channelId);
                    }
                    if (message == null && messageId != 0)
                    {
                        message = new(messageId);
                    }
                    category = new(categoryId, categoryName, channel, message);
                }
                if (roleId != 0)
                {
                    Role role = new(roleId, roleName, new(emojiId, emojiName), roleIsGame);
                    roles.Add(role);
                }
            }
            category?.Roles.AddRange(roles);
            return category;
        }

        #endregion privatemethods
    }
}
