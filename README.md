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
+ Adding/Removing roles by reacting to the corresponding emoji by users
+ Use of a SQLite (standard) or Sql Server database.

### Roadmap

Roadmap of things to come.
- [ ] Use Discords button system for role assign/revoke instead of emojis (If the system proves to be viable)
- [ ] Use Discords slash commands instead of normal text based commands
- [ ] Add/remove categories via commands
- [ ] Potentially lower the amount of dependencies
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
The ``"discord_token"`` key holds the bots discord token. Commnd prefixes can be added/changed in the ``"command_prefixes"``
array. To use a custom database (currently SQLite and Sql Server should be supported) put the connection string in the corresponding
field. Leave empty to use the default database.\
Categories are added in the ``"categories:"`` section. The key value pair should be ``"YourCategoryName": DiscordChannelId``.


### License
I'm not a lawyer but licenses are good I think.
So this thing is licensed under the [**MIT LICENSE**](https://github.com/pron1x/DBuddyBot/blob/master/LICENSE).

---