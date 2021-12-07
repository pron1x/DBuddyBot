namespace DBuddyBot.Models
{
    /// <summary>
    /// Class <c>Game</c> models a game with basic information.
    /// </summary>
    public class Role
    {
        #region backingfields
        private readonly int _id;
        private readonly ulong _discordId;
        private readonly string _name;
        private readonly string _description;
        private readonly bool _isGame;
        #endregion backingfields

        #region properties
        public int Id => _id;
        public ulong DiscordId => _discordId;
        public string Name => _name;
        public string Description => _description;
        public bool IsGame => _isGame;
        public string ComponentId => $"button_{Name.ToLower().Replace(' ', '_')}";
        #endregion properties


        #region constructors

        public Role(int id, ulong discordId, string name, string description, bool isGame = false)
        {
            _id = id;
            _discordId = discordId;
            _name = name;
            _description = description;
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

            return role.DiscordId == DiscordId;
        }


        public override int GetHashCode()
        {
            return DiscordId.GetHashCode();
        }


        public override string ToString()
        {
            return Name;
        }
        #endregion overrides
    }
}
