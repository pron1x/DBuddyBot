using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBuddyBot.EventHandlers;

public static class ClientGuildEventHandler
{
    private static readonly IDatabaseService _database;

    static ClientGuildEventHandler()
    {
        _database = Bootstrapper.Database;
    }

    internal static Task SendRoleMessages(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        return Task.Run(async () =>
        {
            sender.Logger.LogInformation($"Guilds have completed downloading.");
            List<Category> categories = _database.GetAllCategories();
            foreach (Category category in categories)
            {
                if (category.Message == null && category.RoleCount > 0 && category.Channel != null)
                {
                    DiscordChannel channel = await sender.GetChannelAsync(category.Channel.DiscordId);
                    DiscordMessage message = await channel.SendMessageAsync(category.GetMessage(e.Guilds.Values.First()));
                    sender.Logger.LogInformation("Category message sent for category {category} on server {server}({id}) in channel {channel}.", category.Name, e.Guilds.Values.First().Name, e.Guilds.Values.First().Id, channel.Id);
                    _database.UpdateMessage(category.Id, message.Id);
                    sender.Logger.LogDebug("Message for category {category} updated.", category.Name);
                }
            }
        });
    }

    internal static Task UpdateRoleInDatabase(DiscordClient sender, GuildRoleUpdateEventArgs e)
    {
        return Task.Run(() =>
        {
            sender.Logger.LogInformation("Role {name}({id}) has been updated.", e.RoleBefore.Name, e.RoleBefore.Id);
            if (e.RoleBefore.Name != e.RoleAfter.Name && _database.TryGetRole(e.RoleBefore.Id, out _))
            {
                _database.UpdateRoleName(e.RoleAfter.Id, e.RoleAfter.Name);
                sender.Logger.LogInformation("Role name has been updated from {before} to {after}.", e.RoleBefore.Name, e.RoleAfter.Name);
            }
        });
    }

    internal static Task DeleteChannelFromDatabase(DiscordClient sender, ChannelDeleteEventArgs e)
    {
        return Task.Run(() =>
        {
            sender.Logger.LogInformation("Channel {channel}({channelId}) has been deleted from guild {guild}({guildId}).", e.Channel.Name, e.Channel.Id, e.Guild.Name, e.Guild.Id);
            Channel channel = _database.GetChannel(e.Channel.Id);
            if (channel != null)
            {
                _database.RemoveChannel(channel.Id);
                sender.Logger.LogInformation("Channel {channel} has been removed from the database.", e.Channel.Id);
            }
        });
    }

    internal static Task DeleteRoleFromDatabase(DiscordClient sender, GuildRoleDeleteEventArgs e)
    {
        return Task.Run(() =>
        {
            sender.Logger.LogInformation("Role {name}({id}) has been deleted from guild {guild}({id}).", e.Role.Name, e.Role.Id, e.Guild.Name, e.Guild.Id);
            if (_database.TryGetRole(e.Role.Id, out Role role))
            {
                _database.RemoveRole(role.Id);
                sender.Logger.LogInformation("Channel {name}({id}) has been removed from the database.", e.Role.Name, e.Role.Id);
            }
        });
    }
}
