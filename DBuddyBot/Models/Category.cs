using System.Collections.Generic;

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

        #endregion constructors
    }
}
