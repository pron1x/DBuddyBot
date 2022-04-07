using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBuddyBot.Commands;

public class CategoryAutocompleteProvider : IAutocompleteProvider
{
    public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        List<string> categories = Bootstrapper.Database.GetAllCategoryNames();
        return Task.Run(() => categories.FindAll(c => c.Contains((string)ctx.OptionValue)).Select(c => new DiscordAutoCompleteChoice(c, c)));
    }
}
