using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
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
        public async Task AddRole(CommandContext ctx, string categoryName, DiscordEmoji emoji, [RemainingText] string name)
        {
            categoryName = categoryName.ToTitleCase();
            name = name.ToTitleCase();
            Category category = Database.GetCategory(categoryName);
            if (category == null)
            {
                await ctx.Channel.SendMessageAsync($"No category {categoryName} exists. Can not add role to it.");
                return;
            }
            else if (category.Roles.Any(role => role.Name == name))
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
                Role newRole = new(role.Id, role.Name, new(emoji.Id, emoji.GetDiscordName()));
                category.Roles.Add(newRole);
                Database.AddRole(newRole, category.Id);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                ctx.Client.Logger.LogInformation($"{ctx.Member.Username} added {role.Name} to database.");
            }
            UpdateRoleMessage(ctx.Client, category);
        }


        /// <summary>
        /// Removes a role from the database
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="categoryName">Name of the category the role resides in</param>
        /// <param name="name">Name of the game to remove</param>
        /// <returns></returns>
        [Command("remove"), RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task RemoveRole(CommandContext ctx, string categoryName, [RemainingText] string name)
        {
            name = name.ToTitleCase();
            Category category = Database.GetCategory(categoryName);
            Role role = category.Roles.FirstOrDefault(role => role.Name == name);

            if (role == null)
            {
                await ctx.Channel.SendMessageAsync($"No role {name} exists. Can not remove it.");
                return;
            }
            else
            {
                category.Roles.Remove(role);
                Database.RemoveRole(role.Id);
                ctx.Client.Logger.LogInformation($"{ctx.Member.Username} removed role {role.Name} from database.");
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            UpdateRoleMessage(ctx.Client, category);
        }

        #endregion commandmethods

        #region privatemethods

        private async void UpdateRoleMessage(DSharpPlus.DiscordClient client, Category category)
        {
            DiscordChannel channel = await client.GetChannelAsync(category.Channel.Id);
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
