Metadata Plugins
=====================

To implement metadata plugin:

* Read the introduction to [extensions](intro.md) and [plugins](plugins.md).
* Create new public class inheriting from [MetadataPlugin](xref:Playnite.SDK.Plugins.MetadataPlugin) abstract class.
* Add implementation for mandatory abstract members.

Mandatory members
---------------------

| Member | Description |
| -- | -- |
| Id | Unique plugin id. |
| Name | Provider name. |
| SupportedFields | Returns list of metadata fields this plugin can generally provide. |
| GetMetadataProvider | Returns metadata provider class for specific metadata request. |


OnDemandMetadataProvider
---------------------

[OnDemandMetadataProvider](xref:Playnite.SDK.Plugins.OnDemandMetadataProvider) is an object to be returned from `GetMetadataProvider` method. Playnite uses it to request specific metadata fields based on user's metadata download settings. Override specific `Get*` methods based on what your plugin can provide. The class also contains another implementation of `AvailableFields` property, which should return list available fields for specific request.

`OnDemandMetadataProvider` is `IDisposable` and object is disposed automatically once metadata are processed completely for a single game.

MetadataRequestOptions
---------------------
[MetadataRequestOptions](xref:Playnite.SDK.Plugins.MetadataRequestOptions) is passed into every `GetMetadataProvider` request for each specific game. Most important fields are `IsBackgroundDownload` and `GameData`.

- `IsBackgroundDownload` indicates whether the download is being processed in background. By either automatically started download for newly imported games or manually initiated bulk download. When set to false it indicates that download is for manually initiated request from game edit dialog.

- `GameData` contains game that should be updated we new metadata.

MetadataProperty
---------------------

Some fields like `Genres`, `Tags` and others that are globally stored as static values and shared between games are referenced by metadata sources via `MetadataProperty`. Based on what type is used to reference metadata, Playnite will use different method to assign final data.

Available property types:

| Type | Description |
| --- | --- |
| MetadataNameProperty | Playnite will assign field value based on a name (Playnite 8 behavior). If fields with the same name exists in game library, it's reused. |
| MetadataIdProperty | Playnite will assign field value based on objects database ID. Use if you want to reference specific object already existing in the game library. |
| MetadataSpecProperty | Playnite will assign filed value based on specification identifier. Currently only available for [Platforms](https://github.com/JosefNemec/Playnite/blob/devel/source/Playnite/Emulation/Platforms.yaml) and [Regions](https://github.com/JosefNemec/Playnite/blob/devel/source/Playnite/Emulation/Regions.yaml). Identifier matches based on an ID or a Name. |

> [!NOTE]
> `MetadataIdProperty` should be used **only** when directly referencing existing items from game library. Do not add metadata fields into the library manually and then reference those added items via `MetadataIdProperty`. In majority of cases only `MetadataNameProperty` or `MetadataSpecProperty` should be used. Playnite will automatically add new items to the library when specified items don't exist and will automatically references existing items if specified items already exist, including handling of duplicates and indirect duplicates like "single player" vs "single-player".

Example plugin
---------------------

Playnite's IGDB integration is implemented as a metadata plugin. You can see the source [on GitHub](https://github.com/JosefNemec/PlayniteExtensions/tree/master/source/Metadata).