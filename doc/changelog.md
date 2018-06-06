#### 1.1.0

* **Breaking Change**: Scripts and Plugins must be place in subfolders rather then directly inside of `Scripts` or `Plugins` folders.
* New: [OnGameStarting](xref:Playnite.SDK.Plugin.OnGameStarting(Playnite.SDK.Models.Game)) event that will execute before game is started. See [events](tutorials/scriptingEvents.md) for use from scripts.
* New: [ShowErrorMessage](xref:Playnite.SDK.IDialogsFactory.ShowErrorMessage(System.String,System.String)) methon in `IDialogsFactory`