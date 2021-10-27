using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Text;

namespace DBuddyBot.Models
{
    public class Category
    {
        #region backingfields
        private readonly int _id;
        private readonly string _name;
        private readonly Channel _channel;
        private readonly List<Role> _roles;

        #endregion backingfields

        #region properties
        public int Id => _id;
        public string Name => _name;
        public Channel Channel => _channel;
        public List<Role> Roles => _roles;
        #endregion properties

        #region constructors

        public Category(string name, Channel channel)
        {
            _name = name;
            _channel = channel;
            _roles = new();
        }

        public Category(int id, string name, Channel channel)
        {
            _id = id;
            _name = name;
            _channel = channel;
            _roles = new();
        }

        //TODO: Allow sorting of roles in specific orders (Alphabetically, certain roles first etc.)
        public DiscordEmbed GetEmbed(DiscordClient client)
        {
            DiscordEmbedBuilder builder = new();
            builder.Title = Name;
            builder.Description = $"Roles in the {Name} category";
            builder.Color = DiscordColor.Orange;
            StringBuilder roleString = new();
            foreach (Role role in Roles)
            {
                roleString.AppendLine($"{role.Name} {DiscordEmoji.FromGuildEmote(client, role.EmoteId)}");
            }
            if (string.IsNullOrWhiteSpace(roleString.ToString()))
            {
                roleString.Clear();
                roleString.Append("No roles available in this category.");
            }
            builder.AddField("Sign up to roles by reacting with the given emote", roleString.ToString());

            return builder.Build();
        }

        #endregion constructors
    }
}
