using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.DesktopApp.ViewModels
{
    public partial class GameEditViewModel
    {
        public RelayCommand<object> AddPlatformCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewPlatform();
            });
        }

        public RelayCommand<object> AddSeriesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewSeries();
            });
        }

        public RelayCommand<object> AddAgeRatingCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewAgeRating();
            });
        }

        public RelayCommand<object> AddRegionCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewRegion();
            });
        }

        public RelayCommand<object> AddSourceCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewSource();
            });
        }

        public RelayCommand<object> AddCategoryCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewCategory();
            });
        }

        public RelayCommand<object> AddPublisherCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewPublisher();
            });
        }

        public RelayCommand<object> AddDeveloperCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewDeveloper();
            });
        }

        public RelayCommand<object> AddGenreCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewGenre();
            });
        }

        public RelayCommand<object> AddTagCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewTag();
            });
        }

        public RelayCommand<object> AddFeatureCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewFeature();
            });
        }

        public RelayCommand<object> AddCompletionStatusCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewCompletionStatus();
            });
        }

        public RelayCommand<object> ConfirmCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog(false);
            });
        }

        public RelayCommand<object> CancelCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CancelDialog();
            });
        }

        public RelayCommand<CancelEventArgs> WindowClosingCommand
        {
            get => new RelayCommand<CancelEventArgs>((a) =>
            {
                WindowClosing(a);
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

        public RelayCommand<DragEventArgs> DropIconCommand
        {
            get => new RelayCommand<DragEventArgs>((args) =>
            {
                DropIcon(args);
            });
        }

        public RelayCommand<DragEventArgs> DropCoverCommand
        {
            get => new RelayCommand<DragEventArgs>((args) =>
            {
                DropCover(args);
            });
        }

        public RelayCommand<object> SelectCoverCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectCover();
            });
        }

        public RelayCommand<DragEventArgs> DropBackgroundCommand
        {
            get => new RelayCommand<DragEventArgs>((args) =>
            {
                DropBackground(args);
            });
        }

        public RelayCommand<object> SelectBackgroundCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectBackground();
            });
        }

        public RelayCommand<object> SetBackgroundUrlCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SetBackgroundUrl();
            });
        }

        public RelayCommand<object> SetIconUrlCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SetIconUrl();
            });
        }

        public RelayCommand<object> SetCoverUrlCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SetCoverUrl();
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

        public RelayCommand<Link> MoveUpLinkCommand
        {
            get => new RelayCommand<Link>((link) =>
            {
                MoveLinkUp(link);
            });
        }

        public RelayCommand<Link> MoveDownLinkCommand
        {
            get => new RelayCommand<Link>((link) =>
            {
                MoveLinkDown(link);
            });
        }

        public RelayCommand<object> AddRomCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddRom();
            });
        }

        public RelayCommand<GameRom> RemoveRomCommand
        {
            get => new RelayCommand<GameRom>((rom) =>
            {
                RemoveRom(rom);
            });
        }

        public RelayCommand<GameRom> MoveUpRomCommand
        {
            get => new RelayCommand<GameRom>((rom) =>
            {
                MoveRomUp(rom);
            });
        }

        public RelayCommand<GameRom> MoveDownRomCommand
        {
            get => new RelayCommand<GameRom>((rom) =>
            {
                MoveRomDown(rom);
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

        public RelayCommand<object> AddActionCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddAction();
            });
        }

        public RelayCommand<GameAction> MoveUpActionCommand
        {
            get => new RelayCommand<GameAction>((action) =>
            {
                MoveActionUp(action);
            });
        }

        public RelayCommand<GameAction> MoveDownActionCommand
        {
            get => new RelayCommand<GameAction>((action) =>
            {
                MoveActionDown(action);
            });
        }

        public RelayCommand<GameAction> DeleteActionCommand
        {
            get => new RelayCommand<GameAction>((action) =>
            {
                RemoveAction(action);
            });
        }

        public RelayCommand<object> RemoveIconCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveIcon();
            });
        }

        public RelayCommand<object> RemoveImageCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveImage();
            });
        }

        public RelayCommand<object> RemoveBackgroundCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveBackground();
            });
        }

        public RelayCommand<object> OpenMetadataFolderCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                OpenMetadataFolder();
            });
        }

        public RelayCommand<object> SelectGoogleIconCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectGoogleIcon();
            });
        }

        public RelayCommand<object> SelectGoogleCoverCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectGoogleCover();
            });
        }

        public RelayCommand<object> SelectGoogleBackgroundCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectGoogleBackground();
            });
        }

        public RelayCommand<string> TestScriptCommand
        {
            get => new RelayCommand<string>((a) =>
            {
                TestScript(a);
            }, (a) => !a.IsNullOrEmpty());
        }

        public RelayCommand CalculateInstallSizeWithDialogCommand
        {
            get => new RelayCommand(() =>
            {
                CalculateInstallSizeWithDialog();
            });
        }
    }
}
