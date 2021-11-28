﻿using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
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
        public IEnumerable<Role> Roles => _roles.AsReadOnly();
        public int RoleCount => _roles.Count;
        #endregion properties

        #region constructors

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

        public Role GetRole(string name)
        {
            return Roles.FirstOrDefault(r => r.Name == name);
        }

        public Role GetRoleFromComponentId(string componentId)
        {
            return Roles.FirstOrDefault(role => role.ComponentId == componentId);
        }

        public bool AddRole(Role role)
        {
            if (RoleCount >= 25)
            {
                return false;
            }
            _roles.Add(role);
            return true;
        }

        public bool RemoveRole(Role role)
        {
            return _roles.Remove(role);
        }


        public DiscordMessageBuilder GetMessage(DiscordClient client)
        {
            if (RoleCount == 0)
            {
                return null;
            }
            DiscordEmbedBuilder builder = new();
            DiscordMessageBuilder messageBuilder = new();
            List<List<DiscordComponent>> componentsList = new();
            builder.Title = Name;
            builder.Description = $"Roles in the {Name.ToTitleCase()} category";
            builder.Color = DiscordColor.Orange;
            StringBuilder roleString = new();
            foreach (Role role in Roles)
            {
                if (componentsList.Count == 0)
                {
                    componentsList.Add(new List<DiscordComponent>());
                }
                roleString.AppendLine($"{role.Name.ToTitleCase()}");
                if (componentsList[^1].Count >= 5)
                {
                    componentsList.Add(new List<DiscordComponent>());
                }
                componentsList[^1].Add(new DiscordButtonComponent(ButtonStyle.Primary, role.ComponentId, role.Name.ToTitleCase()));

            }
            builder.AddField("Sign up to roles by clicking on the button.", roleString.ToString());
            messageBuilder.AddEmbed(builder.Build());
            componentsList.ForEach(x => messageBuilder.AddComponents(x));
            return messageBuilder;
        }

        #endregion publicmethods
    }
}
