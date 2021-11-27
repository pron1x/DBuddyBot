using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBuddyBot.EventHandlers
{
    public static class ClientGuildEventHandler
    {
        private static readonly IDatabaseService _database;

        static ClientGuildEventHandler()
        {
            _database = Bootstrapper.Database;
        }

        public static async Task SendRoleMessages(DiscordClient sender, DSharpPlus.EventArgs.GuildDownloadCompletedEventArgs e)
        {
            List<Category> categories = _database.GetAllCategories();
            foreach (Category category in categories)
            {
                if (category.Message == null && category.RoleCount > 0)
                {
                    DiscordChannel channel = await sender.GetChannelAsync(category.Channel.Id);
                    DiscordMessage message = await channel.SendMessageAsync(category.GetMessage(sender));
                    _database.UpdateMessage(category.Id, message.Id);
                }
            }
        }
    }
}
