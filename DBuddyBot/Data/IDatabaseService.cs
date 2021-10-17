using DBuddyBot.Models;

namespace DBuddyBot.Data
{
    /// <summary>
    /// Interface for database access.
    /// </summary>
    public interface IDatabaseService
    {
        public int AddCategory(string name);
        public void AddRole(Role role, int categoryId);
        public void AddChannel(ulong channelId, int categoryId);
        public Category GetCategory(string name);
        public Role GetRole(string name);
        public bool TryGetRole(string name, out Role role);
        public Role GetRole(ulong id);
        public bool TryGetRole(ulong id, out Role role);
        public Channel GetChannel(ulong channelId);
        public void RemoveRole(ulong id);

    }
}
