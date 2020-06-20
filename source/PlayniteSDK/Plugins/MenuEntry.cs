using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Plugins
{
    /// <summary>
    /// Represents base class for plugin menu item.
    /// </summary>
    public abstract class PluginMenuItem
    {
        /// <summary>
        /// Gets or sets menu item icon.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets menu item description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets meni item menu section.
        /// </summary>
        public string MenuSection { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Description ?? base.ToString();
        }
    }

    #region Game menu

    /// <summary>
    /// Represents arguments object used when game menu item action is activated.
    /// </summary>
    public class GameMenuItemActionArgs
    {
        /// <summary>
        /// Gets or sets source games.
        /// </summary>
        public List<Game> Games { get; set; }

        /// <summary>
        /// Gets or sets source menu item.
        /// </summary>
        public GameMenuItem SourceItem { get; set; }
    }

    /// <summary>
    /// Represents arguments object used when game menu item action is activated.
    /// </summary>
    public class ScriptGameMenuItemActionArgs
    {
        /// <summary>
        /// Gets or sets source games.
        /// </summary>
        public List<Game> Games { get; set; }

        /// <summary>
        /// Gets or sets source menu item.
        /// </summary>
        public ScriptGameMenuItem SourceItem { get; set; }
    }

    /// <summary>
    /// Represents game menu item.
    /// </summary>
    public class GameMenuItem : PluginMenuItem
    {
        /// <summary>
        /// Gets or sets action to be invoked when menu item is activated.
        /// </summary>
        public Action<GameMenuItemActionArgs> Action { get; set; }

        /// <summary>
        /// Creates game menu item from script game menu item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static GameMenuItem FromScriptGameMenuItem(ScriptGameMenuItem item)
        {
            return new GameMenuItem
            {
                Description = item.Description,
                Icon = item.Icon,
                MenuSection = item.MenuSection
            };
        }
    }

    /// <summary>
    /// Represents script game menu item.
    /// </summary>
    public class ScriptGameMenuItem : PluginMenuItem
    {
        /// <summary>
        /// Gets or sets function to be executed when menu item is activated.
        /// </summary>
        public string FunctionName { get; set; }
    }

    /// <summary>
    /// Represents arguments for getting game menu items.
    /// </summary>
    public class GetGameMenuItemsArgs
    {
        /// <summary>
        /// Gets or sets source games for target menu items.
        /// </summary>
        public List<Game> Games { get; set; }
    }

    #endregion Game menu

    #region Main menu

    /// <summary>
    /// Represents arguments object used when main menu item action is activated.
    /// </summary>
    public class ScriptMainMenuItemActionArgs
    {
        /// <summary>
        /// Gets or sets source menu item.
        /// </summary>
        public ScriptMainMenuItem SourceItem { get; set; }
    }

    /// <summary>
    /// Represents arguments object used when main menu item action is activated.
    /// </summary>
    public class MainMenuItemActionArgs
    {
        /// <summary>
        /// Gets or sets source menu item.
        /// </summary>
        public MainMenuItem SourceItem { get; set; }
    }

    /// <summary>
    /// Represents main menu item.
    /// </summary>
    public class MainMenuItem : PluginMenuItem
    {
        /// <summary>
        /// Gets or sets action to be invoked when menu item is activated.
        /// </summary>
        public Action<MainMenuItemActionArgs> Action { get; set; }

        /// <summary>
        /// Creates main menu item from script main menu item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static MainMenuItem FromScriptMainMenuItem(ScriptMainMenuItem item)
        {
            return new MainMenuItem
            {
                Description = item.Description,
                Icon = item.Icon,
                MenuSection = item.MenuSection
            };
        }
    }

    /// <summary>
    ///  Represents script main menu item.
    /// </summary>
    public class ScriptMainMenuItem : PluginMenuItem
    {
        /// <summary>
        /// Gets or sets function to be executed when menu item is activated.
        /// </summary>
        public string FunctionName { get; set; }
    }

    /// <summary>
    /// Represents arguments for getting main menu items.
    /// </summary>
    public class GetMainMenuItemsArgs
    {
    }

    #endregion Main menu
}
