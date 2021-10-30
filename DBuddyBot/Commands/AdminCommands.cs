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
        }


        /// <summary>
        /// Removes a role from the database
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name">Name of the game to remove</param>
        /// <returns></returns>
        [Command("remove"), RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task RemoveRole(CommandContext ctx, [RemainingText] string name)
        {
            name = name.ToTitleCase();
            Role role = Database.GetRole(name);

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
        }

        #endregion commandmethods
    }
}
