using DBuddyBot.Models;

namespace DBuddyBot.Data
{
    /// <summary>
    /// Interface for database access.
    /// </summary>
    public interface IDatabaseService
    {
        public void AddGame(Role game);
        public Role GetGame(string name);
        public bool TryGetGame(string name, out Role game);
        public Role GetGame(int id);
        public bool TryGetGame(int id, out Role game);
        public void RemoveGame(int id);

        public Category GetCategory(int id);
        public Category GetCategory(string name);
        public void Save(Category category);
    }
}
