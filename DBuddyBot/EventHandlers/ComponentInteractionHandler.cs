using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DBuddyBot.EventHandlers
{
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
                            description = $"Removed {discordRole.Mention}";
                        }
                        else
                        {
                            await member.GrantRoleAsync(discordRole);
                            description = $"Added {discordRole.Mention}";
                        }
                    }
                    catch (UnauthorizedException exception)
                    {
                        Log.Logger.Error(exception, $"Not allowed to grant/revoke role {discordRole.Name} ({discordRole.Id}) on guild {e.Guild.Name} ({e.Guild.Id}).");
                        color = DiscordColor.DarkRed;
                        description = $"Can't manage the role {discordRole.Mention}. Please let the admins know!";
                    }
                    catch (NotFoundException exception)
                    {
                        Log.Logger.Error(exception, $"Role {discordRole.Name} ({discordRole.Id}) was not found on guild {e.Guild.Name} ({e.Guild.Id}).");
                        color = DiscordColor.DarkRed;
                        description = $"Role {discordRole.Name} was not found. Please let the admins know!";
                    }
                    catch (Exception exception)
                    {
                        Log.Logger.Error(exception, "An error occured trying to grant/revoke a role.");
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
            });
        }
    }
}
