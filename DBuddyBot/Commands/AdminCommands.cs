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
        /// Adds a game to the database and creates the corresponding discord role
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name">Name of the game to add</param>
        /// <returns></returns>
        [Command("add"), RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task AddGame(CommandContext ctx, string categoryName, DiscordEmoji emoji, [RemainingText] string name)
        {
            categoryName = categoryName.ToTitleCase();
            name = name.ToTitleCase();
            Category category = Database.GetCategory(categoryName);
            if (category == null)
            {
                await ctx.Channel.SendMessageAsync($"No category {categoryName} exists. Can not add role.");
                return;
            }

            if (category.Roles.Any(role => role.Name == name))
            {
                await ctx.Channel.SendMessageAsync($"Role {name} already exists in managed context, no need to add it again.");
                return;
            }
            else
            {
                DiscordRole role = ctx.Guild.Roles.First(role => role.Value.Name == name).Value;
                if (role == null)
                {
                    role = await ctx.Guild.CreateRoleAsync(name, DSharpPlus.Permissions.None, DiscordColor.Brown, mentionable: true);
                }
                category.Roles.Add(new(role.Id, role.Name, emoji.Id));
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} added {role.Name} to database.");
            }
        }


        /// <summary>
        /// Removes a game from the database and deletes the corresponding discord role
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name">Name of the game to remove</param>
        /// <returns></returns>
        [Command("remove"), RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task RemoveGame(CommandContext ctx, [RemainingText] string name)
        {
            /*
             * Needs rework to select Category based on if it contains the game, then delete it.
             * Own Database command for easier use?
             */

            //name = name.ToTitleCase();
            //if (Database.TryGetGame(name, out Role game))
            //{
            //    DiscordRole role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name.ToLower() == game.Name.ToLower()).Value;
            //    Database.RemoveGame(game.Id);
            //    ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} removed {game.Name} from database.");
            //    await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            //}
            //else
            //{
            //    await ctx.Channel.SendMessageAsync($"Game {name} does not exist in the database, no need to remove it.");
            //    ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} tried to remove {name} from database, but does not exist.");
            //}
        }

        #endregion commandmethods
    }
}
