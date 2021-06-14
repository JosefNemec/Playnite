Metadata Plugins
=====================

> [!NOTE]
> Playnite 6.0 and newer is required for custom metadata support.

To implement metadata plugin:

* Read the introduction to [extensions](../intro.md) and [plugins](plugins.md).
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

`OnDemandMetadataProvider` is `IDisposable` and object is disposed automatically once metadata are processes completely for a single game.

MetadataRequestOptions
---------------------
[MetadataRequestOptions](xref:Playnite.SDK.Plugins.MetadataRequestOptions) is passed into every `GetMetadataProvider` request for each specific game. Most important fields are `IsBackgroundDownload` and `GameData`.

- `IsBackgroundDownload` indicates whether the download is being processed in background. By either automatically started download for newly imported games or manually initiated bulk download. When set to false it indicates that download is for manually initiated request from game edit dialog.

- `GameData` contains game that should be updated we new metadata.

Example plugin
---------------------

Playnite's IGDB integration is implemented as a metadata plugin. You can see the source [on GitHub](https://github.com/JosefNemec/Playnite/tree/devel/source/Plugins/IGDBMetadata).