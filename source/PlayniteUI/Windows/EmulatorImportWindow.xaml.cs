using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using Playnite.Models;
using System.Diagnostics;
using Playnite.Providers.Steam;
using PlayniteUI.Controls;
using System.IO;
using Playnite;
using Playnite.Emulators;
using System.Threading;

namespace PlayniteUI.Windows
{
    public class ImportableEmulator
    {
        public bool Import
        {
            get; set;
        } = true;

        public ScannedEmulator Emulator
        {
            get; set;
        }

        public ImportableEmulator(ScannedEmulator emulator)
        {
            Emulator = emulator;
        }
    }

    public enum DialogType
    {
        EmulatorImport,
        EmulatorDownload,
        GameImport,
        Wizard
    }

    public class EmulatorImportWindowModel : INotifyPropertyChanged
    {
        public bool ShowCloseButton
        {
            get => Type != DialogType.Wizard;
        }

        public bool ShowNextButton
        {
            get => Type == DialogType.Wizard;
        }

        public bool ShowBackButton
        {
            get => Type == DialogType.Wizard;
        }

        public bool ShowFinishButton
        {
            get => Type == DialogType.Wizard;
        }

        public bool ShowImportButton
        {
            get => Type != DialogType.Wizard;
        }

        private DialogType type;
        public DialogType Type
        {
            get => type;
            set
            {
                type = value;
                OnPropertyChanged("Type");
            }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        private RangeObservableCollection<ImportableEmulator> emulatorList;
        public RangeObservableCollection<ImportableEmulator> EmulatorList
        {
            get => emulatorList;
            set
            {
                emulatorList = value;
                OnPropertyChanged("EmulatorList");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public async Task SearchEmulators(string path)
        {
            try
            {
                IsLoading = true;
                var emulators = await Task.Run(() =>
                {
                    return EmulatorFinder.SearchForEmulators(path, EmulatorDefinition.GetDefinitions());
                });

                if (EmulatorList == null)
                {
                    EmulatorList = new RangeObservableCollection<ImportableEmulator>();
                }

                EmulatorList.AddRange(emulators.Select(a => new ImportableEmulator(a)));
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    /// <summary>
    /// Interaction logic for EmulatorImportWindow.xaml
    /// </summary>
    public partial class EmulatorImportWindow : WindowBase
    {
        public EmulatorImportWindowModel Model
        {
            get; private set;
        }

        public EmulatorImportWindow(DialogType type)
        {
            InitializeComponent();
            Model = new EmulatorImportWindowModel()
            {
                Type = type
            };
            DataContext = Model;            
        }

        private async void ButtonScanEmulator_Click(object sender, RoutedEventArgs e)
        {
            var path = Dialogs.SelectFolder(this);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            await Model.SearchEmulators(path);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonFinish_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonImport_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
