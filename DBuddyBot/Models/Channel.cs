namespace DBuddyBot.Models
{
    public class Channel
    {
        #region backingfields
        private readonly int _id;
        private readonly ulong _discordId;

        #endregion backingfields

        #region properties
        public int Id => _id;
        public ulong DiscordId => _discordId;
        #endregion properties

        #region constructors
        public Channel(int id, ulong discordId)
        {
            _id = id;
            _discordId = discordId;
        }

        #endregion constructors

    }
}
