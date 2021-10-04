﻿using DBuddyBot.Models;
using System.Data.SQLite;

namespace DBuddyBot.Data
{
    public class SQLiteDatabaseService : IAppDatabase
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
        public Role GetGame(string name)
        {
            Role game = null;
            using (SQLiteConnection _connection = new(_connectionString))
            {
                using SQLiteCommand command = new("SELECT * FROM games WHERE lower(name) = $name;", _connection);
                command.Parameters.AddWithValue("$name", name.ToLower());

                _connection.Open();
                using SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    game = new((int)reader["id"], (string)reader["name"], (string)reader["emoji"]);
                }
                _connection.Close();
            }
            return game;
        }


        public Role GetGame(int id)
        {
            Role game = null;
            using (SQLiteConnection _connection = new(_connectionString))
            {
                using SQLiteCommand command = new("SELECT * FROM games WHERE id = $id;", _connection);
                command.Parameters.AddWithValue("$name", id);

                _connection.Open();
                using SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    game = new((int)reader["id"], (string)reader["name"], (string)reader["emoji"]);
                }
                _connection.Close();
            }
            return game;
        }


        public bool TryGetGame(string name, out Role game)
        {
            game = GetGame(name);
            return game != null;
        }


        public bool TryGetGame(int id, out Role game)
        {
            game = GetGame(id);
            return game != null;
        }


        public void AddGame(Role game)
        {
            using SQLiteConnection _connection = new(_connectionString);
            using SQLiteCommand command = new("INSERT INTO games (id, name, emoji) VALUES ($id, $name, $emoji);", _connection);
            command.Parameters.AddWithValue("$id", game.Id);
            command.Parameters.AddWithValue("$name", game.Name);
            command.Parameters.AddWithValue("$emoji", game.EmoteId);

            _connection.Open();
            command.ExecuteNonQueryAsync();
            _connection.Close();
        }


        public void RemoveGame(int id)
        {
            using SQLiteConnection _connection = new(_connectionString);
            using SQLiteCommand command = new("DELETE FROM games WHERE id = $id;", _connection);
            command.Parameters.AddWithValue("$id", id);

            _connection.Open();
            command.ExecuteNonQueryAsync();
            _connection.Close();
        }

        #endregion publicmethods
    }
}
