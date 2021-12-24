using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace DBuddyBot.Commands
{
    [SlashCommandGroup("role", "Manage roles.")]
    public class RoleSlashCommands : ApplicationCommandModule
    {
        #region properties
        public IDatabaseService Database { private get; set; }
        #endregion properties


        [SlashCommand("add", "Adds a role to a category."), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task AddRole(InteractionContext ctx,
                                  [Autocomplete(typeof(CategoryAutocompleteProvider))][Option("category", "Category to add to", true)] string categoryName,
                                  [Autocomplete(typeof(DiscordRoleAutoCompleteProvider))][Option("role", "Role to add")] string name,
                                  [Option("description", "Description for the role")] string description = "")
        {
            await ctx.DeferAsync(true);
            Category category = Database.GetCategory(categoryName);
            if (category == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"No category {categoryName} exists. Can not add role to it."));
                return;
            }
            else if (category.GetRole(name.ToLower()) != null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Role {name} already exists in managed context, no need to add it again."));
                return;
            }
            else
            {
                DiscordRole discordRole = ctx.Guild.Roles.FirstOrDefault(role => role.Value.Name.ToLower() == name.ToLower()).Value;
                if (discordRole == null)
                {
                    try
                    {
                        discordRole = await ctx.Guild.CreateRoleAsync(name.ToTitleCase(), DSharpPlus.Permissions.None, DiscordColor.Brown, mentionable: true);
                    }
                    catch (System.Exception e)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Role could not be created. Make sure necessary permissions are granted!"));
                        ctx.Client.Logger.LogError(e, $"Unable to create a new role in guild {ctx.Guild.Name} ({ctx.Guild.Id}). Database is unchanged.");
                        return;
                    }
                }
                Role role = Database.GetRole(name);
                if (role == null)
                {
                    role = new(-1, discordRole.Id, discordRole.Name.ToLower(), description);
                }
                if (category.AddRole(role))
                {
                    Database.AddRole(role, category.Id);
                    ctx.Client.Logger.LogInformation($"{ctx.Member.Username} added {discordRole.Name} to database.");
                    CommandUtilities.UpdateRoleMessage(ctx.Client, category, Database);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added role {discordRole.Name} to database."));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{category.Name} category already has 25 roles, the current maximum supported. " +
                        $"{discordRole.Name} has been created, but needs to be added to a different category."));
                }
            }
        }


        [SlashCommand("remove", "Removes a role from a category."), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task RemoveRole(InteractionContext ctx,
                                     [Autocomplete(typeof(CategoryAutocompleteProvider))][Option("category", "Category to remove from", true)] string categoryName,
                                     [Autocomplete(typeof(RoleCategoryAutocompleteProvider))][Option("role", "Role to remove")] string name)
        {
            await ctx.DeferAsync(true);
            Category category = Database.GetCategory(categoryName);
            if (category == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"No category {categoryName} exists, can not remove from it."));
            }
            Role role = category?.GetRole(name.ToLower());

            if (role == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"No role {name} exists. Can not remove it."));
                return;
            }
            else
            {
                category.RemoveRole(role);
                Database.RemoveRole(category.Id, role.Id);
                ctx.Client.Logger.LogInformation($"{ctx.Member.Username} removed role {role.Name} from database.");
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Removed {role.Name} from {category.Name}."));
            }
            CommandUtilities.UpdateRoleMessage(ctx.Client, category, Database);
        }

        [SlashCommand("description", "Update the description of a role"), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task UpdateRoleDescription(InteractionContext ctx,
                                                [Autocomplete(typeof(RoleAutocompleteProvider))][Option("role", "Role to update description of")] string roleName,
                                                [Option("description", "The new description")] string description)
        {
            await ctx.DeferAsync(true);
            Role role = Database.GetRole(roleName);
            if (role == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"No role {roleName} exists, can not change it's description."));
            }
            else
            {
                Database.UpdateRoleDescription(role.Id, description);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Updated description of role {role.Name.ToTitleCase()}. Make sure to refresh the categories."));
            }
        }
    }

}
