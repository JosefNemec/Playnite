Playtime Import
=====================

This setting configures when should Playnite import the playtime reported by library plugins for games in the Playnite database.

> [!WARNING] 
> Please be aware that support by the library plugins in charge of handling the game(s) is needed to be able to use this feature, as only library plugins that internally obtain the playtime from their respective service will allow Playnite to import it. Also while how plugins obtain this data differs, this usually requires that the user is logged in or/and configures a key in the specific plugin settings page.

The following modes are available for this setting:
 
`Always`: Imports playtime for new imported games added during library update. and existing games in Playnite database.

`Only for newly imported games`: Imports playtime only for new imported games added during library update.

`Never`: Never imports playtime under any circumstance.