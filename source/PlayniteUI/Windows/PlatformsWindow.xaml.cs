using System;
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
using System.Windows.Shapes;
using Playnite.Database;
using NLog;
using PlayniteUI.Controls;
using Playnite;
using Playnite.Models;
using System.Collections.ObjectModel;
using System.IO;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class PlatformsWindow : WindowBase
    {
        private GameDatabase database;
        private ObservableCollection<Platform> platforms;        

        public PlatformsWindow()
        {
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            using (database.BufferedUpdate())
            {
                // Remove deleted platforms from database
                var dbPlatforms = database.PlatformsCollection.FindAll();
                var removedPlatforms = dbPlatforms.Where(a => platforms.FirstOrDefault(b => b.Id == a.Id) == null).ToList();
                database.RemovePlatform(removedPlatforms?.ToList());

                // Add new platforms to database
                var addedPlatforms = platforms.Where(a => a.Id == 0).ToList();
                database.AddPlatform(addedPlatforms?.ToList());
                
                // Remove files from deleted platforms
                foreach (var platform in removedPlatforms)
                {
                    if (!string.IsNullOrEmpty(platform.Icon))
                    {
                        database.DeleteFile(platform.Icon);
                    }

                    if (!string.IsNullOrEmpty(platform.Cover))
                    {
                        database.DeleteFile(platform.Cover);
                    }
                }

                // Save files from modified platforms
                var fileIdMask = "images/platforms/{0}/{1}";
                foreach (var platform in platforms)
                {
                    var dbPlatform = database.PlatformsCollection.FindById(platform.Id);

                    if (!string.IsNullOrEmpty(platform.Icon) && !platform.Icon.StartsWith("images") && File.Exists(platform.Icon))
                    {
                        if (!string.IsNullOrEmpty(dbPlatform.Icon))
                        {
                            database.DeleteFile(dbPlatform.Icon);
                        }

                        var extension = System.IO.Path.GetExtension(platform.Icon);
                        var name = Guid.NewGuid() + extension;
                        var id = string.Format(fileIdMask, platform.Id, name);
                        database.AddImage(id, name, File.ReadAllBytes(platform.Icon));
                        platform.Icon = id;
                    }

                    if (!string.IsNullOrEmpty(platform.Cover) && !platform.Cover.StartsWith("images") && File.Exists(platform.Cover))
                    {
                        if (!string.IsNullOrEmpty(dbPlatform.Cover))
                        {
                            database.DeleteFile(dbPlatform.Cover);
                        }

                        var extension = System.IO.Path.GetExtension(platform.Cover);
                        var name = Guid.NewGuid() + extension;
                        var id = string.Format(fileIdMask, platform.Id, name);
                        database.AddImage(id, name, File.ReadAllBytes(platform.Cover));
                        platform.Cover = id;
                    }
                }

                // Update modified platforms in database
                foreach (var platform in platforms)
                {
                    var dbPlatform = database.PlatformsCollection.FindById(platform.Id);
                    if (dbPlatform != null && !platform.IsEqualJson(dbPlatform))
                    {
                        database.UpdatePlatform(platform);
                    }
                }
            }

            DialogResult = true;
            Close();
        }

        public bool? ConfigurePlatforms(GameDatabase database)
        {
            platforms = new ObservableCollection<Platform>(database.PlatformsCollection.FindAll().OrderBy(a => a.Name));
            ListPlatforms.ItemsSource = platforms;

            this.database = database;
            return ShowDialog();
        }

        private void ButtonAddPlatform_Click(object sender, RoutedEventArgs e)
        {
            var platform = new Platform("New Platform") { Id = 0 };
            platforms.Add(platform);
            ListPlatforms.SelectedItem = platform;            
            TextPlatformName.Focus();
            TextPlatformName.SelectAll();
        }

        private void ButtonRemovePlatform_Click(object sender, RoutedEventArgs e)
        {
            if (ListPlatforms.SelectedItem != null)
            {
                var platform = ListPlatforms.SelectedItem as Platform;
                platforms.Remove(platform);
            }
        }

        private void ButtonSelectIcon_Click(object sender, RoutedEventArgs e)
        {
            var path = Dialogs.SelectIconFile(this);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var platform = ListPlatforms.SelectedItem as Platform;
            platform.Icon = path;
        }

        private void ButtonSelectCover_Click(object sender, RoutedEventArgs e)
        {
            var path = Dialogs.SelectIconFile(this);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var platform = ListPlatforms.SelectedItem as Platform;
            platform.Cover = path;
        }

        private void ButtonAddEmulator_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonRemoveEmulator_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}