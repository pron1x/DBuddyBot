using DBuddyBot.Models;

namespace DBuddyBot.Data
{
    /// <summary>
    /// Interface for database access.
    /// </summary>
    public interface IAppDatabase
    {
        public void AddGame(Game game);
        public Game GetGame(string name);
        public bool TryGetGame(string name, out Game game);
        public Game GetGame(int id);
        public bool TryGetGame(int id, out Game game);
        public void RemoveGame(int id);
    }
}
