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
        private static IDatabaseService _database;

        static ClientGuildEventHandler()
        {
            _database = Bootstrapper.Database;
        }

        //TODO: Add created messages to database
        //TODO: Only send message if category contains roles. Otherwise send when roles get added.
        public static async Task SendRoleMessages(DiscordClient sender, DSharpPlus.EventArgs.GuildDownloadCompletedEventArgs e)
        {
            List<Category> categories = _database.GetAllCategories();
            foreach (Category category in categories)
            {
                if (category.Message == null && category.Roles.Count > 0)
                {
                    DiscordChannel channel = await sender.GetChannelAsync(category.Channel.Id);
                    DiscordMessage message = await channel.SendMessageAsync(category.GetEmbed(sender));
                    foreach (Role role in category.Roles)
                    {
                        if (DiscordEmoji.TryFromName(sender, role.Emoji.Name, out DiscordEmoji emoji))
                        {
                            await message.CreateReactionAsync(emoji);
                        }
                        else if (DiscordEmoji.TryFromGuildEmote(sender, role.Emoji.Id, out emoji))
                        {
                            await message.CreateReactionAsync(emoji);
                        }
                    }

                }
            }
        }
    }
}
