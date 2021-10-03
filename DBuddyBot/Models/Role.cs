namespace DBuddyBot.Models
{
    /// <summary>
    /// Class <c>Game</c> models a game with basic information.
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Simple internal ID counter to auto increment game IDs.
        /// </summary>
        #region staticfields
        private static int idCounter = 0;
        #endregion staticfields

        #region backingfields
        private readonly int _id;
        private readonly string _name;
        private readonly string _emoji;
        #endregion backingfields

        #region properties
        public int Id { get => _id; }
        public string Name { get => _name; }
        public string Emoji { get => _emoji; }
        #endregion properties


        #region constructors
        public Role(string name, string emoji)
        {
            _id = System.Threading.Interlocked.Increment(ref idCounter);
            _name = name;
            _emoji = emoji;
        }


        public Role(int id, string name, string emoji)
        {
            _id = id;
            _name = name;
            _emoji = emoji;
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
            Role game = (Role)obj;

            return game.Id == Id;
        }


        public override int GetHashCode()
        {
            return Id;
        }


        public override string ToString()
        {
            return Name;
        }
        #endregion overrides
    }
}
