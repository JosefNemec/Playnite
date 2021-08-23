using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes emulation API.
    /// </summary>
    public interface IEmulationAPI
    {
        /// <summary>
        /// Gets built-in platform definitions.
        /// </summary>
        IList<EmulatedPlatform> Platforms { get; }

        /// <summary>
        /// Gets built-in region defitions.
        /// </summary>
        IList<EmulatedRegion> Regions { get; }

        /// <summary>
        /// Gets specific platform by id.
        /// </summary>
        /// <param name="platformId"></param>
        /// <returns></returns>
        EmulatedPlatform GetPlatform(string platformId);

        /// <summary>
        /// Gets specific region by id.
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        EmulatedRegion GetRegion(string regionId);
    }
}
