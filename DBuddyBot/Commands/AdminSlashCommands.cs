using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace DBuddyBot.Commands
{

    [SlashCommandGroup("role", "Manage roles.")]
    public class RoleCommandsGroupContainer : ApplicationCommandModule
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
                                     [Autocomplete(typeof(RoleAutocompleteProvider))][Option("role", "Role to remove")] string name)
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
    }


    [SlashCommandGroup("category", "Manage categories.")]
    public class CategoryCommandsGroupContainer : ApplicationCommandModule
    {
        #region properties
        public IDatabaseService Database { private get; set; }
        #endregion properties


        [SlashCommand("add", "Adds a new category for roles."), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task AddCategory(InteractionContext ctx,
                                          [Option("category", "Category to add")] string name,
                                          [Option("channel", "Channel for the category message")] DiscordChannel channel,
                                          [Option("color", "Embed color of the category in hex")] string colorString = "#000000",
                                          [Option("description", "Description for the category")] string description = "")
        {
            await ctx.DeferAsync(true);
            Category category = Database.GetCategory(name);
            if (category == null)
            {
                DiscordColor color;
                try
                {
                    color = new(colorString);
                }
                catch
                {
                    color = new("#000000");
                }
                int categoryId = Database.AddCategory(name, description, channel.Id, color.Value);
                if (categoryId == -1)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Category {name} already exists, or something went wrong."));
                    return;
                }
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added {name}"));
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Category {name} already exists, or something went wrong."));
            }
        }

        [SlashCommand("remove", "Removes a category."), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task RemoveCategory(InteractionContext ctx, [Autocomplete(typeof(CategoryAutocompleteProvider))][Option("category", "Category to remove", true)] string name)
        {
            await ctx.DeferAsync(true);
            Category category = Database.GetCategory(name);
            if (category == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Category {name} does not exist, cannot remove it."));
                return;
            }
            else
            {
                try
                {
                    DiscordChannel channel = await ctx.Client.GetChannelAsync(category.Channel.DiscordId);
                    if (category.Message != null)
                    {
                        DiscordMessage message = await channel.GetMessageAsync(category.Message.Id);
                        await message.DeleteAsync($"Category {name} has been removed by {ctx.Member.Nickname}.");
                    }
                    Database.RemoveCategory(category);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Removed {category.Name}."));
                }
                catch (NotFoundException e)
                {
                    ctx.Client.Logger.LogError(e, $"A channel (id: {category.Channel.DiscordId}) or message (id: {category.Message.Id}) in the RemoveCategory function could not be found! Database might hold faulty records.");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"The channel or message could not be found. Category has not been removed."));
                }
                catch (UnauthorizedException e)
                {
                    ctx.Client.Logger.LogError(e, $"Bot is not authorized to resolve, send or modify messages in channel with id {category.Channel.DiscordId}.");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"No permissions to resolve, send or modify messages. Category has not been removed."));
                }
                catch (System.Exception e)
                {
                    ctx.Client.Logger.LogError(e, "Exception occured in UpdateRoleMessage.");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Something went wrong. Category has not been removed."));
                }
            }
        }

        [SlashCommand("refresh", "Refreshes a category message."), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task RefreshCategory(InteractionContext ctx, [Autocomplete(typeof(CategoryAutocompleteProvider))][Option("category", "Category to remove", true)] string name)
        {
            await ctx.DeferAsync(true);
            Category category = Database.GetCategory(name);
            if (category != null)
            {
                CommandUtilities.UpdateRoleMessage(ctx.Client, category, Database);
            }
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Message refreshed."));
        }
    }
}

