using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.DesktopApp.Controls
{
    public class ExpanderEx : Expander
    {
        private bool ignoreChanges = false;
        private readonly PlayniteSettings settings;

        public string SaveGameGroupId
        {
            get { return (string)GetValue(SaveGameGroupIdProperty); }
            set { SetValue(SaveGameGroupIdProperty, value); }
        }

        public static readonly DependencyProperty SaveGameGroupIdProperty =
            DependencyProperty.Register(
                nameof(SaveGameGroupId),
                typeof(string),
                typeof(ExpanderEx),
                new FrameworkPropertyMetadata(null));

        static ExpanderEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpanderEx), new FrameworkPropertyMetadata(typeof(ExpanderEx)));
        }

        public ExpanderEx() : this(PlayniteApplication.Current?.AppSettings)
        {
        }

        public ExpanderEx(PlayniteSettings settings) : base()
        {
            if (settings == null || DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.settings = settings;
            Loaded += ExpanderEx_Loaded;
            Unloaded += ExpanderEx_Unloaded;
        }

        private void ViewSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ignoreChanges)
            {
                return;
            }

            if (e.PropertyName == nameof(ViewSettings.CollapsedGroups))
            {
                if (SaveGameGroupId != null && settings != null)
                {
                    ignoreChanges = true;
                    IsExpanded = !settings.ViewSettings.IsGroupCollapsed(settings.ViewSettings.GroupingOrder, SaveGameGroupId);
                    ignoreChanges = false;
                }
            }
        }

        private void ExpanderEx_Loaded(object sender, RoutedEventArgs e)
        {
            Expanded += ExpanderEx_Expanded;
            Collapsed += ExpanderEx_Collapsed;

            if (SaveGameGroupId != null && settings != null)
            {
                settings.ViewSettings.PropertyChanged += ViewSettings_PropertyChanged;
                ignoreChanges = true;
                var newState = !settings.ViewSettings.IsGroupCollapsed(settings.ViewSettings.GroupingOrder, SaveGameGroupId);
                if (newState != IsExpanded)
                {
                    IsExpanded = newState;
                }

                ignoreChanges = false;
            }
        }

        private void ExpanderEx_Unloaded(object sender, RoutedEventArgs e)
        {
            Expanded -= ExpanderEx_Expanded;
            Collapsed -= ExpanderEx_Collapsed;

            if (SaveGameGroupId != null && settings != null)
            {
                settings.ViewSettings.PropertyChanged -= ViewSettings_PropertyChanged;
            }
        }

        private void ExpanderEx_Collapsed(object sender, RoutedEventArgs e)
        {
            if (ignoreChanges)
            {
                return;
            }

            if (SaveGameGroupId != null && settings != null)
            {
                ignoreChanges = true;
                settings.ViewSettings.SetGroupCollapseState(settings.ViewSettings.GroupingOrder, SaveGameGroupId, true);
                ignoreChanges = false;
            }
        }

        private void ExpanderEx_Expanded(object sender, RoutedEventArgs e)
        {
            if (ignoreChanges)
            {
                return;
            }

            if (SaveGameGroupId != null && settings != null)
            {
                ignoreChanges = true;
                settings.ViewSettings.SetGroupCollapseState(settings.ViewSettings.GroupingOrder, SaveGameGroupId, false);
                ignoreChanges = false;
            }
        }
    }
}
