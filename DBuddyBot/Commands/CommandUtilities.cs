using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus.Entities;
using System.Linq;

namespace DBuddyBot.Commands
{
    internal static class CommandUtilities
    {
        internal static async void UpdateRoleMessage(DSharpPlus.DiscordClient client, Category category, IDatabaseService database)
        {
            DiscordChannel channel = await client.GetChannelAsync(category.Channel.DiscordId);
            DiscordMessageBuilder messageBuilder = category.GetMessage(client.Guilds.Values.First());
            if (category.Message == null)
            {
                DiscordMessage message = await messageBuilder.SendAsync(channel);
                database.UpdateMessage(category.Id, message.Id);
            }
            else
            {
                DiscordMessage message = await channel.GetMessageAsync(category.Message.Id);
                if (messageBuilder == null)
                {
                    await message.DeleteAsync();
                    database.UpdateMessage(category.Id, 0);
                    return;
                }
                else
                {
                    await message.ModifyAsync(messageBuilder);
                }
            }
        }
    }
}
