using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Plugins
{
    /// <summary>
    ///
    /// </summary>
    public class PluginGameAction
    {
        /// <summary>
        ///
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Game Game { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class PlayAction : PluginGameAction
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual PlayController GetPlayController(GetPlayControllerArgs args)
        {
            return null;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class InstallAction : PluginGameAction
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual InstallController GetInstallController(GetInstallControllerArgs args)
        {
            return null;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public enum GenericPlayActionType
    {
        /// <summary>
        ///
        /// </summary>
        File,
        /// <summary>
        ///
        /// </summary>
        Url
    }

    /// <summary>
    ///
    /// </summary>
    public class GenericPlayAction : PlayAction
    {
        /// <summary>
        /// Gets or sets task type.
        /// </summary>
        public GenericPlayActionType Type { get; set; }

        /// <summary>
        ///
        /// </summary>
        public TrackingMode TrackingMode { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string TrackingPath { get; set; }

        /// <summary>
        /// Gets or sets executable arguments for File type tasks.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Gets or sets executable path for File action type or URL for URL action type.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets working directory for File action type executable.
        /// </summary>
        public string WorkingDir { get; set; }
    }
}
