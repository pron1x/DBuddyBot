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
+ `/category add <category name> <discord channel> [color] [description]`
  + `<category name>` specifies the unique name of the new category
  + `<discord channel>` specifies the channel the category message will be posted in
  + `[color]` specifies an optional color for the embed of the category message. Accepts a HEX value with/without the '#' prefix
  + `[description]` specifies an optional description for the category, which will be included in the category message
+ `/category remove <category name>`
  + `<category name>` specifies the name of the category that should be removed. Removes the category and roles from the database
+ `/category refresh <category name>`
  + `<category name>` specifies the name of the category that should be refreshed. This forces an update of the category message.
+ `/role add <category name> <role name> [description]`
  + `<category name>` specifies the category the new role is part of
  + `<role name>` specifies the name of the role. Checks if a Discord role with the name exists before creating a new one
  + `[description]` specifies an optional description of the role
+ `/role remove <category name> <role name>`
  + `<category name>` specifies the category the role is part of
  + `<role name>` specifies the name of the role to be removed. Removes it from the category and database if it is not part of a different category, 
  does not delete the Discord role.
##### Server Users
Server users can assign the roles by clicking on the buttons. The buttons work as a toggle, if the user is not part of a role, the role will be granted,
if they are, the role will instead be revoked.


### Roadmap

Roadmap of things to come.
- [ ] Add multi-server support (mutliple servers are currently **NOT** supported!)
- [ ] Potentially lower the amount of dependencies
- [x] Use Discords slash commands instead of normal text based commands
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
        "discord_token": "token here"
    },
    "database": {
        "connection_string": ""
    },
    "categories": [
        {
            "name": "Name 1",
            "channel": channel_id,
            "color": "#FFFFFF",
            "description": "Descriptive text!"
        },
        {
            "name": "Name 2",
            "channel": channel_id,
            "color": "#123123",
            "description": "It's another category."
        }
    ]
}
```
The ``"discord_token"`` key holds the bots discord token. To use a custom database (currently SQLite and Sql Server should be supported) put the connection string in the corresponding
field. Leave empty to use the default database.\
Categories are added in the ``"categories"`` array. The category elements should contain the `"name"`,`"channel"`,`"color"` and `"description"` properties.
If the `"name"` or `"channel"` attributes are missing or could not be parsed, the category is skipped and not added to the database. In the case of a missing/faulty 
`"color"` attribute a standard value of `"#000000"` (black) is used. A missing/faulty `"description"` results in an empty description.


### License
I'm not a lawyer but licenses are good I think.
So this thing is licensed under the [**MIT LICENSE**](https://github.com/pron1x/DBuddyBot/blob/master/LICENSE).

---
