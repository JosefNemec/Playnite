using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary.Models
{
    public class PickActionAction
    {
        /// <summary>
        /// human-readable or standard name
        /// </summary>
        public string name;

        /// <summary>
        /// file path(relative to manifest or absolute), URL, etc.
        /// </summary>
        public string path;

        /// <summary>
        /// icon name(see static/fonts/icomoon/demo.html, don’t include icon- prefix)
        /// </summary>
        public string icon;

        /// <summary>
        /// command-line arguments
        /// </summary>
        public string[] args;

        /// <summary>
        /// sandbox opt-in
        /// </summary>
        public bool sandbox;

        /// <summary>
        /// requested API scope
        /// </summary>
        public string scope;

        /// <summary>
        /// don’t redirect stdout/stderr, open in new console window
        /// </summary>
        public bool console;
    }

    public class PickManifestAction
    {
        public List<PickActionAction> actions;
    }
}
