using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBuddyBot.EventHandlers
{
    public static class ClientReadyEventHandler
    {
        private static IDatabaseService _database;

        static ClientReadyEventHandler()
        {
            _database = Bootstrapper.Database;
        }

        //TODO: Add created messages to database
        //TODO: Only send message if category contains roles. Otherwise send when roles get added.
        public static async Task SendRoleMessages(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            List<Category> categories = _database.GetAllCategories();
            foreach (Category category in categories)
            {
                if (category.Channel.Messages.Count == 0 && category.Roles.Count > 0)
                {
                    DiscordChannel channel = await sender.GetChannelAsync(category.Channel.Id);
                    DiscordMessage message = await channel.SendMessageAsync(category.GetEmbed(sender));
                    foreach (Role role in category.Roles)
                    {
                        DiscordEmoji emoji = DiscordEmoji.FromGuildEmote(sender, role.EmoteId);
                        await message.CreateReactionAsync(emoji);
                    }

                }
            }
        }
    }
}
