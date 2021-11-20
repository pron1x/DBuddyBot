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
        public IDatabaseService Database { private get; set; }
        #endregion properties


        #region commandmethods
        
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
            if (Database.TryGetRole(name, out Role game))
            {
                DiscordRole role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name.ToLower() == game.Name.ToLower()).Value;
                int amount = ctx.Guild.Members.Count(member => member.Value.Roles.Contains(role));
                await ctx.Channel.SendMessageAsync($"{game.Name} currently has {amount} subscribers!");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{name} does not exist in the database. Perhaps you should recommend adding it.");
            }
        }

        #endregion commandmethods
    }

}
