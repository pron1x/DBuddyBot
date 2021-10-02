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
        public IAppDatabase Database { private get; set; }
        #endregion properties


        #region commandmethods
        /// <summary>
        /// Adds a game to the database and creates the corresponding discord role
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name">Name of the game to add</param>
        /// <returns></returns>
        [Command("add"), RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task AddGame(CommandContext ctx, DiscordEmoji emoji, [RemainingText] string name)
        {
            name = name.ToTitleCase();
            bool existsInDatabase = Database.TryGetGame(name, out Game game);
            DiscordRole existsAsRole = ctx.Guild.Roles.FirstOrDefault(r => r.Value.Name.ToLower() == name.ToLower()).Value;
            if (existsAsRole != null && !existsInDatabase)
            {
                game = new(existsAsRole.Name, emoji.GetDiscordName());
                Database.AddGame(game);
                await ctx.Channel.SendMessageAsync($"A role named {existsAsRole.Name} already exists on the Server, it has been added to the database.");
                ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} added {game.Name} to database, role existed.");
            }
            else if (!existsInDatabase)
            {
                DiscordRole role = await ctx.Guild.CreateRoleAsync(name: name, color: DiscordColor.Purple, mentionable: true, reason: $"{ctx.Member.Nickname} added {name} to the game database.");
                if (role != null)
                {
                    Game newGame = new(name, emoji.GetDiscordName());
                    Database.AddGame(newGame);
                    await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                    ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} added {newGame.Name} to database, new role has been created.");
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{game.Name} already exists in the Databank, no need to add it again");
                ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} tried to add {name} to database, but already exists.");
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
            name = name.ToTitleCase();
            if (Database.TryGetGame(name, out Game game))
            {
                DiscordRole role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name.ToLower() == game.Name.ToLower()).Value;
                await role.DeleteAsync($"{ctx.Member.Nickname} removed the game from database.");
                Database.RemoveGame(game.Id);
                await ctx.Channel.SendMessageAsync($"Succesfully removed role for {game.Name}!");
                ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} removed {game.Name} from database.");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"Game {name} does not exist in the database, no need to remove it.");
                ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} tried to remove {name} from database, but does not exist.");
            }
        }

        #endregion commandmethods
    }
}
