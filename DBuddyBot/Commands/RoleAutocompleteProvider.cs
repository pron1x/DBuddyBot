using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBuddyBot.Commands
{
    public class RoleAutocompleteProvider : IAutocompleteProvider
    {
        public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            Models.Category category = Bootstrapper.Database.GetCategory((string)ctx.Options.FirstOrDefault(option => option.Name == "category").Value);
            List<string> roles = category.Roles.Select(r => r.Name).ToList();
            return Task.Run(() => roles.FindAll(r => r.Contains((string)ctx.OptionValue)).Select(r => new DiscordAutoCompleteChoice(r, r)));
        }
    }

    public class DiscordRoleAutoCompleteProvider : IAutocompleteProvider
    {
        public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            return Task.Run(() => ctx.Guild.Roles.Where(r => r.Value.Name.Contains((string)ctx.OptionValue)).Select(r => new DiscordAutoCompleteChoice(r.Value.Name, r.Value.Name)));
        }
    }
}
