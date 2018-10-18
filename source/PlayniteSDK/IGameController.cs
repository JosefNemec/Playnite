﻿using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes game controller.
    /// </summary>
    public interface IGameController : IDisposable
    {
        /// <summary>
        /// Gets value indicating wheter the game is running.
        /// </summary>
        bool IsGameRunning { get; }

        /// <summary>
        /// Gets game being handled.
        /// </summary>
        Game Game
        {
            get;
        }

        /// <summary>
        /// Installs game.
        /// </summary>
        void Install();

        /// <summary>
        /// Uninstalls game.
        /// </summary>
        void Uninstall();

        /// <summary>
        /// Starts game.
        /// </summary>
        void Play();

        /// <summary>
        /// Occurs when game is being started.
        /// </summary>
        event GameControllerEventHandler Starting;

        /// <summary>
        /// Occurs when game is started.
        /// </summary>
        event GameControllerEventHandler Started;

        /// <summary>
        /// Occurs when game stops running.
        /// </summary>
        event GameControllerEventHandler Stopped;

        /// <summary>
        /// Occurs when game is finished uninstalling.
        /// </summary>
        event GameControllerEventHandler Uninstalled;

        /// <summary>
        /// Occurs when game is finished installing.
        /// </summary>
        event GameControllerEventHandler Installed;
    }
}
