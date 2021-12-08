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
    public class RoleCommandsGroupContainer : ApplicationCommandModule
    {
        #region properties
        public IDatabaseService Database { private get; set; }
        #endregion properties


        [SlashCommand("add", "Adds a role to a category."), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task AddRole(InteractionContext ctx,
                                  [Autocomplete(typeof(CategoryAutocompleteProvider))][Option("category", "Category to add to", true)] string categoryName,
                                  [Option("role", "Role to add")] string name,
                                  [Option("description", "Description for the role")] string description = "")
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
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
                    discordRole = await ctx.Guild.CreateRoleAsync(name.ToTitleCase(), DSharpPlus.Permissions.None, DiscordColor.Brown, mentionable: true);
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
                    UpdateRoleMessage(ctx.Client, category);
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
                                     [Option("role", "Role to remove")] string name)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
            Category category = Database.GetCategory(categoryName);
            if (category == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"No category {categoryName} exists, cannot remove from it."));
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
            UpdateRoleMessage(ctx.Client, category);
        }

        private async void UpdateRoleMessage(DSharpPlus.DiscordClient client, Category category)
        {
            DiscordChannel channel = await client.GetChannelAsync(category.Channel.DiscordId);
            DiscordMessageBuilder messageBuilder = category.GetMessage(client.Guilds.Values.First());
            if (category.Message == null)
            {
                DiscordMessage message = await messageBuilder.SendAsync(channel);
                Database.UpdateMessage(category.Id, message.Id);
            }
            else
            {
                DiscordMessage message = await channel.GetMessageAsync(category.Message.Id);
                if (messageBuilder == null)
                {
                    await message.DeleteAsync();
                    Database.UpdateMessage(category.Id, 0);
                    return;
                }
                else
                {
                    await message.ModifyAsync(messageBuilder);
                }
            }
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
                                          [Option("color", "Embed color of the category in hex")] string color = "#000000",
                                          [Option("description", "Description for the category")] string description = "")
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
            Category category = Database.GetCategory(name);
            if (category == null)
            {
                int categoryId = Database.AddCategory(name, description, channel.Id, new DiscordColor(color).Value);
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
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
            Category category = Database.GetCategory(name);
            if (category == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Category {name} does not exist, cannot remove it."));
                return;
            }
            else
            {
                Database.RemoveCategory(category);
                DiscordChannel channel = await ctx.Client.GetChannelAsync(category.Channel.DiscordId);
                if (category.Message != null)
                {
                    DiscordMessage message = await channel.GetMessageAsync(category.Message.Id);
                    await message.DeleteAsync($"Category {name} has been removed by {ctx.Member.Nickname}.");
                }
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Removed {category.Name}."));
            }
        }
    }
}

