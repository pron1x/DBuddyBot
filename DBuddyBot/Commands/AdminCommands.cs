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

        //TODO: Update rolemessages on adding/removing roles.
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
            ctx.Client.Logger.LogDebug($"Emoji info. Id: {emoji.Id} discordname: {emoji.GetDiscordName()} name:{emoji.Name}");
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
                Database.AddRole(new(role.Id, role.Name, new(emoji.Id, emoji.GetDiscordName())), category.Id);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                ctx.Client.Logger.LogInformation($"{ctx.Member.Username} added {role.Name} to database.");
            }
            UpdateRoleMessage(ctx.Client, category.Name);
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
                Database.RemoveRole(role.Id);
                ctx.Client.Logger.LogInformation($"{ctx.Member.Username} removed role {role.Name} from database.");
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            UpdateRoleMessage(ctx.Client, categoryName);
        }

        #endregion commandmethods

        #region privatemethods

        private async void UpdateRoleMessage(DSharpPlus.DiscordClient client, string categoryName)
        {
            Category category = Database.GetCategory(categoryName);
            DiscordChannel channel = await client.GetChannelAsync(category.Channel.Id);
            DiscordEmbed embed = category.GetEmbed(client);
            if (category.Message == null)
            {
                DiscordMessage message = await channel.SendMessageAsync(embed);
                foreach (Role role in category.Roles)
                {
                    if (DiscordEmoji.TryFromName(client, role.Emoji.Name, out DiscordEmoji emoji))
                    {
                        await message.CreateReactionAsync(emoji);
                    }
                    else if (DiscordEmoji.TryFromGuildEmote(client, role.Emoji.Id, out emoji))
                    {
                        await message.CreateReactionAsync(emoji);
                    }
                }
                Database.UpdateMessage(category.Id, message.Id);
            }
            else
            {
                DiscordMessage message = await channel.GetMessageAsync(category.Message.Id);
                if (embed == null)
                {
                    await message.DeleteAsync();
                    Database.UpdateMessage(category.Id, 0);
                    return;
                }
                else
                {
                    await message.ModifyAsync(embed);
                    List<DiscordEmoji> messageEmojis = message.Reactions.Select(reaction => reaction.Emoji).ToList();
                    foreach (Role role in category.Roles)
                    {
                        bool success = role.Emoji.Name == "" ? DiscordEmoji.TryFromGuildEmote(client, role.Emoji.Id, out DiscordEmoji emoji)
                            : DiscordEmoji.TryFromName(client, role.Emoji.Name, out emoji);
                        if (success)
                        {
                            if (messageEmojis.Contains(emoji))
                            {
                                messageEmojis.Remove(emoji);
                            }
                            else
                            {
                                await message.CreateReactionAsync(emoji);
                            }
                        }
                    }
                    if(messageEmojis.Count > 0)
                    {
                        foreach(DiscordEmoji emoji in messageEmojis)
                        {
                            await message.DeleteReactionsEmojiAsync(emoji);
                        }
                    }
                }
            }
        }

        #endregion privatemthods
    }
}
