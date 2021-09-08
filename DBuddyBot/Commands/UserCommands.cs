using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DBuddyBot.Commands
{
    [Group("game")]
    public class UserCommands : BaseCommandModule
    {
        public IAppDatabase Database { private get; set; }

        /// <summary>
        /// <c>AddGameToUser()</c> adds the game role to the user.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name"></param>
        /// <returns>Name of the game</returns>
        [Command("get")]
        public async Task AddGameToUser(CommandContext ctx, [RemainingText] string name)
        {
            if (Database.TryGetGame(name, out Game game))
            {
                await ctx.Member.GrantRoleAsync(game.GameRole, $"User added {game.Name} to their collection.");
                game.AddSubscriber();
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"The game {name} does not exist as a role yet. Ask an admin to add it to the collection!");
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":no_entry:"));
            }
        }


        /// <summary>
        /// <c>RemoveGameFromUser</c> removes the game role from the user.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name">Name of the game</param>
        /// <returns></returns>
        [Command("unsubscribe")]
        public async Task RemoveGameFromUser(CommandContext ctx, [RemainingText] string name)
        {
            if (Database.TryGetGame(name, out Game game))
            {
                if (ctx.Member.Roles.Contains(game.GameRole))
                {
                    await ctx.Member.RevokeRoleAsync(game.GameRole);
                    game.RemoveSubscriber();
                }
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"The game {name} does not exist as a role!");
            }
        }


        /// <summary>
        /// <c>ShowGameInfo</c> shows info about the game.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name">Name of the game</param>
        /// <returns></returns>
        [Command("info")]
        public async Task ShowGameInfo(CommandContext ctx, [RemainingText] string name)
        {
            // In future versions this could call the igdb api to retrieve more information about games.
            if (Database.TryGetGame(name, out Game game))
            {
                await ctx.Channel.SendMessageAsync($"{game.Name} currently has {game.Subscribers} subscribers!");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{name} does not exist in the database. Perhaps you should recommend adding it.");
            }
        }
    }

}
