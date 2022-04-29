using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Playnite.Controls;

namespace Playnite.DesktopApp.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class ActionSelectionWindow : WindowBase
    {
        public ActionSelectionWindow() : base()
        {
            InitializeComponent();
        }

        private void WindowActionSelectionWindow_ContentRendered(object sender, EventArgs e)
        {
            for(int i = 0; i < 9; ++i)
            {
                DependencyObject item = ActionItems.ItemContainerGenerator.ContainerFromIndex(i);
                if (item == null)
                {
                    break; //less than 9 items
                }

                Button actionButton = (Button)VisualTreeHelper.GetChild(item, 0);
                Key key = (Key)Enum.Parse(typeof(Key), "D" + (i + 1));
                KeyBinding newBinding = new KeyBinding() { Command = actionButton.Command, CommandParameter = actionButton.CommandParameter, Key = key };
                InputBindings.Add(newBinding);
            }
        }
    }
}