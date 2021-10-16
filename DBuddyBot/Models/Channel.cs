using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBuddyBot.Models
{
    public class Channel
    {
        #region backingfields
        private ulong _id;
        private string _name;
        private List<RoleMessage> _messages;

        #endregion backingfields

        #region properties
        public ulong Id => _id;
        public string Name => _name;
        public List<RoleMessage> Messages => _messages;
        #endregion properties

        #region constructors
        public Channel(ulong id, string name)
        {
            _id = id;
            _name = name;
            _messages = new();
        }

        #endregion constructors

    }
}
