namespace DBuddyBot.Models
{
    /// <summary>
    /// Class <c>Game</c> models a game with basic information.
    /// </summary>
    public class Role
    {
        #region backingfields
        private readonly ulong _id;
        private readonly string _name;
        private readonly ulong _emoteId;
        private readonly bool _isGame;
        #endregion backingfields

        #region properties
        public ulong Id => _id;
        public string Name => _name;
        public ulong EmoteId => _emoteId;
        public bool IsGame => _isGame;
        #endregion properties


        #region constructors

        public Role(ulong id, string name, ulong emoteId, bool isGame = false)
        {
            _id = id;
            _name = name;
            _emoteId = emoteId;
            _isGame = isGame;
        }

        #endregion constructors


        #region publicmethods

        #endregion publicmethods


        #region overrides
        public override bool Equals(object obj)
        {
            if (obj is not Role)
            {
                return false;
            }
            Role role = (Role)obj;

            return role.Id == Id;
        }


        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }


        public override string ToString()
        {
            return Name;
        }
        #endregion overrides
    }
}
