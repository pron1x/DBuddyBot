using DBuddyBot.Models;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DBuddyBot.Data
{
    class SQLDatabaseService : IDatabaseService
    {
        #region backingfields
        private readonly string _connectionString;
        #endregion backingfields


        #region constructors
        public SQLDatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }
        #endregion constructors


        #region publicmethods
        public Role GetRole(string name)
        {
            Role role = null;
            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand command = new("SELECT * FROM roles WHERE lower(name) = $name;", connection);
                command.Parameters.AddWithValue("$name", name.ToLower());

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
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
            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand command = new("SELECT * FROM roles WHERE id = $id;", connection);
                command.Parameters.AddWithValue("$name", id);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    role = new((ulong)reader["id"], (string)reader["name"], (ulong)reader["emote"], (bool)reader["game"]);
                }
                connection.Close();
            }
            return role;
        }


        public bool TryGetRole(string name, out Role game)
        {
            game = GetRole(name);
            return game != null;
        }


        public bool TryGetRole(ulong id, out Role game)
        {
            game = GetRole(id);
            return game != null;
        }


        public void AddRole(Role role, int categoryId)
        {
            //using SqlConnection _connection = new(_connectionString);
            //using SqlCommand command = new("INSERT INTO games (id, name, emoji) VALUES ($id, $name, $emoji);", _connection);
            //command.Parameters.AddWithValue("$id", role.Id);
            //command.Parameters.AddWithValue("$name", role.Name);
            //command.Parameters.AddWithValue("$emoji", role.EmoteId);

            //_connection.Open();
            //command.ExecuteNonQueryAsync();
            //_connection.Close();
        }


        public void RemoveRole(ulong id)
        {
            using SqlConnection _connection = new(_connectionString);
            using SqlCommand command = new("DELETE FROM roles WHERE id = $id;", _connection);
            command.Parameters.AddWithValue("$id", id);

            _connection.Open();
            command.ExecuteNonQueryAsync();
            _connection.Close();
        }

        public Category GetCategory(int id)
        {
            Category category = null;
            Channel channel = null;
            List<Role> roles = new();
            RoleMessage message = null;
            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand command = new("SELECT categories.id, categories.name, channels.id as channelId, channels.name as channelName, roles.id as roleId, roles.name as roleName, roles.emote as roleEmote, roles.game as roleGame, role_messages.id as messageId"
                                                    + "FROM categories, channels, roles, role_messages WHERE channels.category_id = categories.id AND roles.category_id = categories.id and role_messages.channel_id = channels.id and categories.id = $id;", connection);
                command.Parameters.AddWithValue("$id", id);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
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
            Channel channel = null;
            List<Role> roles = new();
            RoleMessage message = null;
            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand command = new("SELECT categories.id, categories.name, channels.id as channelId, channels.name as channelName, roles.id as roleId, roles.name as roleName, roles.emote as roleEmote, roles.game as roleGame, role_messages.id as messageId"
                                                    + "FROM categories, channels, roles, role_messages WHERE channels.category_id = categories.id AND roles.category_id = categories.id and role_messages.channel_id = channels.id and categories.name = $name;", connection);
                command.Parameters.AddWithValue("$name", name);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
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

        public void Save(Category category)
        {
            throw new System.NotImplementedException();
        }

        public int AddCategory(string name)
        {
            throw new System.NotImplementedException();
        }

        public void AddChannel(ulong channelId, int categoryId)
        {
            throw new System.NotImplementedException();
        }



        #endregion publicmethods
    }
}
