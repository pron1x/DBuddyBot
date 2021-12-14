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

        public int AddCategory(string name, string description, ulong channelId, int color)
        {
            Channel channel = GetChannel(channelId);
            int cId = channel == null ? AddChannel(channelId) : channel.Id;
            if (GetCategory(name) == null)
            {
                using IDbConnection connection = GetConnection(_connectionString);
                using IDbCommand command = GetCommand(InsertCategory, connection);
                command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$name", name.ToLower()));
                command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$description", description));
                command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$channelId", cId));
                command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$color", color));
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            Category category = GetCategory(name);
            return category == null ? -1 : category.Id;
        }

        public void AddRole(Role role, int categoryId)
        {
            if (GetRole(role.DiscordId) == null)
            {
                using IDbConnection connection = GetConnection(_connectionString);
                using IDbCommand commandRole = GetCommand(InsertRole, connection);
                commandRole.Parameters.Add(GetParameterWithValue(commandRole.CreateParameter(), "$roleId", role.DiscordId));
                commandRole.Parameters.Add(GetParameterWithValue(commandRole.CreateParameter(), "$roleName", role.Name.ToLower()));
                commandRole.Parameters.Add(GetParameterWithValue(commandRole.CreateParameter(), "$description", role.Description));
                commandRole.Parameters.Add(GetParameterWithValue(commandRole.CreateParameter(), "$roleIsGame", role.IsGame));
                connection.Open();
                commandRole.ExecuteNonQuery();
                connection.Close();
            }
            role = GetRole(role.DiscordId);
            using IDbConnection conn = GetConnection(_connectionString);
            using IDbCommand commandCategoryRoles = GetCommand(InsertCategoryRole, conn);
            commandCategoryRoles.Parameters.Add(GetParameterWithValue(commandCategoryRoles.CreateParameter(), "$categoryId", categoryId));
            commandCategoryRoles.Parameters.Add(GetParameterWithValue(commandCategoryRoles.CreateParameter(), "roleId", role.Id));
            conn.Open();
            commandCategoryRoles.ExecuteNonQuery();
            conn.Close();
        }

        public int AddChannel(ulong channelId)
        {
            Channel channel = GetChannel(channelId);
            if (channel == null)
            {
                using IDbConnection connection = GetConnection(_connectionString);
                using IDbCommand command = GetCommand(InsertChannel, connection);
                command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$channelId", channelId));
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                channel = GetChannel(channelId);
            }
            return channel.Id;
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

        public Category UpdateCategoryDescription(Category category, string description)
        {
            using IDbConnection connection = GetConnection(_connectionString);
            using IDbCommand command = GetCommand(UpdateCategoryDescriptionOnId, connection);
            command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$description", description));
            command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$categoryId", category.Id));
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
            return GetCategory(category.Name);
        }

        public void UpdateRoleName(ulong roleId, string name)
        {
            using IDbConnection connection = GetConnection(_connectionString);
            using IDbCommand command = GetCommand(UpdateRoleNameOnDId, connection);
            command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$name", name));
            command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "id", roleId));
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void UpdateRoleDescription(int roleId, string description)
        {
            using IDbConnection connection = GetConnection(_connectionString);
            using IDbCommand command = GetCommand(UpdateRoleDescriptionOnId, connection);
            command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$description", description));
            command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "roleId", roleId));
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

        public List<string> GetAllCategoryNames()
        {
            List<string> categoryNames = new();
            using IDbConnection connection = GetConnection(_connectionString);
            using IDbCommand command = GetCommand(SelectCategoryNames, connection);
            connection.Open();
            IDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                categoryNames.Add(reader.GetString(0));
            }
            connection.Close();
            return categoryNames;
        }

        public List<Category> GetAllCategories()
        {
            List<Category> categories = new();
            foreach (string name in GetAllCategoryNames())
            {
                categories.Add(GetCategory(name));
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

        public List<Role> GetAllRoles()
        {
            List<Role> roles = new();
            using IDbConnection connection = GetConnection(_connectionString);
            using IDbCommand command = GetCommand(SelectAllRoles, connection);
            connection.Open();
            using IDataReader reader = command.ExecuteReader();
            while(reader.Read())
            {
                roles.Add(new(reader.GetInt32(0), (ulong)reader.GetInt64(1), reader.GetString(2), reader.GetString(3), reader.GetBoolean(4)));
            }
            connection.Close();
            return roles;
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
                    role = new(reader.GetInt32(0), (ulong)reader.GetInt64(1), reader.GetString(2), reader.GetString(3), reader.GetBoolean(4));
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
                    role = new(reader.GetInt32(0), (ulong)reader.GetInt64(1), reader.GetString(2), reader.GetString(3), reader.GetBoolean(4));
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
                int id = reader.GetInt32(0);
                ulong discordId = (ulong)reader.GetInt64(1);
                channel = new(id, discordId);
            }
            return channel;
        }

        public void RemoveRole(int categoryId, int roleId)
        {
            using IDbConnection connection = GetConnection(_connectionString);
            using IDbCommand commandCategoryRoles = GetCommand(DeleteRoleCategoryOnRoleId, connection);
            using IDbCommand commandRoles = GetCommand(DeleteRoleOnId, connection);
            commandCategoryRoles.Parameters.Add(GetParameterWithValue(commandCategoryRoles.CreateParameter(), "$categoryId", categoryId));
            commandCategoryRoles.Parameters.Add(GetParameterWithValue(commandCategoryRoles.CreateParameter(), "$roleId", roleId));
            commandRoles.Parameters.Add(GetParameterWithValue(commandRoles.CreateParameter(), "$id", roleId));

            connection.Open();
            commandCategoryRoles.ExecuteNonQuery();
            commandRoles.ExecuteNonQuery();
            connection.Close();
        }

        public void RemoveCategory(Category category)
        {
            foreach (Role role in category.Roles)
            {
                RemoveRole(category.Id, role.Id);
            }
            using IDbConnection connection = GetConnection(_connectionString);
            using IDbCommand commandChannel = GetCommand(DeleteChannelOnId, connection);
            using IDbCommand commandCategory = GetCommand(DeleteCategoryOnId, connection);
            commandChannel.Parameters.Add(GetParameterWithValue(commandChannel.CreateParameter(), "$id", category.Channel.Id));
            commandCategory.Parameters.Add(GetParameterWithValue(commandCategory.CreateParameter(), "$id", category.Id));

            connection.Open();
            commandChannel.ExecuteNonQuery();
            commandCategory.ExecuteNonQuery();
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
                string categoryDescription = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                int categoryColor = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                ulong messageId = reader.IsDBNull(4) ? 0 : (ulong)reader.GetInt64(4);
                int channelId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                ulong channelDiscordId = reader.IsDBNull(6) ? 0 : (ulong)reader.GetInt64(6);
                int roleId = reader.IsDBNull(7) ? -1 : reader.GetInt32(7);
                ulong roleDId = reader.IsDBNull(8) ? 0 : (ulong)reader.GetInt64(8);
                string roleName = reader.IsDBNull(9) ? string.Empty : reader.GetString(9);
                string roleDescription = reader.IsDBNull(10) ? string.Empty : reader.GetString(10);
                bool roleIsGame = !reader.IsDBNull(11) && reader.GetBoolean(11);

                if (category == null && categoryId != -1 && categoryName != string.Empty)
                {
                    Channel channel = null;
                    if (channelId != 0)
                    {
                        channel = new(channelId, channelDiscordId);
                    }
                    if (message == null && messageId != 0)
                    {
                        message = new(messageId);
                    }
                    category = new(categoryId, categoryName, categoryDescription, new DSharpPlus.Entities.DiscordColor(categoryColor), channel, message);
                }
                if (roleDId != 0)
                {
                    Role role = new(roleId, roleDId, roleName, roleDescription, roleIsGame);
                    roles.Add(role);
                }
            }
            roles.ForEach(x => category?.AddRole(x));
            return category;
        }

        #endregion privatemethods
    }
}
