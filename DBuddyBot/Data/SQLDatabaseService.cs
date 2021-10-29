﻿using DBuddyBot.Models;
using Serilog;
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

        public int AddCategory(string name)
        {
            if (GetCategory(name) == null)
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new("INSERT INTO categories(name) VALUES ($name);", connection);
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
                using SqlCommand command = new("INSERT INTO roles(id, name, emote, game, category_id) VALUES ($roleId, $roleName, $roleEmote, $roleIsGame, $categoryId)", connection);
                command.Parameters.AddWithValue("$roleId", role.Id);
                command.Parameters.AddWithValue("$roleName", role.Name);
                command.Parameters.AddWithValue("$roleEmote", role.Emote);
                command.Parameters.AddWithValue("$roleIsGame", role.IsGame);
                command.Parameters.AddWithValue("$categoryId", categoryId);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void AddChannel(ulong channelId, int categoryId)
        {
            if (GetChannel(channelId) == null)
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new("INSERT INTO channels(id, category_id) VALUES ($channelId, $categoryId);", connection);
                command.Parameters.AddWithValue("$channelId", channelId);
                command.Parameters.AddWithValue("$categoryId", categoryId);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public List<Category> GetAllCategories()
        {
            List<Category> categories = new();
            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand command = new("SELECT name FROM categories;", connection);

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
                using SqlCommand command = new("SELECT ca.id AS categoryId, ca.name AS categoryName, ca.message as categoryMessage, ch.id AS channelId, ro.id AS roleId, ro.name AS roleName, ro.emote AS roleEmote, ro.game AS roleGame "
                                                    + "FROM categories ca LEFT JOIN channels ch ON ca.id = ch.category_id LEFT JOIN roles ro ON ca.id = ro.category_id WHERE lower(categoryName) = $name;", connection);
                command.Parameters.AddWithValue("$name", name.ToLower());
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                Log.Logger.Debug($"Command executed, reading response... Rows? {reader.HasRows}");
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int categoryId = reader.IsDBNull(0) ? -1 : reader.GetInt32(0);
                        Log.Logger.Debug($"categorId done... Is: {categoryId}");
                        string categoryName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        Log.Logger.Debug($"categoryName done... Is: {categoryName}");
                        ulong messageId = reader.IsDBNull(2) ? 0 : (ulong)reader.GetInt64(2);
                        Log.Logger.Debug($"messageId done... Is: {messageId}");
                        ulong channelId = reader.IsDBNull(3) ? 0 : (ulong)reader.GetInt64(3);
                        Log.Logger.Debug($"channelId done... Is: {channelId}");
                        ulong roleId = reader.IsDBNull(4) ? 0 : (ulong)reader.GetInt64(4);
                        Log.Logger.Debug($"roleId done... Is: {roleId}");
                        string roleName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                        Log.Logger.Debug($"roleName done... Is: {roleName}");
                        string roleEmote = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                        Log.Logger.Debug($"roleEmote done... Is: {roleEmote}");
                        bool roleIsGame = reader.IsDBNull(7) ? false : reader.GetBoolean(7);
                        Log.Logger.Debug($"roleIsGame done... Is: {roleIsGame}");
                        Log.Logger.Debug($"All fields parsed...");

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
                            Role role = new(roleId, roleName, roleEmote, roleIsGame);
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
                using SqlCommand command = new("SELECT * FROM roles WHERE lower(name) = $name;", connection);
                command.Parameters.AddWithValue("$name", name.ToLower());

                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    role = new((ulong)reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3));
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
                using SqlCommand command = new("SELECT * FROM roles WHERE id = $id;", connection);
                command.Parameters.AddWithValue("$id", id);

                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    role = new((ulong)reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3));
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

        public Role GetRoleFromEmote(string emote)
        {
            Role role = null;
            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand command = new("SELECT * FROM roles WHERE emote = $emote;", connection);
                command.Parameters.AddWithValue("$emote", emote);
                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    role = new((ulong)reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3));
                }
                connection.Close();
            }
            return role;
        }

        public Channel GetChannel(ulong channelId)
        {
            Channel channel = null;
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new("SELECT * FROM channels WHERE id=$id;", connection);
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
            using SqlCommand command = new("DELETE FROM roles WHERE id = $id;", connection);
            command.Parameters.AddWithValue("$id", id);

            connection.Open();
            command.ExecuteNonQueryAsync();
            connection.Close();
        }


        #endregion publicmethods
    }
}
