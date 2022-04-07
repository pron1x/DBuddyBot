using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace DBuddyBot.Commands;

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
        ctx.Client.Logger.LogInformation("{command} executed by user {user}.", ctx.CommandName, ctx.Member.DisplayName);
        await ctx.DeferAsync(true);
        DiscordWebhookBuilder response = new();
        Category category = Database.GetCategory(categoryName);
        if (category == null)
        {
            ctx.Client.Logger.LogDebug("Category {category} does not exist in the database.", categoryName);
            await ctx.EditResponseAsync(response.WithContent($"No category {categoryName} exists. Can not add role to it."));
            return;
        }
        else if (category.GetRole(name.ToLower()) != null)
        {
            ctx.Client.Logger.LogDebug("Category {category} already contains role {role}.", category.Name, name);
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
                    ctx.Client.Logger.LogInformation("Created role {role} on server {name} ({id}).", discordRole.Name, ctx.Guild.Name, ctx.Guild.Id);
                }
                catch (System.Exception e)
                {
                    ctx.Client.Logger.LogError(e, "Unable to create a new role in guild {name} ({id}). Database is unchanged.", ctx.Guild.Name, ctx.Guild.Id);
                    await ctx.EditResponseAsync(response.WithContent($"Role could not be created. Make sure necessary permissions are granted!"));
                    return;
                }
            }
            Role role = Database.GetRole(name);
            if (role == null)
            {
                ctx.Client.Logger.LogDebug("Role {name} not in database, creating new object.", name);
                role = new(-1, discordRole.Id, discordRole.Name.ToLower(), description);
            }
            if (category.AddRole(role))
            {
                Database.AddRole(role, category.Id);
                ctx.Client.Logger.LogInformation("{user} added {role} to database.", ctx.Member.Username, discordRole.Name);
                await ctx.EditResponseAsync(response.WithContent($"Added role {discordRole.Name} to database."));
                if (category.Channel != null)
                {
                    CommandUtilities.UpdateRoleMessage(ctx.Client, category, Database);
                    ctx.Client.Logger.LogDebug("Updated message of category {category}", category.Name);
                }
                else
                {
                    ctx.Client.Logger.LogInformation("Category {category} does not have an assigned channel.", category.Name);
                    await ctx.EditResponseAsync(response.WithContent(response.Content + $"\nNo channel assigned to {category.Name}. Please assign one with `/category channel`."));
                }
            }
            else
            {
                ctx.Client.Logger.LogInformation("Category {category} has too many roles, {role} has been created nonetheless.", category.Name, role.Name);
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
        ctx.Client.Logger.LogInformation("{command} executed by user {user}.", ctx.CommandName, ctx.Member.DisplayName);
        await ctx.DeferAsync(true);
        DiscordWebhookBuilder response = new();
        Category category = Database.GetCategory(categoryName);
        if (category == null)
        {
            ctx.Client.Logger.LogInformation("Category {category} does not exist in the database.", categoryName);
            await ctx.EditResponseAsync(response.WithContent($"No category {categoryName} exists, can not remove from it."));
        }
        Role role = category?.GetRole(name.ToLower());

        if (role == null)
        {
            ctx.Client.Logger.LogDebug("Role {name} does not exist in the database.", name);
            await ctx.EditResponseAsync(response.WithContent($"No role {name} exists. Can not remove it."));
            return;
        }
        else
        {
            category.RemoveRole(role);
            Database.RemoveRole(category.Id, role.Id);
            ctx.Client.Logger.LogInformation("{user} removed role {role} from database.", ctx.Member.Username, role.Name);
            await ctx.EditResponseAsync(response.WithContent($"Removed {role.Name} from {category.Name}."));
        }
        if (category.Message != null)
        {
            CommandUtilities.UpdateRoleMessage(ctx.Client, category, Database);
            ctx.Client.Logger.LogDebug("Updated message of category {category}", category.Name);
        }
        else
        {
            ctx.Client.Logger.LogInformation("Category {category} does not have an assigned channel.", category.Name);
            await ctx.EditResponseAsync(response.WithContent(response.Content + $"\nNo channel assigned to {category.Name}. Please assign one with `/category channel`."));
        }
    }

    [SlashCommand("description", "Update the description of a role"), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
    public async Task UpdateRoleDescription(InteractionContext ctx,
                                            [Autocomplete(typeof(RoleAutocompleteProvider))][Option("role", "Role to update description of")] string roleName,
                                            [Option("description", "The new description")] string description)
    {
        ctx.Client.Logger.LogInformation("{command} executed by user {user}.", ctx.CommandName, ctx.Member.DisplayName);
        await ctx.DeferAsync(true);
        Role role = Database.GetRole(roleName);
        if (role == null)
        {
            ctx.Client.Logger.LogInformation("Role {role} does not exist in the database.", roleName);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"No role {roleName} exists, can not change it's description."));
        }
        else
        {
            Database.UpdateRoleDescription(role.Id, description);
            ctx.Client.Logger.LogInformation("Updated description of role {role} in the database.", roleName);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Updated description of role {role.Name.ToTitleCase()}. Make sure to refresh the categories."));
        }
    }
}

