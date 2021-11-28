# DBuddyBot
> A bot for categorized role assignment on discord servers.

---

## Overview

DBuddyBot<sup>1</sup> is a Discord bot for easy role assignment. It features a categorization system for roles,
so that multiple different role signup messages can be shown in different text channels. This could prove viable
especially for larger Discord communities that include multiple games or categories with their own role system.\
<sub><sup>1</sup> Name will probably change in the future<sub/>

Please report bugs either by opening an issue, or sending me a message on Discord.

### Features

The bot currently has the following features:
+ Adding new Discord roles (for administrators)
+ Adding new categories (for administrators)
+ Removing Discord roles from categories (does not delete the Discord role)
+ Removing categories
+ Adding/Removing roles by clicking the corresponding button by users
+ Use of a SQLite (standard) or Sql Server database.

### Usage

##### Server Administrators

The following commands are only usable by server members with the `Manage Roles` permission:
+ `addcategory <category name> <discord channel> [color] [description]`
  + `<category name>` specifies the unique name of the new category
  + `<discord channel>` specifies the channel the category message will be posted in
  + `[color]` specifies an optional color for the embed of the category message. Accepts a HEX value without the '#' prefix
  + `[description]` specifies an optional description for the category which will be included in the category message
+ `removecategory <category name>`
  + `<category name>` specifies the name of the category that should be removed. Removes the category and roles from the database
+ `add <category name> <role name> [description]`
  + `<category name>` specifies the category the new role is part of
  + `<role name>` specifies the name of the role. Must be surrounded with `""` if it contains spaces. 
  Checks if a Discord role with the name exists before creating a new one
  + `[description]` specifies an optional description of the role
+ `remove <category name> <role name>`
  + `<category name>` specifies the category the role is part of
  + `<role name>` specifies the name of the role to be removed. Removes it from the bot managed roles, does not delete the Discord role.
##### Server Users
Server users can assign the roles by clicking on the buttons. The buttons work as a toggle, if the user is part of a role, the role will be revoked,
if he is not the role will instead be granted.
The bot has a (deprecated) `info` command that anyone can use. `info <role name>` will return the amount of users that have the specified
role. This will either be removed or changed in future versions.

### Roadmap

Roadmap of things to come.
- [ ] Use Discords slash commands instead of normal text based commands
- [ ] Add multi-server support (no guarantee for multi-server support currently)
- [ ] Potentially lower the amount of dependencies
- [x] Use Discords button system for role assign/revoke instead of emojis (If the system proves to be viable)
- [x] Add/Remove categories via commands
- [x] SQLite and Sql Server support
- [x] Configuration improvements (on-going)

---
### Configuration
Currently, the configuration file resides in the `/Config/BotConfig.json` file.
It is parsed once on startup. In case the configuration file is missing, the bot creates an empty one and shuts down. 
```json
{
    "discord": {
        "discord_token": "token here",
        "command_prefixes": [ "?" ]
    },
    "database": {
        "connection_string": ""
    },
    "categories": {
        "categoryName": 10000000,
        "category2": 10000000,
        ...
    }
}
```
The ``"discord_token"`` key holds the bots discord token. Command prefixes can be added/changed in the ``"command_prefixes"``
array. To use a custom database (currently SQLite and Sql Server should be supported) put the connection string in the corresponding
field. Leave empty to use the default database.\
Categories are added in the ``"categories:"`` section. The key value pair should be ``"YourCategoryName": DiscordChannelId``.


### License
I'm not a lawyer but licenses are good I think.
So this thing is licensed under the [**MIT LICENSE**](https://github.com/pron1x/DBuddyBot/blob/master/LICENSE).

---
