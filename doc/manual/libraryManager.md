Library manager
=====================

Introduction
---------------------

Library manager can be accessed from main menu `Library -> Library manager` sub menu. I can be used to manage various game fields shared between games.

Platforms
---------------------

### Custom images

Icon, Cover and Background images assigned to a platform can be used as a replacement images for games that don't have any of those images assigned. Whether platform images will be used can be controlled via application settings in `Appearance -> Advanced` section via `Missing game * source` options.

### Platform specification

Platform specification fields allow you indicate to Playnite how to treat your specific platform based on internal list of built-in platforms. Setting this to correct value will improve field assignments from various automatic actions like game import or metadata import and will also prevent creation of potential platform duplicates.

If you don't see a platform on the list of platform specifications, please [open new GitHub issue](https://github.com/JosefNemec/Playnite/issues) for the platform to be added.

Regions
---------------------

Similar to platform specifications, you can assign region specification from built-in list of regions to improve automatic game and metadata imports.

Completion statuses
---------------------

Completion statuses view offers additional settings used to configure how Playnite handles specific statuses.

`Default status assigned to newly added games`: specify which status will be assigned to newly imported imported games with no play time recorded yet.

`Status assigned to games played for the first time`: specify which status will be assigned to games played for the first time.