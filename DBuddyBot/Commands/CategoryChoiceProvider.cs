using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBuddyBot.Commands
{
    public class CategoryChoiceProvider : IChoiceProvider
    {
        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            List<Models.Category> categories = Bootstrapper.Database.GetAllCategories();
            return categories.Select(c => c.Name).Select(c => new DiscordApplicationCommandOptionChoice(c, c)).ToArray();

        }
    }
}
