using Playnite.Commands;
using Playnite.Common;
using Playnite.Database;
using Playnite.DesktopApp.ViewModels;
using Playnite.Extensions;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Playnite.DesktopApp.Controls.Views
{
    [TemplatePart(Name = "PART_SelectFields", Type = typeof(Selector))]
    [TemplatePart(Name = "PART_SelectItems", Type = typeof(Selector))]
    public class ExplorerPanel : Control
    {
        private readonly DesktopAppViewModel mainModel;
        private Selector SelectFields;
        private Selector SelectItems;

        static ExplorerPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExplorerPanel), new FrameworkPropertyMetadata(typeof(ExplorerPanel)));
        }

        public ExplorerPanel() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public ExplorerPanel(DesktopAppViewModel mainModel)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.mainModel = DesignMainViewModel.DesignIntance;
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            SelectFields = Template.FindName("PART_SelectFields", this) as Selector;
            if (SelectFields != null)
            {
                BindingTools.SetBinding(SelectFields,
                    Selector.SelectedValueProperty,
                    mainModel.DatabaseExplorer,
                    nameof(DatabaseExplorer.SelectedField),
                    BindingMode.TwoWay);
                BindingTools.SetBinding(SelectFields,
                    Selector.ItemsSourceProperty,
                    mainModel.DatabaseExplorer,
                    nameof(DatabaseExplorer.Fields));
            }

            SelectItems = Template.FindName("PART_SelectItems", this) as Selector;
            if (SelectItems != null)
            {
                SelectItems.DisplayMemberPath = nameof(DatabaseExplorer.SelectionObject.Name);
                BindingTools.SetBinding(SelectItems,
                    Selector.SelectedItemProperty,
                    mainModel.DatabaseExplorer,
                    nameof(DatabaseExplorer.SelectedFieldObject),
                    BindingMode.TwoWay);
                BindingTools.SetBinding(SelectItems,
                    Selector.ItemsSourceProperty,
                    mainModel.DatabaseExplorer,
                    nameof(DatabaseExplorer.FieldValues));
            }
        }
    }
}
