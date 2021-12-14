using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
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

        internal static async Task SendRoleMessages(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            List<Category> categories = _database.GetAllCategories();
            foreach (Category category in categories)
            {
                if (category.Message == null && category.RoleCount > 0)
                {
                    DiscordChannel channel = await sender.GetChannelAsync(category.Channel.DiscordId);
                    DiscordMessage message = await channel.SendMessageAsync(category.GetMessage(e.Guilds.Values.First()));
                    _database.UpdateMessage(category.Id, message.Id);
                }
            }
        }

        internal static Task UpdateRoleInDatabase(DiscordClient sender, GuildRoleUpdateEventArgs e)
        {
            return Task.Run(() =>
            {
                if (e.RoleBefore.Name != e.RoleAfter.Name && _database.TryGetRole(e.RoleBefore.Id, out _))
                {
                    _database.UpdateRoleName(e.RoleAfter.Id, e.RoleAfter.Name);
                }
            });
            
        }
    }
}
