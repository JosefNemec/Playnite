using Playnite.Common;
using Playnite.Common.Media.Icons;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.ViewModels
{
    public enum MetadataChangeDataSource
    {
        Current,
        New
    }

    public class MetadataComparisonViewModel : ObservableObject
    {
        public class DiffItem : ObservableObject
        {
            private bool enabled = false;
            public bool Enabled
            {
                get
                {
                    return enabled;
                }

                set
                {
                    enabled = value;
                    OnPropertyChanged();
                }
            }

            private MetadataChangeDataSource source = MetadataChangeDataSource.New;
            public MetadataChangeDataSource Source
            {
                get
                {
                    return source;
                }

                set
                {
                    source = value;
                    OnPropertyChanged();
                }
            }
        }

        public class ListDiffItem<T> : DiffItem where T : DatabaseObject
        {
            public List<SelectableItem<T>> Current { get; set; }
            public List<SelectableItem<T>> New { get; set; }
        }

        public class ListObjectItem<T> : DiffItem
        {
            public List<SelectableItem<T>> Current { get; set; }
            public List<SelectableItem<T>> New { get; set; }
        }

        private static readonly ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private List<GameField> diffFields;

        public Game CurrentGame { get; set; }
        public ComparableMetadatGameData NewGame { get; set; }

        public DiffItem Name { get; } = new DiffItem();
        public DiffItem Description { get; } = new DiffItem();
        public DiffItem CommunityScore { get; } = new DiffItem();
        public DiffItem CriticScore { get; } = new DiffItem();
        public DiffItem ReleaseDate { get; } = new DiffItem();
        public DiffItem InstallSize { get; } = new DiffItem();
        public ListDiffItem<AgeRating> AgeRatings { get; } = new ListDiffItem<AgeRating>();
        public ListDiffItem<Region> Regions { get; } = new ListDiffItem<Region>();
        public ListDiffItem<Series> Series { get; } = new ListDiffItem<Series>();
        public ListDiffItem<Platform> Platforms { get; } = new ListDiffItem<Platform>();
        public ListDiffItem<Genre> Genres { get; } = new ListDiffItem<Genre>();
        public ListDiffItem<Tag> Tags { get; } = new ListDiffItem<Tag>();
        public ListDiffItem<Company> Developers { get; } = new ListDiffItem<Company>();
        public ListDiffItem<Company> Publishers { get; } = new ListDiffItem<Company>();
        public ListDiffItem<GameFeature> Features { get; } = new ListDiffItem<GameFeature>();
        public ListObjectItem<Link> Links { get; } = new ListObjectItem<Link>();
        public DiffItem Icon { get; } = new DiffItem();
        public DiffItem Cover { get; } = new DiffItem();
        public DiffItem Background { get; } = new DiffItem();

        public object CurrentIcon { get; set; }
        public string CurrentIconDimensions { get; set; }
        public object NewIcon { get; set; }
        public string NewIconDimensions { get; set; }
        public object CurrentCover { get; set; }
        public string CurrentCoverDimensions { get; set; }
        public object NewCover { get; set; }
        public string NewCoverDimensions { get; set; }
        public object CurrentBackground { get; set; }
        public string CurrentBackgroundDimensions { get; set; }
        public object NewBackground { get; set; }
        public string NewBackgroundDimensions { get; set; }

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
                CloseView();
            });
        }

        public RelayCommand<object> SelectAllCurrentCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectAllCurrent();
            });
        }

        public RelayCommand<object> SelectAllNewCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectAllNew();
            });
        }

        public ComparableMetadatGameData ResultMetadata { get; private set; }

        public MetadataComparisonViewModel(
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            Game currentData,
            ComparableMetadatGameData newData,
            List<GameField> diffFields)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.diffFields = diffFields;
            CurrentGame = currentData;
            NewGame = newData;

            if (diffFields.Contains(GameField.Name))
            {
                Name.Enabled = true;
            }

            if (diffFields.Contains(GameField.Description))
            {
                Description.Enabled = true;
            }

            if (diffFields.Contains(GameField.ReleaseDate))
            {
                ReleaseDate.Enabled = true;
            }

            if (diffFields.Contains(GameField.CriticScore))
            {
                CriticScore.Enabled = true;
            }

            if (diffFields.Contains(GameField.CommunityScore))
            {
                CommunityScore.Enabled = true;
            }

            if (diffFields.Contains(GameField.InstallSize))
            {
                InstallSize.Enabled = true;
            }

            void loadNewListData<T>(ListDiffItem<T> list, List<T> currentGameData, List<T> newGameData, GameField field) where T : DatabaseObject
            {
                if (diffFields.Contains(field))
                {
                    list.Enabled = true;
                    list.New = newGameData.Select(a => new SelectableItem<T>(a) { Selected = true }).ToList();
                    if (currentGameData.HasItems())
                    {
                        list.Current = currentGameData.Select(a => new SelectableItem<T>(a)).ToList();
                    }
                }
            }

            loadNewListData(AgeRatings, CurrentGame.AgeRatings, NewGame.AgeRatings, GameField.AgeRatings);
            loadNewListData(Series, CurrentGame.Series, NewGame.Series, GameField.Series);
            loadNewListData(Regions, CurrentGame.Regions, NewGame.Regions, GameField.Regions);
            loadNewListData(Platforms, CurrentGame.Platforms, NewGame.Platforms, GameField.Platforms);
            loadNewListData(Developers, CurrentGame.Developers, NewGame.Developers, GameField.Developers);
            loadNewListData(Publishers, CurrentGame.Publishers, NewGame.Publishers, GameField.Publishers);
            loadNewListData(Tags, CurrentGame.Tags, NewGame.Tags, GameField.Tags);
            loadNewListData(Features, CurrentGame.Features, NewGame.Features, GameField.Features);
            loadNewListData(Genres, CurrentGame.Genres, NewGame.Genres, GameField.Genres);

            if (diffFields.Contains(GameField.Links))
            {
                Links.Enabled = true;
                Links.New = NewGame.Links.Select(a => new SelectableItem<Link>(a) { Selected = true }).ToList();
                if (CurrentGame.Links.HasItems())
                {
                    Links.Current = CurrentGame.Links.Select(a => new SelectableItem<Link>(a)).ToList();
                }
            }

            if (diffFields.Contains(GameField.Icon))
            {
                Icon.Enabled = true;
                CurrentIcon = ImageSourceManager.GetImage(CurrentGame.Icon, false, new BitmapLoadProperties(256, 256));
                CurrentIconDimensions = GetImageProperties(CurrentGame.Icon)?.Item1;
                NewIcon = ImageSourceManager.GetImage(newData.Icon.Path, false, new BitmapLoadProperties(256, 256));
                NewIconDimensions = GetImageProperties(newData.Icon.Path)?.Item1;
            }

            if (diffFields.Contains(GameField.CoverImage))
            {
                Cover.Enabled = true;
                CurrentCover = ImageSourceManager.GetImage(CurrentGame.CoverImage, false, new BitmapLoadProperties(900, 900));
                CurrentCoverDimensions = GetImageProperties(CurrentGame.CoverImage)?.Item1;
                NewCover = ImageSourceManager.GetImage(newData.CoverImage.Path, false, new BitmapLoadProperties(900, 900));
                NewCoverDimensions = GetImageProperties(newData.CoverImage.Path)?.Item1;
            }

            if (diffFields.Contains(GameField.BackgroundImage))
            {
                Background.Enabled = true;
                CurrentBackground = ImageSourceManager.GetImage(CurrentGame.BackgroundImage, false, new BitmapLoadProperties(1920, 1080));
                CurrentBackgroundDimensions = GetImageProperties(CurrentGame.BackgroundImage)?.Item1;
                NewBackground = ImageSourceManager.GetImage(newData.BackgroundImage.Path, false, new BitmapLoadProperties(1920, 1080));
                NewBackgroundDimensions = GetImageProperties(newData.BackgroundImage.Path)?.Item1;
            }
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView()
        {
            window.Close(false);
        }

        public void ConfirmDialog()
        {
            ResultMetadata = NewGame.GetClone();

            if (diffFields.Contains(GameField.Name) && Name.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.Name = null;
            }

            if (diffFields.Contains(GameField.Description) && Description.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.Description = null;
            }

            if (diffFields.Contains(GameField.CommunityScore) && CommunityScore.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.CommunityScore = null;
            }

            if (diffFields.Contains(GameField.CriticScore) && CriticScore.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.CriticScore = null;
            }

            if (diffFields.Contains(GameField.ReleaseDate) && ReleaseDate.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.ReleaseDate = null;
            }

            if (diffFields.Contains(GameField.InstallSize) && InstallSize.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.InstallSize = null;
            }

            if (diffFields.Contains(GameField.AgeRatings))
            {
                ResultMetadata.AgeRatings = ConsolidateDbListSources(AgeRatings.Current, AgeRatings.New);
            }

            if (diffFields.Contains(GameField.Regions))
            {
                ResultMetadata.Regions = ConsolidateDbListSources(Regions.Current, Regions.New);
            }

            if (diffFields.Contains(GameField.Series))
            {
                ResultMetadata.Series = ConsolidateDbListSources(Series.Current, Series.New);
            }

            if (diffFields.Contains(GameField.Platforms))
            {
                ResultMetadata.Platforms = ConsolidateDbListSources(Platforms.Current, Platforms.New);
            }

            if (diffFields.Contains(GameField.Links))
            {
                ResultMetadata.Links = ConsolidateGenericListSources(Links.Current, Links.New);
            }

            if (diffFields.Contains(GameField.Developers))
            {
                ResultMetadata.Developers = ConsolidateDbListSources(Developers.Current, Developers.New);
            }

            if (diffFields.Contains(GameField.Publishers))
            {
                ResultMetadata.Publishers = ConsolidateDbListSources(Publishers.Current, Publishers.New);
            }

            if (diffFields.Contains(GameField.Tags))
            {
                ResultMetadata.Tags = ConsolidateDbListSources(Tags.Current, Tags.New);
            }

            if (diffFields.Contains(GameField.Features))
            {
                ResultMetadata.Features = ConsolidateDbListSources(Features.Current, Features.New);
            }

            if (diffFields.Contains(GameField.Genres))
            {
                ResultMetadata.Genres = ConsolidateDbListSources(Genres.Current, Genres.New);
            }

            if (diffFields.Contains(GameField.Icon) && Icon.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.Icon = null;
            }

            if (diffFields.Contains(GameField.CoverImage) && Cover.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.CoverImage = null;
            }

            if (diffFields.Contains(GameField.BackgroundImage) && Background.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.BackgroundImage = null;
            }

            window.Close(true);
        }

        private List<T> ConsolidateGenericListSources<T>(List<SelectableItem<T>> source, List<SelectableItem<T>> other)
        {
            var res = new List<T>();
            if (source.HasItems())
            {
                var srcItems = source.Where(a => a.Selected == true).Select(a => a.Item);
                if (srcItems.HasItems())
                {
                    res.AddRange(srcItems);
                }
            }

            if (other.HasItems())
            {
                var srcItems = other.Where(a => a.Selected == true).Select(a => a.Item);
                if (srcItems.HasItems())
                {
                    res.AddRange(srcItems.ToList());
                }
            }

            return res;
        }

        private List<T> ConsolidateDbListSources<T>(List<SelectableItem<T>> source, List<SelectableItem<T>> other) where T : DatabaseObject
        {
            var res = new List<T>();
            if (source.HasItems())
            {
                var srcItems = source.Where(a => a.Selected == true).Select(a => a.Item);
                if (srcItems.HasItems())
                {
                    res.AddRange(srcItems);
                }
            }

            if (other.HasItems())
            {
                var srcItems = other.Where(a => a.Selected == true).Select(a => a.Item);
                if (srcItems.HasItems())
                {
                    res.AddMissing(srcItems.ToList());
                }
            }

            return res;
        }

        private Tuple<string, ImageProperties> GetImageProperties(string image)
        {
            try
            {
                var imagePath = ImageSourceManager.GetImagePath(image);
                if (!imagePath.IsNullOrEmpty())
                {
                    var props = Images.GetImageProperties(imagePath);
                    return new Tuple<string, ImageProperties>($"{props?.Width}x{props.Height}px", props);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to get metadata from image  {image}");
                return null;
            }
        }

        private void SelectAllCurrent()
        {
            SetSelection(MetadataChangeDataSource.Current);
        }

        private void SelectAllNew()
        {
            SetSelection(MetadataChangeDataSource.New);
        }

        private void SetSelection(MetadataChangeDataSource source)
        {
            Name.Source = source;
            Description.Source = source;
            CommunityScore.Source = source;
            CriticScore.Source = source;
            ReleaseDate.Source = source;
            InstallSize.Source = source;
            Icon.Source = source;
            Cover.Source = source;
            Background.Source = source;
            AgeRatings.Current?.ForEach(a => a.Selected = source == MetadataChangeDataSource.Current);
            AgeRatings.New?.ForEach(a => a.Selected = source == MetadataChangeDataSource.New);
            Regions.Current?.ForEach(a => a.Selected = source == MetadataChangeDataSource.Current);
            Regions.New?.ForEach(a => a.Selected = source == MetadataChangeDataSource.New);
            Series.Current?.ForEach(a => a.Selected = source == MetadataChangeDataSource.Current);
            Series.New?.ForEach(a => a.Selected = source == MetadataChangeDataSource.New);
            Platforms.Current?.ForEach(a => a.Selected = source == MetadataChangeDataSource.Current);
            Platforms.New?.ForEach(a => a.Selected = source == MetadataChangeDataSource.New);
            Genres.Current?.ForEach(a => a.Selected = source == MetadataChangeDataSource.Current);
            Genres.New?.ForEach(a => a.Selected = source == MetadataChangeDataSource.New);
            Tags.Current?.ForEach(a => a.Selected = source == MetadataChangeDataSource.Current);
            Tags.New?.ForEach(a => a.Selected = source == MetadataChangeDataSource.New);
            Developers.Current?.ForEach(a => a.Selected = source == MetadataChangeDataSource.Current);
            Developers.New?.ForEach(a => a.Selected = source == MetadataChangeDataSource.New);
            Publishers.Current?.ForEach(a => a.Selected = source == MetadataChangeDataSource.Current);
            Publishers.New?.ForEach(a => a.Selected = source == MetadataChangeDataSource.New);
            Features.Current?.ForEach(a => a.Selected = source == MetadataChangeDataSource.Current);
            Features.New?.ForEach(a => a.Selected = source == MetadataChangeDataSource.New);
            Links.Current?.ForEach(a => a.Selected = source == MetadataChangeDataSource.Current);
            Links.New?.ForEach(a => a.Selected = source == MetadataChangeDataSource.New);
        }
    }
}