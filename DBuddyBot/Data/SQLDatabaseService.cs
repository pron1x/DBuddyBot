using DBuddyBot.Models;
using System.Collections.Generic;
using System.Data.SqlClient;

using static DBuddyBot.Data.SqlStrings;

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

        public int AddCategory(string name)
        {
            if (GetCategory(name) == null)
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(InsertCategory, connection);
                command.Parameters.AddWithValue("$name", name.ToTitleCase());
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
                using SqlConnection connection = new(_connectionString);
                using SqlCommand commandRole = new(InsertRole, connection);
                using SqlCommand commandEmoji = new(InsertEmoji, connection);
                commandRole.Parameters.AddWithValue("$roleId", role.Id);
                commandRole.Parameters.AddWithValue("$roleName", role.Name);
                commandRole.Parameters.AddWithValue("$roleIsGame", role.IsGame);
                commandRole.Parameters.AddWithValue("$categoryId", categoryId);
                commandEmoji.Parameters.AddWithValue("$emojiId", role.Emoji.Id);
                commandEmoji.Parameters.AddWithValue("emojiName", role.Emoji.Name);
                commandEmoji.Parameters.AddWithValue("emojiRole", role.Id);
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
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(InsertChannel, connection);
                command.Parameters.AddWithValue("$channelId", channelId);
                command.Parameters.AddWithValue("$categoryId", categoryId);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void UpdateMessage(int categorId, ulong messageId)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(UpdateCategoryMessage, connection);
            command.Parameters.AddWithValue("$messageId", messageId == 0 ? null : messageId);
            command.Parameters.AddWithValue("$categoryId", categorId);
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

        public List<Category> GetAllCategories()
        {
            List<Category> categories = new();
            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand command = new(SelectCategoryNames, connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
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
            List<Role> roles = new();
            RoleMessage message = null;
            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand command = new(SelectCategoryOnName, connection);
                command.Parameters.AddWithValue("$name", name.ToLower());
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
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
                    category.Roles.AddRange(roles);
                }
                connection.Close();
            }
            return category;
        }

        public Category GetCategoryFromMessage(ulong id)
        {
            Category category = null;
            List<Role> roles = new();
            RoleMessage message = null;
            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand command = new(SelectCategoryOnMessage, connection);
                command.Parameters.AddWithValue("messageId", id);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
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
                    category.Roles.AddRange(roles);
                }
                connection.Close();
            }
            return category;
        }

        public Role GetRole(string name)
        {
            Role role = null;
            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand command = new(SelectRoleOnName, connection);
                command.Parameters.AddWithValue("$name", name.ToLower());

                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
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
            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand command = new(SelectRoleOnId, connection);
                command.Parameters.AddWithValue("$id", id);

                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
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
            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand command = new(SelectRoleOnEmoji, connection);
                command.Parameters.AddWithValue("$emojiName", emojiName);
                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
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
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(SelectChannelOnId, connection);
            command.Parameters.AddWithValue("$id", channelId);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                ulong id = (ulong)reader.GetInt64(0);
                channel = new(id);
            }
            return channel;
        }

        public void RemoveRole(ulong id)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand commandEmojis = new(DeleteEmojiOnRoleId, connection);
            using SqlCommand commandRoles = new(DeleteRoleOnId, connection);
            commandEmojis.Parameters.AddWithValue("$id", id);
            commandRoles.Parameters.AddWithValue("$id", id);

            connection.Open();
            commandEmojis.ExecuteNonQueryAsync();
            commandRoles.ExecuteNonQueryAsync();
            connection.Close();
        }


        #endregion publicmethods
    }
}
