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
            ctx.Client.Logger.LogInformation($"{ctx.CommandName} executed by user {ctx.Member.DisplayName}.");
            await ctx.DeferAsync(true);
            DiscordWebhookBuilder response = new();
            Category category = Database.GetCategory(categoryName);
            if (category == null)
            {
                ctx.Client.Logger.LogDebug($"Category {categoryName} does not exist in the database.");
                await ctx.EditResponseAsync(response.WithContent($"No category {categoryName} exists. Can not add role to it."));
                return;
            }
            else if (category.GetRole(name.ToLower()) != null)
            {
                ctx.Client.Logger.LogDebug($"Category {category.Name} already contains role {name}.");
                await ctx.EditResponseAsync(response.WithContent($"Role {name} already exists in managed context, no need to add it again."));
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
                        ctx.Client.Logger.LogInformation($"Created role {discordRole.Name} on server {ctx.Guild.Name} ({ctx.Guild.Id}).");
                    }
                    catch (System.Exception e)
                    {
                        ctx.Client.Logger.LogError(e, $"Unable to create a new role in guild {ctx.Guild.Name} ({ctx.Guild.Id}). Database is unchanged.");
                        await ctx.EditResponseAsync(response.WithContent($"Role could not be created. Make sure necessary permissions are granted!"));
                        return;
                    }
                }
                Role role = Database.GetRole(name);
                if (role == null)
                {
                    ctx.Client.Logger.LogDebug($"Role {name} not in database, creating new object.");
                    role = new(-1, discordRole.Id, discordRole.Name.ToLower(), description);
                }
                if (category.AddRole(role))
                {
                    Database.AddRole(role, category.Id);
                    ctx.Client.Logger.LogInformation($"{ctx.Member.Username} added {discordRole.Name} to database.");
                    await ctx.EditResponseAsync(response.WithContent($"Added role {discordRole.Name} to database."));
                    if (category.Channel != null)
                    {
                        CommandUtilities.UpdateRoleMessage(ctx.Client, category, Database);
                        ctx.Client.Logger.LogDebug($"Updated message of category {category.Name}");
                    }
                    else
                    {
                        ctx.Client.Logger.LogInformation($"Category {category.Name} does not have an assigned channel.");
                        await ctx.EditResponseAsync(response.WithContent(response.Content + $"\nNo channel assigned to {category.Name}. Please assign one with `/category channel`."));
                    }
                }
                else
                {
                    ctx.Client.Logger.LogInformation($"Category {category.Name} has too many roles, {role.Name} has been created nonetheless.");
                    await ctx.EditResponseAsync(response.WithContent($"{category.Name} category already has 25 roles, the current maximum supported. " +
                        $"{discordRole.Name} has been created, but needs to be added to a different category."));
                }
            }
        }


        [SlashCommand("remove", "Removes a role from a category."), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task RemoveRole(InteractionContext ctx,
                                     [Autocomplete(typeof(CategoryAutocompleteProvider))][Option("category", "Category to remove from", true)] string categoryName,
                                     [Autocomplete(typeof(RoleCategoryAutocompleteProvider))][Option("role", "Role to remove")] string name)
        {
            ctx.Client.Logger.LogInformation($"{ctx.CommandName} executed by user {ctx.Member.DisplayName}.");
            await ctx.DeferAsync(true);
            DiscordWebhookBuilder response = new();
            Category category = Database.GetCategory(categoryName);
            if (category == null)
            {
                ctx.Client.Logger.LogInformation($"Category {categoryName} does not exist in the database.");
                await ctx.EditResponseAsync(response.WithContent($"No category {categoryName} exists, can not remove from it."));
            }
            Role role = category?.GetRole(name.ToLower());

            if (role == null)
            {
                ctx.Client.Logger.LogDebug($"Role {name} does not exist in the database.");
                await ctx.EditResponseAsync(response.WithContent($"No role {name} exists. Can not remove it."));
                return;
            }
            else
            {
                category.RemoveRole(role);
                Database.RemoveRole(category.Id, role.Id);
                ctx.Client.Logger.LogInformation($"{ctx.Member.Username} removed role {role.Name} from database.");
                await ctx.EditResponseAsync(response.WithContent($"Removed {role.Name} from {category.Name}."));
            }
            if (category.Message != null)
            {
                CommandUtilities.UpdateRoleMessage(ctx.Client, category, Database);
                ctx.Client.Logger.LogDebug($"Updated message of category {category.Name}");
            }
            else
            {
                ctx.Client.Logger.LogInformation($"Category {category.Name} does not have an assigned channel.");
                await ctx.EditResponseAsync(response.WithContent(response.Content + $"\nNo channel assigned to {category.Name}. Please assign one with `/category channel`."));
            }
        }

        [SlashCommand("description", "Update the description of a role"), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task UpdateRoleDescription(InteractionContext ctx,
                                                [Autocomplete(typeof(RoleAutocompleteProvider))][Option("role", "Role to update description of")] string roleName,
                                                [Option("description", "The new description")] string description)
        {
            ctx.Client.Logger.LogInformation($"{ctx.CommandName} executed by user {ctx.Member.DisplayName}.");
            await ctx.DeferAsync(true);
            Role role = Database.GetRole(roleName);
            if (role == null)
            {
                ctx.Client.Logger.LogInformation($"Role {roleName} does not exist in the database.");
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"No role {roleName} exists, can not change it's description."));
            }
            else
            {
                Database.UpdateRoleDescription(role.Id, description);
                ctx.Client.Logger.LogInformation($"Updated description of role {roleName} in the database.");
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Updated description of role {role.Name.ToTitleCase()}. Make sure to refresh the categories."));
            }
        }
    }

}
