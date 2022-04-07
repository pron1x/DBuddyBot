using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DBuddyBot.EventHandlers;

public class ComponentInteractionHandler
{
    private static readonly IDatabaseService _database;

    static ComponentInteractionHandler()
    {
        _database = Bootstrapper.Database;
    }

    public static Task HandleComponentInteraction(DiscordClient sender, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e)
    {
        return Task.Run(async () =>
        {
            sender.Logger.LogInformation("Received ComponentInteractionEvent for component with id {id} on guild {guild}({id}). Invoked by user {user}.", e.Id, e.Guild.Name, e.Guild.Id, e.User.Username);
            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true }); ;
            string description = string.Empty;
            DiscordColor color = DiscordColor.DarkGreen;
            Category category = _database.GetCategoryFromMessage(e.Message.Id);
            Role role = category?.GetRoleFromComponentId(e.Id);
            if (role != null)
            {
                DiscordMember member = (DiscordMember)e.User;
                DiscordRole discordRole = e.Guild.GetRole(role.DiscordId);
                try
                {
                    if (member.Roles.Contains(discordRole))
                    {
                        await member.RevokeRoleAsync(discordRole);
                        sender.Logger.LogInformation("Revoked role {role}({id}) from user {user} on guild {guild}({id}).", discordRole.Name, discordRole.Id, member.Nickname, e.Guild.Name, e.Guild.Id);
                        description = $"Removed {discordRole.Mention}";
                    }
                    else
                    {
                        await member.GrantRoleAsync(discordRole);
                        sender.Logger.LogInformation("Granted role {role}({id}) to user {user} on guild {guild}({id}).", discordRole.Name, discordRole.Id, member.Nickname, e.Guild.Name, e.Guild.Id);
                        description = $"Added {discordRole.Mention}";
                    }
                }
                catch (UnauthorizedException exception)
                {
                    sender.Logger.LogError(exception, "Not allowed to grant/revoke role {role}({id}) on guild {guild}({id}).", discordRole.Name, discordRole.Id, e.Guild.Name, e.Guild.Id);
                    color = DiscordColor.DarkRed;
                    description = $"Can't manage the role {discordRole.Mention}. Please let the admins know!";
                }
                catch (NotFoundException exception)
                {
                    sender.Logger.LogError(exception, "Role {role}({id}) was not found on guild {guild}({id}).", discordRole.Name, discordRole.Id, e.Guild.Name, e.Guild.Id);
                    color = DiscordColor.DarkRed;
                    description = $"Role {discordRole.Name} was not found. Please let the admins know!";
                }
                catch (Exception exception)
                {
                    sender.Logger.LogError(exception, "An error occured trying to grant/revoke a role.");
                    color = DiscordColor.DarkRed;
                    description = $"Something went wrong. Please let the admins know!";
                }
                finally
                {
                    DiscordEmbedBuilder embed = new();
                    embed.Color = color;
                    embed.Description = description;
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder() { Content = string.Empty }.AddEmbed(embed.Build()));
                }
            }
            sender.Logger.LogWarning($"No role could be found for the interaction id. Database might hold faulty records.");
        });
    }
}
