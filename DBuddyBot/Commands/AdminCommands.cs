﻿using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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
            if (!Database.TryGetGame(name, out Game game))
            {
                DiscordRole role = await ctx.Guild.CreateRoleAsync(name: name, color: DiscordColor.Purple, mentionable: true, reason: $"{ctx.Member.Nickname} added {name} to the game database.");
                if (role != null)
                {
                    Game newGame = new(name, role);
                    Database.AddGame(newGame);
                    await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{game.Name} already exists in the Databank, currently has {game.Subscribers}, no need to add it again");
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
                await game.GameRole.DeleteAsync($"{ctx.Member.Nickname} removed the game from database.");
                Database.RemoveGame(game.Id);
                await ctx.Channel.SendMessageAsync($"Succesfully removed role for {game.Name}! {game.Subscribers} members were subscribed to it.");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"Game {name} does not exist in the database, no need to remove it.");
            }
        }
    }
}
