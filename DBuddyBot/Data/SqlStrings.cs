namespace DBuddyBot.Data
{
    internal static class SqlStrings
    {
        #region constants
        /// <summary>
        /// Parameters: $name
        /// </summary>
        public const string InsertCategory = "INSERT INTO categories(name) VALUES ($name);";

        /// <summary>
        /// Paramters: $roleId, $roleName, $roleIsGame, $categoryId
        /// </summary>
        public const string InsertRole = "INSERT INTO roles(id, name, game, category_id) VALUES ($roleId, $roleName, $roleIsGame, $categoryId);";

        /// <summary>
        /// Parameters: $emojiId, $emojiName, $emojiRole
        /// </summary>
        public const string InsertEmoji = "INSERT INTO emojis(id, name, role) VALUES ($emojiId, $emojiName, $emojiRole);";

        /// <summary>
        /// Parameters: $channelId, $categoryId
        /// </summary>
        public const string InsertChannel = "INSERT INTO channels(id, category_id) VALUES ($channelId, $categoryId);";

        /// <summary>
        /// Parameters: $messageId, $categoryId
        /// </summary>
        public const string UpdateCategoryMessage = "UPDATE categories SET message = $messageId WHERE id = $categoryId;";

        public const string SelectCategoryNames = "SELECT name FROM categories;";

        /// <summary>
        /// Parameters: $name
        /// </summary>
        public const string SelectCategoryOnName = "SELECT ca.id AS categoryId, ca.name AS categoryName, ca.message as categoryMessage, ch.id AS channelId, ro.id AS roleId, ro.name AS roleName, ro.game AS roleGame, em.id as emojiId, em.name as emojiName "
                                                   + "FROM categories ca LEFT JOIN channels ch ON ca.id = ch.category_id LEFT JOIN roles ro ON ca.id = ro.category_id LEFT JOIN emojis em on ro.id = em.role WHERE lower(categoryName) = $name;";

        /// <summary>
        /// Parameters: $name
        /// </summary>
        public const string SelectRoleOnName = "SELECT roles.id, roles.name, roles.game, emojis.id, emojis.name FROM roles JOIN emojis ON roles.id = emojis.role WHERE lower(roles.name) = $name;";

        /// <summary>
        /// Parameters: $id
        /// </summary>
        public const string SelectRoleOnId = "SELECT roles.id, roles.name, roles.game, emojis.id, emojis.name FROM roles JOIN emojis ON roles.id = emojis.role WHERE roles.id = $id;";

        /// <summary>
        /// Parameters: $emojiName
        /// </summary>
        public const string SelectRoleOnEmoji = "SELECT roles.id, roles.name, roles.game, emojis.id, emojis.name FROM roles JOIN emojis ON roles.id = emojis.role WHERE emojis.name = $emojiName;";

        /// <summary>
        /// Parameters: $id
        /// </summary>
        public const string SelectChannelOnId = "SELECT * FROM channels WHERE id=$id;";

        /// <summary>
        /// Parameters: $id
        /// </summary>
        public const string DeleteEmojiOnRoleId = "DELETE FROM emojis WHERE role = $id;";

        /// <summary>
        /// Parameters: $id
        /// </summary>
        public const string DeleteRoleOnId = "DELETE FROM roles WHERE id = $id;";


        #endregion constants
    }
}
