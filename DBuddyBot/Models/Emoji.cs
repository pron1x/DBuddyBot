namespace DBuddyBot.Models
{
    public class Emoji
    {
        #region backingfields
        private ulong _id;
        private string _name;

        #endregion backingfields

        #region properties
        public ulong Id => _id;
        public string Name => _name;
        #endregion properties

        #region constructors
        public Emoji(ulong id, string name)
        {
            _id = id;
            _name = name;
        }

        #endregion constructors
    }
}
