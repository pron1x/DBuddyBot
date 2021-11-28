﻿using DBuddyBot.Models;
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

        public int AddCategory(string name, ulong channelId, int color)
        {
            Channel channel = GetChannel(channelId);
            int cId = channel == null ? AddChannel(channelId) : channel.Id;
            if (GetCategory(name) == null)
            {
                using IDbConnection connection = GetConnection(_connectionString);
                using IDbCommand command = GetCommand(InsertCategory, connection);
                command.Parameters.Add(GetParameterWithValue(command.CreateParameter(), "$name", name.ToLower()));
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
            if (GetRole(role.Id) == null)
            {
                using IDbConnection connection = GetConnection(_connectionString);
                using IDbCommand commandRole = GetCommand(InsertRole, connection);
                commandRole.Parameters.Add(GetParameterWithValue(commandRole.CreateParameter(), "$roleId", role.Id));
                commandRole.Parameters.Add(GetParameterWithValue(commandRole.CreateParameter(), "$roleName", role.Name.ToLower()));
                commandRole.Parameters.Add(GetParameterWithValue(commandRole.CreateParameter(), "$roleIsGame", role.IsGame));
                commandRole.Parameters.Add(GetParameterWithValue(commandRole.CreateParameter(), "$categoryId", categoryId));

                connection.Open();
                commandRole.ExecuteNonQuery();
                connection.Close();
            }
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
                    role = new((ulong)reader.GetInt64(0), reader.GetString(1), reader.GetBoolean(2));
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
                    role = new((ulong)reader.GetInt64(0), reader.GetString(1), reader.GetBoolean(2));
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

        public void RemoveRole(ulong id)
        {
            using IDbConnection connection = GetConnection(_connectionString);
            using IDbCommand commandRoles = GetCommand(DeleteRoleOnId, connection);
            commandRoles.Parameters.Add(GetParameterWithValue(commandRoles.CreateParameter(), "$id", id));

            connection.Open();
            commandRoles.ExecuteNonQuery();
            connection.Close();
        }

        public void RemoveCategory(Category category)
        {
            foreach(Role role in category.Roles)
            {
                RemoveRole(role.Id);
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
                int categoryColor = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                ulong messageId = reader.IsDBNull(3) ? 0 : (ulong)reader.GetInt64(3);
                int channelId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                ulong channelDiscordId = reader.IsDBNull(5) ? 0 : (ulong)reader.GetInt64(5);
                ulong roleId = reader.IsDBNull(6) ? 0 : (ulong)reader.GetInt64(6);
                string roleName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7);
                bool roleIsGame = !reader.IsDBNull(8) && reader.GetBoolean(8);

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
                    category = new(categoryId, categoryName, new DSharpPlus.Entities.DiscordColor(categoryColor), channel, message);
                }
                if (roleId != 0)
                {
                    Role role = new(roleId, roleName, roleIsGame);
                    roles.Add(role);
                }
            }
            roles.ForEach(x => category?.AddRole(x));
            return category;
        }

        #endregion privatemethods
    }
}
