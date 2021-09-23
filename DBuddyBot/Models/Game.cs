using DSharpPlus.Entities;

namespace DBuddyBot.Models
{
    /// <summary>
    /// Class <c>Game</c> models a game with basic information.
    /// </summary>
    public class Game
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
        private readonly DiscordRole _gameRole;
        private int _subscribers;
        #endregion backingfields

        #region properties
        public int Id { get => _id; }
        public string Name { get => _name; }
        public DiscordRole GameRole { get => _gameRole; }
        public int Subscribers { get => _subscribers; }
        #endregion properties


        #region constructors
        public Game(string name)
        {
            _id = System.Threading.Interlocked.Increment(ref idCounter);
            _name = name;
            _subscribers = 0;
        }


        public Game(int id, string name, int subscribers)
        {
            _id = id;
            _name = name;
            _subscribers = subscribers;
        }

        #endregion constructors


        #region publicmethods
        public void AddSubscriber()
            => _subscribers++;


        public void RemoveSubscriber()
            => _subscribers--;
        #endregion publicmethods


        #region overrides
        public override bool Equals(object obj)
        {
            if (obj is not Game)
            {
                return false;
            }
            Game game = (Game)obj;

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
