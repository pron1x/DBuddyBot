using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace DBuddyBot.Commands
{
    public class AdminCommands : BaseCommandModule
    {
        #region properties
        public IDatabaseService Database { private get; set; }
        #endregion properties


        #region commandmethods
        /// <summary>
        /// Adds a role to the database and creates the corresponding discord role
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name">Name of the game to add</param>
        /// <returns></returns>
        [Command("add"), RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task AddRole(CommandContext ctx, string categoryName, [RemainingText] string name)
        {
            categoryName = categoryName.ToTitleCase();
            name = name.ToTitleCase();
            Category category = Database.GetCategory(categoryName);
            if (category == null)
            {
                await ctx.Channel.SendMessageAsync($"No category {categoryName} exists. Can not add role to it.");
                return;
            }
            else if (category.GetRole(name) != null)
            {
                await ctx.Channel.SendMessageAsync($"Role {name} already exists in managed context, no need to add it again.");
                return;
            }
            else
            {
                DiscordRole role = ctx.Guild.Roles.FirstOrDefault(role => role.Value.Name == name).Value;
                if (role == null)
                {
                    role = await ctx.Guild.CreateRoleAsync(name, DSharpPlus.Permissions.None, DiscordColor.Brown, mentionable: true);
                }
                Role newRole = new(role.Id, role.Name);
                if (category.AddRole(newRole))
                {
                    Database.AddRole(newRole, category.Id);
                    await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                    ctx.Client.Logger.LogInformation($"{ctx.Member.Username} added {role.Name} to database.");
                    UpdateRoleMessage(ctx.Client, category);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync($"{category.Name} category already has 25 roles, the current maximum supported. " +
                        $"{role.Name} has been created, but needs to be added to a different category.");
                }
            }
        }


        /// <summary>
        /// Removes a role from the database
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="categoryName">Name of the category the role resides in</param>
        /// <param name="name">Name of the role to remove</param>
        /// <returns></returns>
        [Command("remove"), RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task RemoveRole(CommandContext ctx, string categoryName, [RemainingText] string name)
        {
            name = name.ToTitleCase();
            Category category = Database.GetCategory(categoryName);
            if (category == null)
            {
                await ctx.Channel.SendMessageAsync($"No category {categoryName} exists, cannot remove from it.");
            }
            Role role = category?.GetRole(name);

            if (role == null)
            {
                await ctx.Channel.SendMessageAsync($"No role {name} exists. Can not remove it.");
                return;
            }
            else
            {
                category.RemoveRole(role);
                Database.RemoveRole(role.Id);
                ctx.Client.Logger.LogInformation($"{ctx.Member.Username} removed role {role.Name} from database.");
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            UpdateRoleMessage(ctx.Client, category);
        }

        [Command("addcategory"), RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task AddCategory(CommandContext ctx, string name, DiscordChannel channel)
        {
            name = name.ToTitleCase();
            Category category = Database.GetCategory(name);
            if (category == null)
            {
                int categoryId = Database.AddCategory(name, channel.Id);
                if (categoryId == -1)
                {
                    await ctx.Channel.SendMessageAsync($"Category {name} already exists, or something went wrong.");
                    return;
                }
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"Category {name} already exists, or something went wrong.");
            }

        }

        [Command("removecategory"), RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task RemoveCategory(CommandContext ctx, string name)
        {
            name = name.ToTitleCase();
            Category category = Database.GetCategory(name);
            if (category == null)
            {
                await ctx.Channel.SendMessageAsync($"Category {name} does not exist, cannot remove it.");
                return;
            }
            else
            {
                Database.RemoveCategory(category);
                DiscordChannel channel = await ctx.Client.GetChannelAsync(category.Channel.DiscordId);
                DiscordMessage message = await channel.GetMessageAsync(category.Message.Id);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                await message.DeleteAsync($"Category {name} has been removed by {ctx.Member.Nickname}.");
            }
        }

        #endregion commandmethods

        #region privatemethods

        private async void UpdateRoleMessage(DSharpPlus.DiscordClient client, Category category)
        {
            DiscordChannel channel = await client.GetChannelAsync(category.Channel.DiscordId);
            DiscordMessageBuilder messageBuilder = category.GetMessage(client);
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

        #endregion privatemthods
    }
}
