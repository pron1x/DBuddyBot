using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Serilog;
using System;
using System.Linq;

namespace DBuddyBot.Commands
{
    internal static class CommandUtilities
    {
        internal static async void UpdateRoleMessage(DSharpPlus.DiscordClient client, Category category, IDatabaseService database)
        {
            try
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
                    }
                    else
                    {
                        await message.ModifyAsync(messageBuilder);
                    }
                }
            }
            catch (NotFoundException e)
            {
                Log.Logger.Error(e, $"A channel (id: {category.Channel.DiscordId}) or message (id: {category.Message.Id}) in the UpdateRoleMessage function could not be found! Database might hold faulty records.");
            }
            catch (UnauthorizedException e)
            {
                Log.Logger.Error(e, $"Bot is not authorized to resolve, send or modify messages in channel with id {category.Channel.DiscordId}.");
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Exception occured in UpdateRoleMessage.");
            }

        }
    }
}
