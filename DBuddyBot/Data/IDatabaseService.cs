using DBuddyBot.Models;

namespace DBuddyBot.Data
{
    /// <summary>
    /// Interface for database access.
    /// </summary>
    public interface IDatabaseService
    {
        public void AddRole(Role role, int categoryId);
        public Role GetRole(string name);
        public bool TryGetRole(string name, out Role role);
        public Role GetRole(ulong id);
        public bool TryGetRole(ulong id, out Role role);
        public void RemoveRole(ulong id);

        public Category GetCategory(int id);
        public Category GetCategory(string name);
        public void Save(Category category);
        public int AddCategory(string name);
        public void AddChannel(ulong channelId, int categoryId);
    }
}
