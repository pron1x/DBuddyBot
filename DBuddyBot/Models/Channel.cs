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
        private List<RoleMessage> _messages;

        #endregion backingfields

        #region properties
        public ulong Id => _id;
        public List<RoleMessage> Messages => _messages;
        #endregion properties

        #region constructors
        public Channel(ulong id)
        {
            _id = id;
            _messages = new();
        }

        #endregion constructors

    }
}
