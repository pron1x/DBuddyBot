using DBuddyBot.Models;
using Serilog;
using System.Data.SqlClient;

namespace DBuddyBot.Data
{
    class SQLDatabaseService : IAppDatabase
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
        public Game GetGame(string name)
        {
            Game game = null;
            using (SqlConnection _connection = new(_connectionString))
            {
                using SqlCommand command = new("SELECT * FROM games WHERE lower(name) = $name;", _connection);
                command.Parameters.AddWithValue("$name", name.ToLower());

                _connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    game = new((int)reader["id"], (string)reader["name"], (int)reader["subscribers"]);
                }
                _connection.Close();
            }
            return game;
        }

        public Game GetGame(int id)
        {
            Game game = null;
            using (SqlConnection _connection = new(_connectionString))
            {
                using SqlCommand command = new("SELECT * FROM games WHERE id = $id;", _connection);
                command.Parameters.AddWithValue("$name", id);

                _connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    game = new((int)reader["id"], (string)reader["name"], (int)reader["subscribers"]);
                }
                _connection.Close();
            }
            return game;
        }

        public bool TryGetGame(string name, out Game game)
        {
            game = GetGame(name);
            return game != null;
        }

        public bool TryGetGame(int id, out Game game)
        {
            game = GetGame(id);
            return game != null;
        }

        public void AddGame(Game game)
        {
            using SqlConnection _connection = new(_connectionString);
            using SqlCommand command = new("INSERT INTO games (id, name, subscribers) VALUES ($id, $name, $subscribers);", _connection);
            command.Parameters.AddWithValue("$id", game.Id);
            command.Parameters.AddWithValue("$name", game.Name);
            command.Parameters.AddWithValue("$subscribers", game.Subscribers);

            _connection.Open();
            command.ExecuteNonQueryAsync();
            _connection.Close();
        }

        public void RemoveGame(int id)
        {
            using SqlConnection _connection = new(_connectionString);
            using SqlCommand command = new("DELETE FROM games WHERE id = $id;", _connection);
            command.Parameters.AddWithValue("$id", id);

            _connection.Open();

            command.ExecuteNonQueryAsync();
            _connection.Close();
        }

        #endregion publicmethods

    }
}
