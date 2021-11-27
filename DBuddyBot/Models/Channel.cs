namespace DBuddyBot.Models
{
    public class Channel
    {
        #region backingfields
        private readonly ulong _id;

        #endregion backingfields

        #region properties
        public ulong Id => _id;
        #endregion properties

        #region constructors
        public Channel(ulong id)
        {
            _id = id;
        }

        #endregion constructors

    }
}
