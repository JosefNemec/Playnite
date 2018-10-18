﻿using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Playnite.SDK.Models;
using System.ComponentModel;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for GameTaskView.xaml
    /// </summary>
    public partial class GameTaskView : UserControl, INotifyPropertyChanged
    {
        public bool ShowArgumentsRow
        {
            get
            {
                if (GameTask == null)
                {
                    return false;
                }

                if (GameTask.Type == GameActionType.URL)
                {
                    return false;
                }
                else if (GameTask.Type == GameActionType.Emulator && !GameTask.OverrideDefaultArgs)
                {
                    return false;
                }

                return true;
            }
        }

        public bool ShowAdditionalArgumentsRow
        {
            get
            {
                if (GameTask == null)
                {
                    return false;
                }

                if (GameTask.Type == GameActionType.Emulator && !GameTask.OverrideDefaultArgs)
                {
                    return true;
                }

                return false;
            }
        }

        public bool ShowDefaultArgumentsRow
        {
            get
            {
                if (GameTask == null)
                {
                    return false;
                }

                if (GameTask.Type == GameActionType.Emulator && !GameTask.OverrideDefaultArgs)
                {
                    return true;
                }

                return false;
            }
        }

        public bool ShowPathRow
        {
            get
            {
                if (GameTask == null)
                {
                    return false;
                }

                return GameTask.Type != GameActionType.Emulator;
            }
        }

        public bool ShowWorkingDirRow
        {
            get
            {
                if (GameTask == null)
                {
                    return false;
                }

                return GameTask.Type == GameActionType.File;
            }
        }

        public bool ShowEmulatorRow
        {
            get
            {
                if (GameTask == null)
                {
                    return false;
                }

                return GameTask.Type == GameActionType.Emulator;
            }
        }

        public bool ShowOverrideArgsRow
        {
            get
            {
                if (GameTask == null)
                {
                    return false;
                }

                return GameTask.Type == GameActionType.Emulator;
            }
        }

        public GameAction GameTask
        {
            get
            {
                if (DataContext == null)
                {
                    return null;
                }
                else
                {
                    return ((GameAction)DataContext);
                }
            }
        }

        private string selectedEmulatorArguments;
        public string SelectedEmulatorArguments
        {
            get => selectedEmulatorArguments;
            set
            {
                selectedEmulatorArguments = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedEmulatorArguments"));
            }
        }

        public List<Emulator> Emulators
        {
            get
            {
                return (List<Emulator>)GetValue(EmulatorsProperty);
            }

            set
            {
                SetValue(EmulatorsProperty, value);
            }
        }

        public static readonly DependencyProperty EmulatorsProperty = DependencyProperty.Register("Emulators", typeof(List<Emulator>), typeof(GameTaskView));

        public bool ShowNameRow
        {
            get
            {
                return (bool)GetValue(ShowNameRowProperty);
            }

            set
            {
                SetValue(ShowNameRowProperty, value);
            }
        }

        public static readonly DependencyProperty ShowNameRowProperty = DependencyProperty.Register("ShowNameRow", typeof(bool), typeof(GameTaskView));

        public event PropertyChangedEventHandler PropertyChanged;

        public GameTaskView()
        {
            InitializeComponent();
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ButtonBrowsePath_Click(object sender, RoutedEventArgs e)
        {
            var path = SystemDialogs.SelectFile(Window.GetWindow(this), "*.*|*.*");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            
            TextPath.Text = path;
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NotifyRowChange();
        }

        private void NotifyRowChange()
        {
            OnPropertyChanged("ShowArgumentsRow");
            OnPropertyChanged("ShowAdditionalArgumentsRow");
            OnPropertyChanged("ShowDefaultArgumentsRow");
            OnPropertyChanged("ShowPathRow");
            OnPropertyChanged("ShowWorkingDirRow");
            OnPropertyChanged("ShowEmulatorRow");
            OnPropertyChanged("ShowOverrideArgsRow");            
        }

        private void CheckOverrideArgs_Checked(object sender, RoutedEventArgs e)
        {
            NotifyRowChange();

            if (GameTask.OverrideDefaultArgs && !string.IsNullOrEmpty(SelectedEmulatorArguments))
            {
                GameTask.Arguments = $"{SelectedEmulatorArguments} {GameTask.AdditionalArguments}";
            }
        }

        private void ComboEmulator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Emulators == null || Emulators.Count == 0)
            {
                return;
            }

            if (GameTask?.EmulatorId != Guid.Empty && Emulators.Any(a => a.Id == GameTask?.EmulatorId))
            {
                ComboEmulatorConfig.SelectedItem = Emulators.First(a => a.Id == GameTask.EmulatorId).Profiles?.FirstOrDefault();
            }
        }

        private void ComboEmulatorConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Emulators == null || Emulators.Count == 0)
            {
                SelectedEmulatorArguments = string.Empty;
                return;
            }
            
            if (GameTask?.EmulatorId != Guid.Empty && Emulators.Any(a => a.Id == GameTask?.EmulatorId))
            {
                var emulator = Emulators.First(a => a.Id == GameTask.EmulatorId);
                var emulatorProfile = emulator.Profiles?.FirstOrDefault(a => a.Id == GameTask.EmulatorProfileId);
                if (emulatorProfile != null)
                {
                    SelectedEmulatorArguments = emulatorProfile.Arguments;
                }
                else
                {
                    SelectedEmulatorArguments = emulator.Profiles?.FirstOrDefault()?.Arguments;
                }
            }
            else
            {
                SelectedEmulatorArguments = string.Empty;
            }
        }
    }
}
