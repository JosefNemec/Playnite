using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.SDK.Controls
{
    /// <summary>
    ///
    /// </summary>
    public class PluginUserControl : UserControl
    {
        private static ILogger logger = LogManager.GetLogger();

        /// <summary>
        ///
        /// </summary>
        public Game GameContext
        {
            get
            {
                return (Game)GetValue(GameContextProperty);
            }

            set
            {
                SetValue(GameContextProperty, value);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public static readonly DependencyProperty GameContextProperty = DependencyProperty.Register(
            nameof(GameContext),
            typeof(Game),
            typeof(PluginUserControl),
            new PropertyMetadata(null, GameContextPropertyChangedCallback));

        private static void GameContextPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var newContext = e.NewValue as Game;
            var oldContext = e.OldValue as Game;
            if (sender is PluginUserControl obj)
            {
                try
                {
                    obj.GameContextChanged(oldContext, newContext);
                }
                catch (Exception exc)
                {
                    logger.Error(exc, $"GameContextChanged from {obj.GetType().Name} plugin control failed.");
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="oldContext"></param>
        /// <param name="newContext"></param>
        public virtual void GameContextChanged(Game oldContext, Game newContext)
        {
        }
    }
}