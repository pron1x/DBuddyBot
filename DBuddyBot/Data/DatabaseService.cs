using DBuddyBot.Models;
using System.Collections.Generic;
using System.Linq;

namespace DBuddyBot.Data
{
    public class DatabaseService : IAppDatabase
    {
        private readonly List<Game> _games = new();

        public void AddGame(Game game)
        {
            if (!_games.Contains(game))
            {
                _games.Add(game);
            }
        }

        public Game GetGame(string name)
            => _games.FirstOrDefault(g => g.Name == name);

        public Game GetGame(int id)
            => _games.FirstOrDefault(g => g.Id == id);

        public void RemoveGame(int id)
        {
            if (TryGetGame(id, out Game game))
            {
                _games.Remove(game);
            }
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
    }
}
