using DBuddyBot.Models;

namespace DBuddyBot.Data
{
    /// <summary>
    /// Interface for database access.
    /// </summary>
    public interface IAppDatabase
    {
        public void AddGame(Role game);
        public Role GetGame(string name);
        public bool TryGetGame(string name, out Role game);
        public Role GetGame(int id);
        public bool TryGetGame(int id, out Role game);
        public void RemoveGame(int id);
    }
}
