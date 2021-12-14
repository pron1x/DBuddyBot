using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;

namespace DBuddyBot.EventHandlers
{
    public class ClientMessageEventHandler
    {
        private static readonly IDatabaseService _database;

        static ClientMessageEventHandler()
        {
            _database = Bootstrapper.Database;
        }

        internal static Task RemoveRoleMessage(DiscordClient sender, MessageDeleteEventArgs e)
        {
            return Task.Run(() =>
            {
                Category category;
                if ((category = _database.GetCategoryFromMessage(e.Message.Id)) != null)
                {
                    _database.UpdateMessage(category.Id, 0);
                }
            });
        }
    }
}
