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
        public IAppDatabase Database { private get; set; }

        /// <summary>
        /// Adds a game to the database and creates the corresponding discord role
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name">Name of the game to add</param>
        /// <returns></returns>
        [Command("add"), RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task AddGame(CommandContext ctx, [RemainingText] string name)
        {
            if (ctx.Guild.Roles.Any(r => r.Value.Name == name))
            {
                await ctx.Channel.SendMessageAsync($"A role named {name} already exists on the Server, will not create that again.");
            }
            else if (!Database.TryGetGame(name, out Game game))
            {
                DiscordRole role = await ctx.Guild.CreateRoleAsync(name: name, color: DiscordColor.Purple, mentionable: true, reason: $"{ctx.Member.Nickname} added {name} to the game database.");
                if (role != null)
                {
                    Game newGame = new(name);
                    Database.AddGame(newGame);
                    await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                    ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} added {newGame.Name} to database.");
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{game.Name} already exists in the Databank, currently has {game.Subscribers} subscribers, no need to add it again");
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
            if (Database.TryGetGame(name, out Game game))
            {
                DiscordRole role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == game.Name).Value;
                await role.DeleteAsync($"{ctx.Member.Nickname} removed the game from database.");
                Database.RemoveGame(game.Id);
                await ctx.Channel.SendMessageAsync($"Succesfully removed role for {game.Name}! {game.Subscribers} members were subscribed to it.");
                ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} removed {game.Name} from database.");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"Game {name} does not exist in the database, no need to remove it.");
                ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} tried to remove {name} from database, but does not exist.");
            }
        }
    }
}
