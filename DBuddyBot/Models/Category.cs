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

        #endregion constructors


        #region publicmethods

        public DiscordMessageBuilder GetMessage(DiscordClient client)
        {
            DiscordEmbedBuilder builder = new();
            DiscordMessageBuilder messageBuilder = new();
            List<DiscordComponent> components = new();
            builder.Title = Name;
            builder.Description = $"Roles in the {Name} category";
            builder.Color = DiscordColor.Orange;
            StringBuilder roleString = new();
            foreach (Role role in Roles)
            {
                bool success = role.Emoji.Name == "" ? DiscordEmoji.TryFromGuildEmote(client, role.Emoji.Id, out DiscordEmoji emoji)
                            : DiscordEmoji.TryFromName(client, role.Emoji.Name, out emoji);
                if (success)
                {
                    roleString.AppendLine($"{role.Name} {(emoji ?? "'No emoji found'")}");
                    components.Add(new DiscordButtonComponent(ButtonStyle.Primary, role.ComponentId, role.Name));
                }
                else
                {
                    Log.Logger.Debug($"Could not get Emoji({role.Emoji.Id},{role.Emoji.Name}) from name or id");
                }
            }
            if (string.IsNullOrWhiteSpace(roleString.ToString()))
            {
                return null;
            }
            builder.AddField("Sign up to roles by reacting with the given emote", roleString.ToString());
            messageBuilder.AddEmbed(builder.Build());
            messageBuilder.AddComponents(components);
            return messageBuilder;
        }

        #endregion publicmethods
    }
}
