using NLog;
using Playnite;
using Playnite.Database;
using Playnite.Models;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Playnite.Providers.Steam;
using Playnite.Providers.GOG;
using Playnite.Providers.Origin;
using System.ComponentModel;

namespace PlayniteUI.ViewModels
{
    public class GameEditViewModel : ObservableObject
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private GameDatabase database;

        #region Field checks

        private bool useNameChanges;
        public bool UseNameChanges
        {
            get
            {
                return useNameChanges;
            }

            set
            {
                useNameChanges = value;
                OnPropertyChanged("UseNameChanges");
            }
        }

        private bool usePlatformChanges;
        public bool UsePlatformChanges
        {
            get
            {
                return usePlatformChanges;
            }

            set
            {
                usePlatformChanges = value;
                OnPropertyChanged("UsePlatformChanges");
            }
        }

        private bool useGenresChanges;
        public bool UseGenresChanges
        {
            get
            {
                return useGenresChanges;
            }

            set
            {
                useGenresChanges = value;
                OnPropertyChanged("UseGenresChanges");
            }
        }

        private bool useReleaseDateChanges;
        public bool UseReleaseDateChanges
        {
            get
            {
                return useReleaseDateChanges;
            }

            set
            {
                useReleaseDateChanges = value;
                OnPropertyChanged("UseReleaseDateChanges");
            }
        }

        private bool useDeveloperChanges;
        public bool UseDeveloperChanges
        {
            get
            {
                return useDeveloperChanges;
            }

            set
            {
                useDeveloperChanges = value;
                OnPropertyChanged("UseDeveloperChanges");
            }
        }

        private bool usePublisherChanges;
        public bool UsePublisherChanges
        {
            get
            {
                return usePublisherChanges;
            }

            set
            {
                usePublisherChanges = value;
                OnPropertyChanged("UsePublisherChanges");
            }
        }

        private bool useCategoryChanges;
        public bool UseCategoryChanges
        {
            get
            {
                return useCategoryChanges;
            }

            set
            {
                useCategoryChanges = value;
                OnPropertyChanged("UseCategoryChanges");
            }
        }

        private bool useDescriptionChanges;
        public bool UseDescriptionChanges
        {
            get
            {
                return useDescriptionChanges;
            }

            set
            {
                useDescriptionChanges = value;
                OnPropertyChanged("UseDescriptionChanges");
            }
        }

        private bool useIconChanges;
        public bool UseIconChanges
        {
            get
            {
                return useIconChanges;
            }

            set
            {
                useIconChanges = value;
                OnPropertyChanged("UseIconChanges");
            }
        }

        private bool useImageChanges;
        public bool UseImageChanges
        {
            get
            {
                return useImageChanges;
            }

            set
            {
                useImageChanges = value;
                OnPropertyChanged("UseImageChanges");
            }
        }

        private bool useInstallDirChanges;
        public bool UseInstallDirChanges
        {
            get
            {
                return useInstallDirChanges;
            }

            set
            {
                useInstallDirChanges = value;
                OnPropertyChanged("UseInstallDirChanges");
            }
        }

        private bool useIsoPathChanges;
        public bool UseIsoPathChanges
        {
            get
            {
                return useIsoPathChanges;
            }

            set
            {
                useIsoPathChanges = value;
                OnPropertyChanged("UseIsoPathChanges");
            }
        }

        private bool useLinksChanges;
        public bool UseLinksChanges
        {
            get
            {
                return useLinksChanges;
            }

            set
            {
                useLinksChanges = value;
                OnPropertyChanged("UseLinksChanges");
            }
        }

        #endregion Field checks

        private IGame editingGame;
        public IGame EditingGame
        {
            get
            {
                return editingGame;
            }

            set
            {
                editingGame = value;
                OnPropertyChanged("EditingGame");
            }
        }

        private IGame game;
        public IGame Game
        {
            get
            {
                return game;
            }

            set
            {
                game = value;
                OnPropertyChanged("Game");
            }
        }

        private IEnumerable<IGame> games;
        public IEnumerable<IGame> Games
        {
            get
            {
                return games;
            }

            set
            {
                games = value;
                OnPropertyChanged("Games");
            }
        }

        private bool progressVisible = false;
        public bool ProgressVisible
        {
            get => progressVisible;
            set
            {
                progressVisible = value;
                OnPropertyChanged("ProgressVisible");
            }
        }

        private bool showActions;
        public bool ShowActions
        {
            get
            {
                return showActions;
            }

            set
            {
                showActions = value;
                OnPropertyChanged("ShowActions");
            }
        }

        private bool showLinks;
        public bool ShowLinks
        {
            get
            {
                return showLinks;
            }

            set
            {
                showLinks = value;
                OnPropertyChanged("ShowLinks");
            }
        }

        private bool showInstallation;
        public bool ShowInstallation
        {
            get
            {
                return showInstallation;
            }

            set
            {
                showInstallation = value;
                OnPropertyChanged("ShowInstallation");
            }
        }

        private bool showMetaDownload;
        public bool ShowMetaDownload
        {
            get
            {
                return showMetaDownload;
            }

            set
            {
                showMetaDownload = value;
                OnPropertyChanged("ShowMetaDownload");
            }
        }

        public List<Platform> Platforms
        {
            get
            {
                var platforms = database.PlatformsCollection.FindAll().OrderBy(a => a.Name).ToList();
                platforms.Insert(0, new Platform(string.Empty));
                return platforms;
            }
        }

        public List<Emulator> Emulators
        {
            get
            {
                if (EditingGame.PlatformId == 0 || EditingGame.PlatformId == null)
                {
                    return database.EmulatorsCollection.FindAll().ToList();
                }
                else
                {
                    return database.EmulatorsCollection.FindAll().Where(a => a.Platforms != null && a.Platforms.Contains(EditingGame.PlatformId.Value)).ToList();
                }
            }
        }

        private bool checkBoxesVisible = false;
        public bool ShowCheckBoxes
        {
            get
            {
                return checkBoxesVisible;
            }

            set
            {
                checkBoxesVisible = value;
                OnPropertyChanged("ShowCheckBoxes");
            }
        }

        public RelayCommand<object> ConfirmCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            });
        }

        public RelayCommand<object> CancelCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CancelDialog();
            });
        }

        public RelayCommand<object> SelectIconCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectIcon();
            });
        }

        public RelayCommand<object> UseExeIconCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                UseExeIcon();
            });
        }

        public RelayCommand<object> SelectCoverCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectCover();
            });
        }

        public RelayCommand<object> SelectCategoryCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var model = new CategoryConfigViewModel(CategoryConfigWindowFactory.Instance, database, EditingGame, true);
                SelectCategories(model);
            });
        }

        public RelayCommand<object> AddLinkCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddLink();
            });
        }

        public RelayCommand<Link> RemoveLinkCommand
        {
            get => new RelayCommand<Link>((link) =>
            {
                RemoveLink(link);
            });
        }

        public RelayCommand<object> SelectInstallDirCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectInstallDir();
            });
        }

        public RelayCommand<object> SelectGameImageCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectGameImage();
            });
        }

        public RelayCommand<object> AddPlayActionCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddPlayAction();
            });
        }

        public RelayCommand<object> DeletePlayActionCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemovePlayAction();
            });
        }

        public RelayCommand<object> AddActionCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddAction();
            });
        }

        public RelayCommand<GameTask> MoveUpActionCommand
        {
            get => new RelayCommand<GameTask>((action) =>
            {
                MoveActionUp(action);
            });
        }

        public RelayCommand<GameTask> MoveDownActionCommand
        {
            get => new RelayCommand<GameTask>((action) =>
            {
                MoveActionDown(action);
            });
        }

        public RelayCommand<GameTask> DeleteActionCommand
        {
            get => new RelayCommand<GameTask>((action) =>
            {
                RemoveAction(action);
            });
        }

        public RelayCommand<object> DownloadIGDBMetadataCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var model = new MetadataLookupViewModel(MetadataProvider.IGDB, MetadataLookupWindowFactory.Instance, dialogs);
                DoMetadataLookup(model);
            });
        }

        public RelayCommand<object> DownloadWikiMetadataCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var model = new MetadataLookupViewModel(MetadataProvider.Wiki, MetadataLookupWindowFactory.Instance, dialogs);
                DoMetadataLookup(model);
            });
        }

        public RelayCommand<object> DownloadStoreCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                DownloadStoreData();
            });
        }

        public GameEditViewModel(IGame game, GameDatabase database, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            
            Game = game;
            EditingGame = (game as Game).CloneJson();
            ShowCheckBoxes = false;
            ShowMetaDownload = true;
            ShowLinks = true;
            ShowActions = true;
            ShowInstallation = Game.Provider == Provider.Custom;
            EditingGame.PropertyChanged += EditingGame_PropertyChanged;

        }

        public GameEditViewModel(IEnumerable<IGame> games, GameDatabase database, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;

            Games = games;
            var previewGame = GameHandler.GetMultiGameEditObject(games);
            EditingGame = previewGame;
            ShowCheckBoxes = true;
            ShowMetaDownload = false;
            ShowLinks = false;
            ShowActions = false;
            ShowInstallation = false;
            EditingGame.PropertyChanged += EditingGame_PropertyChanged;
        }

        private void EditingGame_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PlatformId")
            {
                OnPropertyChanged("Emulators");
            }

            switch (e.PropertyName)
            {
                case "Name":
                    UseNameChanges = true;
                    break;
                case "PlatformId":
                    UsePlatformChanges = true;
                    break;
                case "Image":
                    UseImageChanges = true;
                    break;
                case "Icon":
                    UseIconChanges = true;
                    break;
                case "Links":
                    UseLinksChanges = true;
                    break;
                case "InstallDirectory":
                    UseInstallDirChanges = true;
                    break;
                case "IsoPath":
                    UseIsoPathChanges = true;
                    break;
                case "Description":
                    UseDescriptionChanges = true;
                    break;
                case "Categories":
                    UseCategoryChanges = true;
                    break;
                case "Genres":
                    UseGenresChanges = true;
                    break;
                case "ReleaseDate":
                    UseReleaseDateChanges = true;
                    break;
                case "Developers":
                    UseDeveloperChanges = true;
                    break;
                case "Publishers":
                    UsePublisherChanges = true;
                    break;
            }
        }

        public bool? ShowDialog()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CancelDialog()
        {
            window.Close(false);
        }

        public void ConfirmDialog()
        {
            if (Games == null)
            {
                if (string.IsNullOrWhiteSpace(EditingGame.Name))
                {
                    dialogs.ShowMessage("Name cannot be empty.", "Invalid game data", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (UseNameChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Name = Game.Name;
                    }
                }
                else
                {
                    Game.Name = EditingGame.Name;
                }
            }

            if (UseGenresChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Genres = Game.Genres;
                    }
                }
                else
                {
                    Game.Genres = EditingGame.Genres;
                }
            }

            if (UseReleaseDateChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.ReleaseDate = Game.ReleaseDate;
                    }
                }
                else
                {
                    Game.ReleaseDate = EditingGame.ReleaseDate;
                }
            }

            if (UseDeveloperChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Developers = Game.Developers;
                    }
                }
                else
                {
                    Game.Developers = EditingGame.Developers;
                }
            }

            if (UsePublisherChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Publishers = Game.Publishers;
                    }
                }
                else
                {
                    Game.Publishers = EditingGame.Publishers;
                }
            }

            if (UseCategoryChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Categories = Game.Categories;
                    }
                }
                else
                {
                    Game.Categories = EditingGame.Categories;                        
                }
            }

            if (UseDescriptionChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Description = Game.Description;
                    }
                }
                else
                {
                    Game.Description = EditingGame.Description;
                }
            }

            if (Game.InstallDirectory != EditingGame.InstallDirectory)
            {
                Game.InstallDirectory = EditingGame.InstallDirectory;
            }

            if (Game.IsoPath != EditingGame.IsoPath)
            {
                Game.IsoPath = EditingGame.IsoPath;
            }

            if (UsePlatformChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.PlatformId = Game.PlatformId;
                    }
                }
                else
                {
                    Game.PlatformId = EditingGame.PlatformId;
                }
            }

            if (UseIconChanges)
            {
                var iconPath = EditingGame.Icon;
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(iconPath);
                var iconId = "images/custom/" + fileName;
                database.AddImage(iconId, fileName, File.ReadAllBytes(iconPath));

                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        if (!string.IsNullOrEmpty(game.Icon))
                        {
                            database.DeleteImageSafe(game.Icon, game);
                        }

                        game.Icon = iconId;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Game.Icon))
                    {
                        database.DeleteImageSafe(Game.Icon, Game);
                    }

                    Game.Icon = iconId;
                }

                if (Path.GetDirectoryName(iconPath) == Paths.TempPath)
                {
                    File.Delete(iconPath);
                }
            }

            if (UseImageChanges)
            {
                var imagePath = EditingGame.Image;
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagePath);
                var imageId = "images/custom/" + fileName;
                database.AddImage(imageId, fileName, File.ReadAllBytes(imagePath));

                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        if (!string.IsNullOrEmpty(game.Image))
                        {
                            database.DeleteImageSafe(game.Image, game);
                        }

                        game.Image = imageId;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Game.Image))
                    {
                        database.DeleteImageSafe(Game.Image, Game);
                    }

                    Game.Image = imageId;
                }

                if (Path.GetDirectoryName(imagePath) == Paths.TempPath)
                {
                    File.Delete(imagePath);
                }
            }

            if (Games == null)
            {
                if (!Game.PlayTask.IsEqualJson(EditingGame.PlayTask))
                {
                    Game.PlayTask = EditingGame.PlayTask;
                }

                if (!Game.OtherTasks.IsEqualJson(EditingGame.OtherTasks))
                {
                    Game.OtherTasks = EditingGame.OtherTasks;
                }

                if (!Game.Links.IsEqualJson(EditingGame.Links) && UseLinksChanges)
                {
                    Game.Links = EditingGame.Links;
                }
            }

            if (Games != null)
            {
                foreach (var game in Games)
                {
                    database.UpdateGameInDatabase(game);
                }
            }
            else
            {
                database.UpdateGameInDatabase(Game);
            }

            window.Close(true);
        }

        public void PreviewGameData(IGame game)
        {
            var listConverter = new ListToStringConverter();
            var dateConverter = new NullableDateToStringConverter();

            if (!string.IsNullOrEmpty(game.Name))
            {
                EditingGame.Name = game.Name;
            }

            if (game.Developers != null && game.Developers.Count != 0)
            {
                EditingGame.Developers = game.Developers;
            }

            if (game.Publishers != null && game.Publishers.Count != 0)
            {
                EditingGame.Publishers = game.Publishers;
            }

            if (game.Genres != null && game.Genres.Count != 0)
            {
                EditingGame.Genres = game.Genres;
            }

            if (game.ReleaseDate != null)
            {
                EditingGame.ReleaseDate = game.ReleaseDate;
            }

            if (!string.IsNullOrEmpty(game.Description))
            {
                EditingGame.Description = game.Description;
            }

            if (game.Links != null)
            {
                EditingGame.Links = game.Links;
            }

            if (!string.IsNullOrEmpty(game.Image))
            {
                var extension = Path.GetExtension(game.Image);
                var tempPath = Path.Combine(Paths.TempPath, "tempimage" + extension);
                FileSystem.PrepareSaveFile(tempPath);
                Web.DownloadFile(game.Image, tempPath);
                EditingGame.Image = tempPath;
            }
        }

        private string SaveFileIconToTemp(string exePath)
        {
            var ico = IconExtension.ExtractIconFromExe(exePath, true);
            if (ico == null)
            {
                return string.Empty;
            }

            var tempPath = Path.Combine(Paths.TempPath, "tempico.png");
            if (ico != null)
            {
                FileSystem.PrepareSaveFile(tempPath);
                ico.ToBitmap().Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
            }

            return tempPath;
        }

        public void SelectIcon()
        {
            var path = dialogs.SelectIconFile();
            if (!string.IsNullOrEmpty(path))
            {
                if (path.EndsWith("exe", StringComparison.CurrentCultureIgnoreCase))
                {
                    path = SaveFileIconToTemp(path);
                    if (string.IsNullOrEmpty(path))
                    {
                        return;
                    }
                }

                EditingGame.Icon = path;
            }
        }

        public void UseExeIcon()
        {
            if (EditingGame.PlayTask == null || EditingGame.PlayTask.Type == GameTaskType.URL)
            {
                dialogs.ShowMessage(resources.FindString("ExecIconMissingPlayAction"));
                return;
            }

            var path = (EditingGame as Game).ResolveVariables(EditingGame.PlayTask.Path);
            var icon = SaveFileIconToTemp(path);
            if (string.IsNullOrEmpty(icon))
            {
                return;
            }

            EditingGame.Icon = icon;
        }

        public void SelectCover()
        {
            var path = dialogs.SelectImagefile();
            if (!string.IsNullOrEmpty(path))
            {
                EditingGame.Image = path;
            }
        }

        public void AddPlayAction()
        {
            EditingGame.PlayTask = new GameTask()
            {
                Name = "Play",
                IsBuiltIn = false
            };
        }

        public void RemovePlayAction()
        {
            EditingGame.PlayTask = null;
        }

        public void AddAction()
        {
            if (EditingGame.OtherTasks == null)
            {
                EditingGame.OtherTasks = new ObservableCollection<GameTask>();
            }

            var newTask = new GameTask()
            {
                Name = "New Action",
                IsBuiltIn = false
            };

            if (EditingGame.PlayTask != null && EditingGame.PlayTask.Type == GameTaskType.File)
            {
                newTask.WorkingDir = EditingGame.PlayTask.WorkingDir;
                newTask.Path = EditingGame.PlayTask.Path;
            }

            EditingGame.OtherTasks.Add(newTask);
        }

        public void RemoveAction(GameTask action)
        {
            EditingGame.OtherTasks.Remove(action);
        }

        public void MoveActionUp(GameTask action)
        {
            var index = EditingGame.OtherTasks.IndexOf(action);
            if (index != 0)
            {
                EditingGame.OtherTasks.Move(index, index - 1);
            }
        }

        public void MoveActionDown(GameTask action)
        {
            var index = EditingGame.OtherTasks.IndexOf(action);
            if (index != EditingGame.OtherTasks.Count - 1)
            {
                EditingGame.OtherTasks.Move(index, index + 1);
            }
        }

        public void AddLink()
        {
            if (EditingGame.Links == null)
            {
                EditingGame.Links = new ObservableCollection<Link>();
            }

            EditingGame.Links.Add(new Link("NewLink", "NewUrl"));
        }

        public void RemoveLink(Link link)
        {
            EditingGame.Links.Remove(link);
        }

        public void SelectCategories(CategoryConfigViewModel model)
        {
            model.ShowDialog();
        }

        public void SelectInstallDir()
        {
            var path = dialogs.SelectFolder();
            if (!string.IsNullOrEmpty(path))
            {
                EditingGame.InstallDirectory = path;
            }
        }

        public void SelectGameImage()
        {
            var path = dialogs.SelectFile("*.*|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                EditingGame.IsoPath = path;
            }
        }

        public void DoMetadataLookup(MetadataLookupViewModel model)
        {
            if (string.IsNullOrEmpty(EditingGame.Name))
            {
                dialogs.ShowMessage("Game name cannot be empty before searching metadata.", "", MessageBoxButton.OK);
                return;
            }
            
            model.SearchTerm = Game.Name;
            if (model.ShowDialog() == true)
            {
                PreviewGameData(model.MetadataData);
                ShowCheckBoxes = true;
            }
        }       

        public async void DownloadStoreData()
        {
            ProgressVisible = true;

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    GameMetadata metadata;
                    var tempGame = (game as Game).CloneJson();
                    tempGame.Image = string.Empty;

                    switch (tempGame.Provider)
                    {
                        case Provider.Steam:
                            metadata = (new SteamLibrary()).UpdateGameWithMetadata(tempGame);
                            break;
                        case Provider.GOG:
                            metadata = (new GogLibrary()).UpdateGameWithMetadata(tempGame);
                            break;
                        case Provider.Origin:
                            metadata = (new OriginLibrary()).UpdateGameWithMetadata(tempGame);
                            break;
                        case Provider.Uplay:
                            return;
                        case Provider.Custom:
                        default:
                            return;
                    }

                    PreviewGameData(tempGame);
                    ShowCheckBoxes = true;
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, "Failed to download metadata, {0} , {1}", game.Provider, game.ProviderId);
                    dialogs.ShowMessage("Failed to download metadata: " + exc.Message, "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    ProgressVisible = false;
                }
            });
        }
    }
}
