namespace DBuddyBot.Data
{
    internal static class SqlStrings
    {
        #region constants

        internal const string CreateTableCategories = "CREATE TABLE IF NOT EXISTS categories (id INTEGER PRIMARY KEY AUTOINCREMENT, "
                                                      + "name TEXT NOT NULL UNIQUE, "
                                                      + "color INT NOT NULL, "
                                                      + "description TEXT, "
                                                      + "message UNSIGNED BIG INTEGER UNIQUE, "
                                                      + "channel_id INTEGER, "
                                                      + "FOREIGN KEY(channel_id) REFERENCES channels(id));";

        internal const string CreateTableRoles = "CREATE TABLE IF NOT EXISTS roles (id INTEGER PRIMARY KEY AUTOINCREMENT, "
                                                 + "d_id UNSIGNED BIG INTEGER UNIQUE NOT NULL, "
                                                 + "name TEXT NOT NULL UNIQUE, "
                                                 + "description TEXT, "
                                                 + "game BOOL);";

        internal const string CreateTableCategoriesRoles = "CREATE TABLE IF NOT EXISTS categories_roles (category_id INTEGER, "
                                                           + "role_id INTEGER, "
                                                           + "PRIMARY KEY(category_id, role_id), "
                                                           + "FOREIGN KEY(category_id) REFERENCES categories(id), "
                                                           + "FOREIGN KEY(role_id) REFERENCES roles(id));";

        internal const string CreateTableChannels = "CREATE TABLE IF NOT EXISTS channels (id INTEGER PRIMARY KEY AUTOINCREMENT, "
                                                    + "d_id UNSIGNED BIG INTEGER UNIQUE NOT NULL);";

        /// <summary>
        /// Parameters: $name, $channelId, $color, $description
        /// </summary>
        internal const string InsertCategory = "INSERT INTO categories(name, channel_id, color, description) VALUES ($name, $channelId, $color, $description);";

        /// <summary>
        /// Paramters: $roleId, $roleName, $description, $roleIsGame
        /// </summary>
        internal const string InsertRole = "INSERT INTO roles(d_id, name, description, game) VALUES ($roleId, $roleName, $description, $roleIsGame);";

        /// <summary>
        /// Parameters: $categoryId, $roleId
        /// </summary>
        internal const string InsertCategoryRole = "INSERT INTO categories_roles(category_id, role_id) VALUES ($categoryId, $roleId);";

        /// <summary>
        /// Parameters: $channelId
        /// </summary>
        internal const string InsertChannel = "INSERT INTO channels(d_id) VALUES ($channelId);";

        /// <summary>
        /// Parameters: $messageId, $categoryId
        /// </summary>
        internal const string UpdateCategoryMessage = "UPDATE categories SET message = $messageId WHERE id = $categoryId;";

        /// <summary>
        /// Parameters: $description, $categoryId
        /// </summary>
        internal const string UpdateCategoryDescriptionOnId = "UPDATE categories SET description = $description WHERE id = $categoryId;";

        internal const string SelectCategoryNames = "SELECT name FROM categories;";

        /// <summary>
        /// Parameters: $name
        /// </summary>
        internal const string SelectCategoryOnName = "SELECT ca.id AS categoryId, ca.name AS categoryName, ca.description AS categoryDescription, ca.color AS categoryColor, ca.message as categoryMessage, "
                                                     + "ch.id AS channelId, ch.d_id AS channelDiscordId, ro.id as roleId, ro.d_id AS roleDId, ro.name AS roleName, ro.description AS roleDescription, ro.game AS roleGame "
                                                     + "FROM categories ca LEFT JOIN categories_roles cr ON cr.category_id = ca.id LEFT JOIN roles ro ON cr.role_id = ro.id LEFT JOIN channels ch ON ca.channel_id = ch.id WHERE lower(categoryName) = $name;";

        /// <summary>
        /// Parameters: $messageId
        /// </summary>
        internal const string SelectCategoryOnMessage = "SELECT ca.id AS categoryId, ca.name AS categoryName, ca.description AS categoryDescription, ca.color AS categoryColor, ca.message as categoryMessage, "
                                                        + "ch.id AS channelId, ch.d_id AS channelDiscordId, ro.id as roleId, ro.d_id AS roleDId, ro.name AS roleName, ro.description AS roleDescription, ro.game AS roleGame "
                                                        + "FROM categories ca LEFT JOIN categories_roles cr ON cr.category_id = ca.id LEFT JOIN roles ro ON cr.role_id = ro.id LEFT JOIN channels ch ON ca.channel_id = ch.id WHERE categoryMessage = $messageId;";

        /// <summary>
        /// Parameters: $name
        /// </summary>
        internal const string SelectRoleOnName = "SELECT roles.id, roles.d_id, roles.name, roles.description, roles.game FROM roles WHERE lower(roles.name) = $name;";

        /// <summary>
        /// Parameters: $id
        /// </summary>
        internal const string SelectRoleOnId = "SELECT roles.id, roles.d_id, roles.name, roles.description, roles.game FROM roles WHERE roles.d_id = $id;";

        /// <summary>
        /// Parameters: $id
        /// </summary>
        internal const string SelectChannelOnId = "SELECT * FROM channels WHERE d_id=$id;";

        /// <summary>
        /// Parameters: $id
        /// </summary>
        internal const string DeleteRoleOnId = "DELETE FROM roles WHERE (SELECT COUNT(categories_roles.role_id) FROM categories_roles WHERE categories_roles.role_id = $id) == 0 AND id = $id;";

        /// <summary>
        /// Parameters: $categoryId, $roleId
        /// </summary>
        internal const string DeleteRoleCategoryOnRoleId = "DELETE FROM categories_roles WHERE category_id = $categoryId AND role_id = $roleId;";

        /// <summary>
        /// Parameters: $id
        /// </summary>
        internal const string DeleteChannelOnId = "DELETE FROM channels WHERE(SELECT COUNT(categories.channel_id) FROM categories WHERE categories.channel_id == $id) == 1 AND channels.ID == $id;";

        /// <summary>
        /// Parameters: $id
        /// </summary>
        internal const string DeleteCategoryOnId = "DELETE FROM categories WHERE id = $id;";

       

        #endregion constants
    }
}
