using DBuddyBot.Models;
using System.Collections.Generic;

namespace DBuddyBot.Data
{
    /// <summary>
    /// Interface for database access.
    /// </summary>
    public interface IDatabaseService
    {
        public int AddCategory(string name, string description, ulong channelId, int color);
        public void AddRole(Role role, int categoryId);
        public int AddChannel(ulong channelId);
        public void UpdateMessage(int categorId, ulong messageId);
        public List<string> GetAllCategoryNames();
        public List<Category> GetAllCategories();
        public Category GetCategory(string name);
        public Category GetCategoryFromMessage(ulong id);
        public Role GetRole(string name);
        public bool TryGetRole(string name, out Role role);
        public Role GetRole(ulong id);
        public bool TryGetRole(ulong id, out Role role);
        public Channel GetChannel(ulong channelId);
        public void RemoveRole(int categoryId, int roleId);
        public void RemoveCategory(Category category);
    }
}
