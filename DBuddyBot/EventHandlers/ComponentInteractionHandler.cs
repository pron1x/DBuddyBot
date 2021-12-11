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

        public static async Task HandleComponentInteraction(DiscordClient sender, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e)
        {
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
                    }
                    else
                    {
                        await member.GrantRoleAsync(discordRole);
                    }
                }
                catch (UnauthorizedException exception)
                {
                    Log.Logger.Error(exception, $"Not allowed to grant/revoke role {discordRole.Name} ({discordRole.Id}) on guild {e.Guild.Name} ({e.Guild.Id}).");
                    return;
                }
                catch (NotFoundException exception)
                {
                    Log.Logger.Error(exception, $"Role {discordRole.Name} ({discordRole.Id}) was not found on guild {e.Guild.Name} ({e.Guild.Id}).");
                    return;
                }
                catch (Exception exception)
                {
                    Log.Logger.Error(exception, "An error occured trying to grant/revoke a role.");
                    return;
                }
            }
            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        }
    }
}
