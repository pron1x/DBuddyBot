using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus;
using DSharpPlus.Entities;
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
            if (_database.GetChannel(e.Channel.Id) != null)
            {
                Role role = _database.GetRoleFromEmote(e.Emoji.Id);
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
