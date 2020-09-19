using Playnite.Common;
using Playnite.Common.Media.Icons;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Metadata;
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
            public List<SelectableItem<string>> New { get; set; }
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
        private GameDatabase database;
        private List<GameField> diffFields;

        public Game CurrentGame { get; set; }
        public GameMetadata NewGame { get; set; }

        public DiffItem Name { get; } = new DiffItem();
        public DiffItem Description { get; } = new DiffItem();
        public DiffItem CommunityScore { get; } = new DiffItem();
        public DiffItem CriticScore { get; } = new DiffItem();
        public DiffItem ReleaseDate { get; } = new DiffItem();
        public DiffItem AgeRating { get; } = new DiffItem();
        public DiffItem Region { get; } = new DiffItem();
        public DiffItem Series { get; } = new DiffItem();
        public DiffItem Platform { get; } = new DiffItem();
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
        public object NewIcon { get; set; }
        public object CurrentCover { get; set; }
        public object NewCover { get; set; }
        public object CurrentBackground { get; set; }
        public object NewBackground { get; set; }

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

        public GameMetadata ResultMetadata { get; private set; }

        public MetadataComparisonViewModel(
            GameDatabase database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            Game currentData,
            GameMetadata newData,
            List<GameField> diffFields)
        {
            this.database = database;
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

            if (diffFields.Contains(GameField.AgeRating))
            {
                AgeRating.Enabled = true;
            }

            if (diffFields.Contains(GameField.Series))
            {
                Series.Enabled = true;
            }

            if (diffFields.Contains(GameField.Region))
            {
                Region.Enabled = true;
            }

            if (diffFields.Contains(GameField.Platform))
            {
                Platform.Enabled = true;
            }

            if (diffFields.Contains(GameField.Links))
            {
                Links.Enabled = true;
                Links.New = NewGame.GameInfo.Links.Select(a => new SelectableItem<Link>(a) { Selected = true }).ToList();
                if (CurrentGame.Links.HasItems())
                {
                    Links.Current = CurrentGame.Links.Select(a => new SelectableItem<Link>(a)).ToList();
                }
            }

            if (diffFields.Contains(GameField.Developers))
            {
                Developers.Enabled = true;
                Developers.New = NewGame.GameInfo.Developers.Select(a => new SelectableItem<string>(a) { Selected = true }).ToList();
                if (CurrentGame.Developers.HasItems())
                {
                    Developers.Current = CurrentGame.Developers.Select(a => new SelectableItem<Company>(a)).ToList();
                }
            }

            if (diffFields.Contains(GameField.Publishers))
            {
                Publishers.Enabled = true;
                Publishers.New = NewGame.GameInfo.Publishers.Select(a => new SelectableItem<string>(a) { Selected = true }).ToList();
                if (CurrentGame.Publishers.HasItems())
                {
                    Publishers.Current = CurrentGame.Publishers.Select(a => new SelectableItem<Company>(a)).ToList();
                }
            }

            if (diffFields.Contains(GameField.Tags))
            {
                Tags.Enabled = true;
                Tags.New = NewGame.GameInfo.Tags.Select(a => new SelectableItem<string>(a) { Selected = true }).ToList();
                if (CurrentGame.Tags.HasItems())
                {
                    Tags.Current = CurrentGame.Tags.Select(a => new SelectableItem<Tag>(a)).ToList();
                }
            }

            if (diffFields.Contains(GameField.Features))
            {
                Features.Enabled = true;
                Features.New = NewGame.GameInfo.Features.Select(a => new SelectableItem<string>(a) { Selected = true }).ToList();
                if (CurrentGame.Features.HasItems())
                {
                    Features.Current = CurrentGame.Features.Select(a => new SelectableItem<GameFeature>(a)).ToList();
                }
            }

            if (diffFields.Contains(GameField.Genres))
            {
                Genres.Enabled = true;
                Genres.New = NewGame.GameInfo.Genres.Select(a => new SelectableItem<string>(a) { Selected = true }).ToList();
                if (CurrentGame.Genres.HasItems())
                {
                    Genres.Current = CurrentGame.Genres.Select(a => new SelectableItem<Genre>(a)).ToList();
                }
            }

            if (diffFields.Contains(GameField.Icon))
            {
                Icon.Enabled = true;
                CurrentIcon = ImageSourceManager.GetImage(CurrentGame.Icon, false, new BitmapLoadProperties(256, 256));
                NewIcon = ImageSourceManager.GetImage(newData.Icon.OriginalUrl, false, new BitmapLoadProperties(256, 256));
            }

            if (diffFields.Contains(GameField.CoverImage))
            {
                Cover.Enabled = true;
                CurrentCover = ImageSourceManager.GetImage(CurrentGame.CoverImage, false, new BitmapLoadProperties(900, 900));
                NewCover = ImageSourceManager.GetImage(newData.CoverImage.OriginalUrl, false, new BitmapLoadProperties(900, 900));
            }

            if (diffFields.Contains(GameField.BackgroundImage))
            {
                Background.Enabled = true;
                CurrentBackground = ImageSourceManager.GetImage(CurrentGame.BackgroundImage, false, new BitmapLoadProperties(1920, 1080));
                NewBackground = ImageSourceManager.GetImage(newData.BackgroundImage.OriginalUrl, false, new BitmapLoadProperties(1920, 1080));
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
                ResultMetadata.GameInfo.Name = null;
            }

            if (diffFields.Contains(GameField.Description) && Description.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.GameInfo.Description = null;
            }

            if (diffFields.Contains(GameField.CommunityScore) && CommunityScore.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.GameInfo.CommunityScore = null;
            }

            if (diffFields.Contains(GameField.CriticScore) && CriticScore.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.GameInfo.CriticScore = null;
            }

            if (diffFields.Contains(GameField.ReleaseDate) && ReleaseDate.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.GameInfo.ReleaseDate = null;
            }

            if (diffFields.Contains(GameField.AgeRating) && AgeRating.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.GameInfo.AgeRating = null;
            }

            if (diffFields.Contains(GameField.Region) && Region.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.GameInfo.Region = null;
            }

            if (diffFields.Contains(GameField.Series) && Series.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.GameInfo.Series = null;
            }

            if (diffFields.Contains(GameField.Platform) && Platform.Source == MetadataChangeDataSource.Current)
            {
                ResultMetadata.GameInfo.Platform = null;
            }

            if (diffFields.Contains(GameField.Links))
            {
                ResultMetadata.GameInfo.Links = ConsolidateListSources(Links.Current, Links.New);
            }

            if (diffFields.Contains(GameField.Developers))
            {
                ResultMetadata.GameInfo.Developers = ConsolidateListSources(Developers.Current, Developers.New);
            }

            if (diffFields.Contains(GameField.Publishers))
            {
                ResultMetadata.GameInfo.Publishers = ConsolidateListSources(Publishers.Current, Publishers.New);
            }

            if (diffFields.Contains(GameField.Tags))
            {
                ResultMetadata.GameInfo.Tags = ConsolidateListSources(Tags.Current, Tags.New);
            }

            if (diffFields.Contains(GameField.Features))
            {
                ResultMetadata.GameInfo.Features = ConsolidateListSources(Features.Current, Features.New);
            }

            if (diffFields.Contains(GameField.Genres))
            {
                ResultMetadata.GameInfo.Genres = ConsolidateListSources(Genres.Current, Genres.New);
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

        private List<T> ConsolidateListSources<T>(List<SelectableItem<T>> source, List<SelectableItem<T>> other)
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

        private List<string> ConsolidateListSources<T>(List<SelectableItem<T>> source, List<SelectableItem<string>> other) where T : DatabaseObject
        {
            var res = new List<string>();
            if (source.HasItems())
            {
                var srcItems = source.Where(a => a.Selected == true).Select(a => a.Item.Name);
                if (srcItems.HasItems())
                {
                    res.AddRange(srcItems);
                }
            }

            if (other.HasItems())
            {
                var srcItems = other.Where(a => a.Selected == true).Select(a => a.Item);
                {
                    res.AddMissing(srcItems.ToList());
                }
            }

            return res;
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
            Icon.Source = source;
            Cover.Source = source;
            Background.Source = source;
            AgeRating.Source = source;
            Region.Source = source;
            Series.Source = source;
            Platform.Source = source;
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
