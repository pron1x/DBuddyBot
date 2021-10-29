﻿using DSharpPlus;
using DSharpPlus.Entities;
using Serilog;
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
        private readonly RoleMessage _message;
        private readonly List<Role> _roles;

        #endregion backingfields

        #region properties
        public int Id => _id;
        public string Name => _name;
        public Channel Channel => _channel;
        public RoleMessage Message => _message;
        public List<Role> Roles => _roles;
        #endregion properties

        #region constructors

        public Category(string name, Channel channel, RoleMessage message)
        {
            _name = name;
            _channel = channel;
            _message = message;
            _roles = new();
        }

        public Category(int id, string name, Channel channel, RoleMessage message)
        {
            _id = id;
            _name = name;
            _channel = channel;
            _message = message;
            _roles = new();
        }

        //TODO: Allow sorting of roles in specific orders (Alphabetically, certain roles first etc.)
        //TODO!!!: Check why guild emotes cannot be fetched from name!
        public DiscordEmbed GetEmbed(DiscordClient client)
        {
            DiscordEmbedBuilder builder = new();
            builder.Title = Name;
            builder.Description = $"Roles in the {Name} category";
            builder.Color = DiscordColor.Orange;
            StringBuilder roleString = new();
            foreach (Role role in Roles)
            {
                if (!DiscordEmoji.TryFromName(client, role.Emote, out DiscordEmoji emoji))
                {
                    Log.Logger.Debug($"Could not get {role.Emote} from name");
                    continue;
                }
                roleString.AppendLine($"{role.Name} {emoji}");
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
