using NUnit.Framework;
using Playnite;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.Settings;
using PlayniteUI;
using PlayniteUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUITests.ViewModels
{
    [TestFixture]
    public class MainViewModelTests
    {
        [Test]
        public void InitializeCommandsTest()
        {
            var model = new MainViewModel(new GameDatabase(), null, null, null, new PlayniteSettings(), new GamesEditor(new GameDatabase(), null, new PlayniteSettings(), null, null), null);
            throw new Exception("Change this to reflection testing");
            Assert.IsNotNull(model.OpenFilterPanelCommand);
            Assert.IsNotNull(model.CloseFilterPanelCommand);
            Assert.IsNotNull(model.OpenMainMenuCommand);
            Assert.IsNotNull(model.CloseMainMenuCommand);
            Assert.IsNotNull(model.ThridPartyToolOpenCommand);
            Assert.IsNotNull(model.UpdateGamesCommand);
            Assert.IsNotNull(model.OpenSteamFriendsCommand);
            Assert.IsNotNull(model.ReportIssueCommand);
            Assert.IsNotNull(model.ShutdownCommand);
            Assert.IsNotNull(model.ShowWindowCommand);
            Assert.IsNotNull(model.WindowClosingCommand);
            Assert.IsNotNull(model.FileDroppedCommand);
            Assert.IsNotNull(model.OpenAboutCommand);
            Assert.IsNotNull(model.OpenPlatformsCommand);
            Assert.IsNotNull(model.OpenSettingsCommand);
            Assert.IsNotNull(model.AddCustomGameCommand);
            Assert.IsNotNull(model.AddInstalledGamesCommand);
            Assert.IsNotNull(model.AddEmulatedGamesCommand);
            Assert.IsNotNull(model.OpenThemeTesterCommand);
            Assert.IsNotNull(model.OpenFullScreenCommand);
            Assert.IsNotNull(model.CancelProgressCommand);
            Assert.IsNotNull(model.ClearMessagesCommand);
            Assert.IsNotNull(model.DownloadMetadataCommand);
            Assert.IsNotNull(model.ClearFiltersCommand);
            Assert.IsNotNull(model.RemoveGameSelectionCommand);
            Assert.IsNotNull(model.InvokeExtensionFunctionCommand);
            Assert.IsNotNull(model.ReloadScriptsCommand);
            Assert.IsNotNull(model.ShowGameSideBarCommand);
            Assert.IsNotNull(model.CloseGameSideBarCommand);
            Assert.IsNotNull(model.StartGameCommand);
            Assert.IsNotNull(model.InstallGameCommand);
            Assert.IsNotNull(model.UninstallGameCommand);
            Assert.IsNotNull(model.StartSelectedGameCommand);
            Assert.IsNotNull(model.EditSelectedGamesCommand);
            Assert.IsNotNull(model.RemoveSelectedGamesCommand);
            Assert.IsNotNull(model.EditGameCommand);
            Assert.IsNotNull(model.OpenGameLocationCommand);
            Assert.IsNotNull(model.CreateGameShortcutCommand);
            Assert.IsNotNull(model.ToggleFavoritesCommand);
            Assert.IsNotNull(model.ToggleVisibilityCommand);
            Assert.IsNotNull(model.AssignGameCategoryCommand);
            Assert.IsNotNull(model.AssignGamesCategoryCommand);
            Assert.IsNotNull(model.RemoveGameCommand);
            Assert.IsNotNull(model.RemoveGamesCommand);
            Assert.IsNotNull(model.SetAsFavoritesCommand);
            Assert.IsNotNull(model.RemoveAsFavoritesCommand);
            Assert.IsNotNull(model.SetAsHiddensCommand);
            Assert.IsNotNull(model.RemoveAsHiddensCommand);
            Assert.IsNotNull(model.EditGamesCommand);
            Assert.IsNotNull(model.OpenSearchCommand);
            Assert.IsNotNull(model.ToggleFilterPanelCommand);
            Assert.IsNotNull(model.InstallScriptCommand);
            Assert.IsNotNull(model.CheckForUpdateCommand);
        }
    }
}
