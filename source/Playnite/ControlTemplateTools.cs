using Playnite.Common;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite
{
    public class ControlTemplateTools
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public static void InitializePluginControls(
            ExtensionFactory extensions,
            ControlTemplate template,
            FrameworkElement templateParent,
            ApplicationMode mode,
            object contextSource,
            string contextPath)
        {
            if (DesignerTools.IsInDesignMode)
            {
                return;
            }

            foreach (var p in extensions.CustomElementList)
            {
                foreach (var elemName in p.ElementList)
                {
                    if (template.FindName($"{p.SourceName}_{elemName}", templateParent) is ContentControl elem)
                    {
                        Control plugControl = null;
                        try
                        {
                            plugControl = p.Source.GetGameViewControl(new SDK.Plugins.GetGameViewControlArgs
                            {
                                Name = elemName,
                                Mode = mode
                            });
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            logger.Error(e, $"Failed to get plugin control: {p.Source}.");
                        }

                        if (plugControl == null)
                        {
                            continue;
                        }

                        if (contextSource == null)
                        {
                            BindingTools.SetBinding(plugControl,
                                plugControl is PluginUserControl ? PluginUserControl.GameContextProperty : Control.DataContextProperty,
                                contextPath);
                        }
                        else
                        {
                            BindingTools.SetBinding(plugControl,
                                plugControl is PluginUserControl ? PluginUserControl.GameContextProperty : Control.DataContextProperty,
                                contextSource,
                                contextPath);
                        }

                        elem.Focusable = false;
                        elem.Content = plugControl;
                    }
                }
            }
        }
    }
}
