using DBuddyBot.Models;
using System.Collections.Generic;

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
        public void UpdateMessage(int categorId, ulong messageId);
        public List<Category> GetAllCategories();
        public Category GetCategory(string name);
        public Category GetCategoryFromMessage(ulong id);
        public Role GetRole(string name);
        public bool TryGetRole(string name, out Role role);
        public Role GetRole(ulong id);
        public bool TryGetRole(ulong id, out Role role);
        public Role GetRoleFromEmote(string emote);
        public Channel GetChannel(ulong channelId);
        public void RemoveRole(ulong id);
    }
}
