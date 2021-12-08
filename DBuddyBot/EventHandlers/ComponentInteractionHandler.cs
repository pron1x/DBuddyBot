using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus;
using DSharpPlus.Entities;
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
            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            Category category = _database.GetCategoryFromMessage(e.Message.Id);
            Role role = category?.GetRoleFromComponentId(e.Id);
            if (role != null)
            {
                DiscordMember member = (DiscordMember)e.User;
                DiscordRole discordRole = e.Guild.GetRole(role.DiscordId);
                if (member.Roles.Contains(discordRole))
                {
                    try
                    {
                        await member.RevokeRoleAsync(discordRole);
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        await member.GrantRoleAsync(discordRole);
                    }
                    catch { }
                }
            }
        }
    }
}
