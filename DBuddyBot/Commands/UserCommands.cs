using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DBuddyBot.Commands
{
    [Group("game")]
    public class UserCommands : BaseCommandModule
    {
        #region properties
        public IAppDatabase Database { private get; set; }
        #endregion properties


        #region commandmethods
        /// <summary>
        /// <c>AddGameToUser()</c> adds the game role to the user.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name"></param>
        /// <returns>Name of the game</returns>
        [Command("get")]
        public async Task AddGameToUser(CommandContext ctx, [RemainingText] string name)
        {
            name = name.ToTitleCase();
            if (Database.TryGetGame(name, out Game game))
            {
                DiscordRole role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name.ToLower() == game.Name.ToLower()).Value;
                await ctx.Member.GrantRoleAsync(role, $"User added {game.Name} to their collection.");
                game.AddSubscriber();
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} subscribed to {game.Name}.");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"The game {name} does not exist as a role yet. Ask an admin to add it to the collection!");
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":no_entry:"));
                ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} tried to subscribe to {name}, but does not exist.");
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
            name = name.ToTitleCase();
            if (Database.TryGetGame(name, out Game game))
            {
                DiscordRole role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name.ToLower() == game.Name.ToLower()).Value;
                if (ctx.Member.Roles.Contains(role))
                {
                    await ctx.Member.RevokeRoleAsync(role);
                    game.RemoveSubscriber();
                }
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} unsubscribed from {game.Name}.");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"The game {name} does not exist as a role!");
                ctx.Client.Logger.Log(LogLevel.Information, $"{ctx.Member.Username} tried to unsubscribe from {name}, but does not exist.");
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
            name = name.ToTitleCase();
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

        #endregion commandmethods
    }

}
