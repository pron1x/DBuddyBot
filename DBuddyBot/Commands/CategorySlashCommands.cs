using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DBuddyBot.Commands;

[SlashCommandGroup("category", "Manage categories.")]
public class CategorySlashCommands : ApplicationCommandModule
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
        ctx.Client.Logger.LogInformation($"{ctx.CommandName} executed by user {ctx.Member.DisplayName}.");
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
                ctx.Client.Logger.LogInformation($"Colorcode {colorString} was not converted to a color, using default color (#000000).");
                color = new("#000000");
            }
            int categoryId = Database.AddCategory(name, description, channel.Id, color.Value);
            if (categoryId == -1)
            {
                ctx.Client.Logger.LogInformation($"Category {name} already exists or something went wrong.");
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Category {name} already exists, or something went wrong."));
                return;
            }
            ctx.Client.Logger.LogInformation($"Created category {name}.");
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added {name}."));
        }
        else
        {
            ctx.Client.Logger.LogInformation($"Category {name} already exists.");
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Category {name} already exists, or something went wrong."));
        }
    }

    [SlashCommand("remove", "Removes a category."), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
    public async Task RemoveCategory(InteractionContext ctx, [Autocomplete(typeof(CategoryAutocompleteProvider))][Option("category", "Category to remove", true)] string name)
    {
        ctx.Client.Logger.LogInformation($"{ctx.CommandName} executed by user {ctx.Member.DisplayName}.");
        await ctx.DeferAsync(true);
        Category category = Database.GetCategory(name);
        if (category == null)
        {
            ctx.Client.Logger.LogDebug($"Category {name} does not exist.");
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
                    ctx.Client.Logger.LogDebug($"Category message with Id {message.Id} has been removed.");
                }
                Database.RemoveCategory(category);
                ctx.Client.Logger.LogInformation($"Category {category.Name} successfully removed.");
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

    [SlashCommand("description", "Updates the description of a category."), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
    public async Task UpdateCategoryDescription(InteractionContext ctx,
                                                [Autocomplete(typeof(CategoryAutocompleteProvider))][Option("category", "Category to update description of.", true)] string name,
                                                [Option("description", "The new description")] string description)
    {
        ctx.Client.Logger.LogInformation($"{ctx.CommandName} executed by user {ctx.Member.DisplayName}.");
        await ctx.DeferAsync(true);
        DiscordWebhookBuilder response = new();
        Category category = Database.GetCategory(name);
        if (category == null)
        {
            ctx.Client.Logger.LogDebug($"Category {name} does not exist.");
            await ctx.EditResponseAsync(response.WithContent($"Category {name} does not exist, can not update it's description."));
            return;
        }
        category = Database.UpdateCategoryDescription(category, description);
        ctx.Client.Logger.LogInformation($"Description of category {name} has been updated to '{description}'.");
        await ctx.EditResponseAsync(response.WithContent($"Category description updated."));
        if (category.Channel != null)
        {
            CommandUtilities.UpdateRoleMessage(ctx.Client, category, Database);
        }
        else
        {
            ctx.Client.Logger.LogInformation($"Category {category.Name} does not have an assigned channel.");
            await ctx.EditResponseAsync(response.WithContent(response.Content + $"\nNo channel assigned to {category.Name}. Please assign one with `/category channel`."));
        }
    }

    [SlashCommand("color", "Updates the color of a category embed."), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
    public async Task UpdateCategoryColor(InteractionContext ctx, [Autocomplete(typeof(CategoryAutocompleteProvider))][Option("category", "Category to change the color of.")] string name,
        [Option("color", "Embed color of the category in hex")] string color)
    {
        ctx.Client.Logger.LogInformation($"{ctx.CommandName} executed by user {ctx.Member.DisplayName}.");
        await ctx.DeferAsync(true);
        DiscordWebhookBuilder response = new();
        Category category = Database.GetCategory(name);
        if (category == null)
        {
            ctx.Client.Logger.LogDebug($"Category {name} does not exist.");
            await ctx.EditResponseAsync(response.WithContent($"Category {name} does not exist, can not update it's embed color."));
            return;
        }
        DiscordColor discordColor;
        try
        {
            discordColor = new(color);
        }
        catch
        {
            ctx.Client.Logger.LogInformation($"{color} is not a valid hex color.");
            await ctx.EditResponseAsync(response.WithContent($"{color} is not a valid hex color."));
            return;
        }
        category = Database.UpdateCategoryColor(category, discordColor.Value);
        ctx.Client.Logger.LogInformation($"Colour of category {category.Name} has been updated to {color}.");
        await ctx.EditResponseAsync(response.WithContent($"Category color updated."));
        if (category.Channel != null)
        {
            CommandUtilities.UpdateRoleMessage(ctx.Client, category, Database);
        }
        else
        {
            ctx.Client.Logger.LogInformation($"Category {category.Name} does not have an assigned channel.");
            await ctx.EditResponseAsync(response.WithContent(response.Content + $"\nNo channel assigned to {category.Name}. Please assign one with `/category channel`."));
        }
    }

    [SlashCommand("channel", "Updates the channel of the category."), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
    public async Task UpdateCategoryChannel(InteractionContext ctx, [Autocomplete(typeof(CategoryAutocompleteProvider))][Option("category", "Category to change the channel of.")] string name,
        [Option("channel", "New channel of the category.")] DiscordChannel channel)
    {
        ctx.Client.Logger.LogInformation($"{ctx.CommandName} executed by user {ctx.Member.DisplayName}.");
        await ctx.DeferAsync(true);
        Category category = Database.GetCategory(name);
        if (category == null)
        {
            ctx.Client.Logger.LogDebug($"Category {name} does not exist.");
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Category {name} does not exist, can not update it's channel."));
            return;
        }
        if (category.Channel?.DiscordId != channel.Id)
        {
            if (category.Channel != null && category.Message != null)
            {
                DiscordChannel oldChannel = await ctx.Client.GetChannelAsync(category.Channel.DiscordId);
                await oldChannel.GetMessageAsync(category.Message.Id).Result.DeleteAsync();
                ctx.Client.Logger.LogDebug($"Old message with id {category.Message.Id} has been deleted.");
            }
            Database.UpdateMessage(category.Id, 0);
            ctx.Client.Logger.LogDebug($"Message of category {category.Name} has been updated.");
            category = Database.UpdateCategoryChannel(category, channel.Id);
            ctx.Client.Logger.LogDebug($"Channel of category {category.Name} has been updated.");
        }
        CommandUtilities.UpdateRoleMessage(ctx.Client, category, Database);
        ctx.Client.Logger.LogInformation($"Channel of category {category.Name} has been updated.");
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Category channel updated."));
    }


    [SlashCommand("refresh", "Refreshes a category message."), SlashRequirePermissions(DSharpPlus.Permissions.ManageRoles)]
    public async Task RefreshCategory(InteractionContext ctx, [Autocomplete(typeof(CategoryAutocompleteProvider))][Option("category", "Category to remove", true)] string name)
    {
        ctx.Client.Logger.LogInformation($"{ctx.CommandName} executed by user {ctx.Member.DisplayName}.");
        await ctx.DeferAsync(true);
        Category category = Database.GetCategory(name);
        if (category != null)
        {
            if (category.Channel != null)
            {
                CommandUtilities.UpdateRoleMessage(ctx.Client, category, Database);
                ctx.Client.Logger.LogInformation($"Message of category {category.Name} has been refreshed.");
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Message refreshed."));
            }
            else
            {
                ctx.Client.Logger.LogInformation($"Category {category.Name} does not have an assigned channel.");
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"No channel assigned to {category.Name}. Please assign one with `/category channel`."));
            }
        }
        ctx.Client.Logger.LogDebug($"Category {name} does not exist.");
    }
}
