using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace DBuddyBot.EventHandlers
{
    public static class MessageReactionHandler
    {
        private static IDatabaseService _database;

        static MessageReactionHandler()
        {
            _database = Bootstrapper.Database;
        }

        public static Task BaseMessageReactionEventHandler(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            if(e.User == sender.CurrentUser)
            {
                return Task.CompletedTask;
            }
            Category category = _database.GetCategoryFromMessage(e.Message.Id);
            if (category != null)
            {
                Role role = category.Roles.FirstOrDefault(r => r.Emoji.Name == string.Empty ? r.Emoji.Id == e.Emoji.Id : r.Emoji.Name == e.Emoji.GetDiscordName());
                if (role == null)
                {
                    e.Message.DeleteReactionAsync(e.Emoji, e.User, "Emoji not mapped to a role");
                }
                else
                {
                    DiscordMember member = (DiscordMember)e.User;
                    member.GrantRoleAsync(e.Guild.GetRole(role.Id));
                }
            }
            return Task.CompletedTask;
        }
    }
}
