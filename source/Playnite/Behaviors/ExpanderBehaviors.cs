using Playnite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Playnite.Behaviors
{
    public class ExpanderBehaviors
    {
        private static readonly DependencyProperty SaveStateProperty =
            DependencyProperty.RegisterAttached("SaveState", typeof(bool), typeof(ExpanderBehaviors), new PropertyMetadata(new PropertyChangedCallback(HandleSaveStateChanged)));

        public static bool GetSaveState(DependencyObject obj)
        {
            return (bool)obj.GetValue(SaveStateProperty);
        }

        public static void SetSaveState(DependencyObject obj, bool value)
        {
            obj.SetValue(SaveStateProperty, value);
        }

        private static readonly DependencyProperty SaveStateIdProperty =
            DependencyProperty.RegisterAttached("SaveStateId", typeof(string), typeof(ExpanderBehaviors));

        public static string GetSaveStateId(DependencyObject obj)
        {
            return (string)obj.GetValue(SaveStateIdProperty);
        }

        public static void SetSaveStateId(DependencyObject obj, string value)
        {
            obj.SetValue(SaveStateIdProperty, value);
        }

        private static void HandleSaveStateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var expander = (Expander)obj;
            if (DesignerProperties.GetIsInDesignMode(expander))
            {
                return;
            }

            expander.Loaded += Expander_Loaded;
            if ((bool)args.NewValue)
            {
                expander.Expanded += Control_Expanded;
                expander.Collapsed += Expander_Collapsed;
            }
            else
            {
                expander.Expanded -= Control_Expanded;
                expander.Collapsed -= Expander_Collapsed;
            }
        }

        private static void Expander_Loaded(object sender, RoutedEventArgs e)
        {            
            var expander = (Expander)sender;
            var id = GetSaveStateId(expander);
            expander.IsExpanded = !PlayniteApplication.Current.AppSettings.ViewSettings.CollapsedCategories.Contains(id);
        }

        private static void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            var expander = sender as Expander;
            if (!expander.IsLoaded)
            {
                return;
            }

            var id = GetSaveStateId(expander);            
            if (!PlayniteApplication.Current.AppSettings.ViewSettings.CollapsedCategories.Contains(id))
            {
                PlayniteApplication.Current.AppSettings.ViewSettings.CollapsedCategories.Add(id);
            }
        }

        private static void Control_Expanded(object sender, RoutedEventArgs e)
        {
            var expander = sender as Expander;
            if (!expander.IsLoaded)
            {
                return;
            }

            var id = GetSaveStateId(expander);
            if (PlayniteApplication.Current.AppSettings.ViewSettings.CollapsedCategories.Contains(id))
            {
                PlayniteApplication.Current.AppSettings.ViewSettings.CollapsedCategories.Remove(id);
            }
        }
    }
}
