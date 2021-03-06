using DBuddyBot.Data;
using DBuddyBot.Models;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DBuddyBot.EventHandlers;

public class ClientMessageEventHandler
{
    private static readonly IDatabaseService _database;

    static ClientMessageEventHandler()
    {
        _database = Bootstrapper.Database;
    }

    internal static Task RemoveRoleMessage(DiscordClient sender, MessageDeleteEventArgs e)
    {
        return Task.Run(() =>
        {
            sender.Logger.LogInformation("Message {message} has been deleted in guild {guild}({id}).", e.Message.Id, e.Guild.Name, e.Guild.Id);
            Category category;
            if ((category = _database.GetCategoryFromMessage(e.Message.Id)) != null)
            {
                _database.UpdateMessage(category.Id, 0);
                sender.Logger.LogInformation("Message of category {category} has been removed.", category.Name);
            }
        });
    }
}
