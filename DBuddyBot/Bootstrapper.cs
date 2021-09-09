using System.IO;
using System.Linq;
using System.Text.Json;

namespace DBuddyBot
{
    public static class Bootstrapper
    {
        private static string _discordToken;
        private static string[] _commandPrefixes;

        public static string DiscordToken { get => _discordToken; }
        public static string[] CommandPrefixes { get => _commandPrefixes; }

        public static void Setup()
        {
            string filePath = @".\Config\BotConfig.json";
            if (File.Exists(filePath))
            {

                using (JsonDocument json = JsonDocument.Parse(File.ReadAllText(filePath)))
                {
                    if (json.RootElement.TryGetProperty("discord", out JsonElement discord))
                    {
                        if (discord.TryGetProperty("discord_token", out JsonElement token))
                        {
                            _discordToken = token.GetString();
                        }
                        if(discord.TryGetProperty("command_prefixes", out JsonElement prefixes))
                        {
                            _commandPrefixes = prefixes.EnumerateArray().Select(x => x.GetString()).ToArray();
                        }

                    }
                }
            }
        }
    }
}
