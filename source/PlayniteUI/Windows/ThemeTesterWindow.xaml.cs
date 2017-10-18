using PlayniteUI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PlayniteUI.Windows
{
    public class User
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string Mail { get; set; }
    }

    public class ListViewMock
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string Mail { get; set; }

        public ObservableCollection<User> MyListBoxItems { get; set; }

        public ListViewMock()
        {
            MyListBoxItems = new ObservableCollection<User>();
            MyListBoxItems.Add(new User() { Name = "John Doe", Age = 42, Mail = "john@doe-family.com" });
            MyListBoxItems.Add(new User() { Name = "Jane Doe", Age = 39, Mail = "jane@doe-family.com" });
            MyListBoxItems.Add(new User() { Name = "Sammy Doe", Age = 7, Mail = "sammy.doe@gmail.com" });
            MyListBoxItems.Add(new User() { Name = "John Doe", Age = 42, Mail = "john@doe-family.com" });
            MyListBoxItems.Add(new User() { Name = "Jane Doe", Age = 39, Mail = "jane@doe-family.com" });
            MyListBoxItems.Add(new User() { Name = "Sammy Doe", Age = 7, Mail = "sammy.doe@gmail.com" });
            MyListBoxItems.Add(new User() { Name = "John Doe", Age = 42, Mail = "john@doe-family.com" });
            MyListBoxItems.Add(new User() { Name = "Jane Doe", Age = 39, Mail = "jane@doe-family.com" });
            MyListBoxItems.Add(new User() { Name = "Sammy Doe", Age = 7, Mail = "sammy.doe@gmail.com" });
        }
    }

    /// <summary>
    /// Interaction logic for ThemeTesterWindow.xaml
    /// </summary>
    public partial class ThemeTesterWindow : WindowBase
    {
        public ThemeTesterWindow()
        {
            InitializeComponent();
            listview.ItemsSource = new ListViewMock().MyListBoxItems;
        }
    }
}
