namespace DBuddyBot.Data
{
    internal static class SqlStrings
    {
        #region constants

        internal const string CreateTableCategories = "CREATE TABLE IF NOT EXISTS categories (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL UNIQUE, message UNSIGNED BIG INTEGER UNIQUE, channel_id INTEGER, FOREIGN KEY(channel_id) REFERENCES channels(id));";

        internal const string CreateTableRoles = "CREATE TABLE IF NOT EXISTS roles (id INTEGER PRIMARY KEY AUTOINCREMENT, d_id UNSIGNED BIG INTEGER UNIQUE NOT NULL, name TEXT NOT NULL UNIQUE, game BOOL, category_id UNSIGNED BIG INT NOT NULL, FOREIGN KEY(category_id) REFERENCES categories(id));";

        internal const string CreateTableChannels = "CREATE TABLE IF NOT EXISTS channels (id INTEGER PRIMARY KEY AUTOINCREMENT, d_id UNSIGNED BIG INTEGER UNIQUE NOT NULL);";

        /// <summary>
        /// Parameters: $name, $channelId
        /// </summary>
        internal const string InsertCategory = "INSERT INTO categories(name, channel_id) VALUES ($name, $channelId);";

        /// <summary>
        /// Paramters: $roleId, $roleName, $roleIsGame, $categoryId
        /// </summary>
        internal const string InsertRole = "INSERT INTO roles(d_id, name, game, category_id) VALUES ($roleId, $roleName, $roleIsGame, $categoryId);";

        /// <summary>
        /// Parameters: $channelId
        /// </summary>
        internal const string InsertChannel = "INSERT INTO channels(d_id) VALUES ($channelId);";

        /// <summary>
        /// Parameters: $messageId, $categoryId
        /// </summary>
        internal const string UpdateCategoryMessage = "UPDATE categories SET message = $messageId WHERE id = $categoryId;";

        internal const string SelectCategoryNames = "SELECT name FROM categories;";

        /// <summary>
        /// Parameters: $name
        /// </summary>
        internal const string SelectCategoryOnName = "SELECT ca.id AS categoryId, ca.name AS categoryName, ca.message as categoryMessage, ch.id AS channelId, ch.d_id AS channelDiscordId, ro.id AS roleId, ro.name AS roleName, ro.game AS roleGame "
                                                   + "FROM categories ca LEFT JOIN channels ch ON ca.channel_id = ch.id LEFT JOIN roles ro ON ca.id = ro.category_id WHERE lower(categoryName) = $name;";

        /// <summary>
        /// Parameters: $messageId
        /// </summary>
        internal const string SelectCategoryOnMessage = "SELECT ca.id AS categoryId, ca.name AS categoryName, ca.message as categoryMessage, ch.id AS channelId, ch.d_id AS channelDiscordId, ro.id AS roleId, ro.name AS roleName, ro.game AS roleGame "
                                                   + "FROM categories ca LEFT JOIN channels ch ON ca.channel_id = ch.id LEFT JOIN roles ro ON ca.id = ro.category_id WHERE categoryMessage = $messageId;";

        /// <summary>
        /// Parameters: $name
        /// </summary>
        internal const string SelectRoleOnName = "SELECT roles.id, roles.name, roles.game FROM roles WHERE lower(roles.name) = $name;";

        /// <summary>
        /// Parameters: $id
        /// </summary>
        internal const string SelectRoleOnId = "SELECT roles.id, roles.name, roles.game FROM roles WHERE roles.id = $id;";

        /// <summary>
        /// Parameters: $id
        /// </summary>
        internal const string SelectChannelOnId = "SELECT * FROM channels WHERE d_id=$id;";

        /// <summary>
        /// Parameters: $id
        /// </summary>
        internal const string DeleteRoleOnId = "DELETE FROM roles WHERE id = $id;";


        #endregion constants
    }
}
