using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents built-in plugin distributed by default with Playnite installation.
    /// </summary>
    public enum BuiltinExtension
    {
        ///
        Unknown,
        ///
        BattleNetLibrary,
        ///
        BethesdaLibrary,
        ///
        EpicLibrary,
        ///
        GogLibrary,
        ///
        ItchioLibrary,
        ///
        OriginLibrary,
        ///
        SteamLibrary,
        ///
        UplayLibrary,
        ///
        TwitchLibrary,
        ///
        IgdbMetadata,
        ///
        HumbleLibrary
    }

    /// <summary>
    /// Represents class with utilities for built-in extensions.
    /// </summary>
    public class BuiltinExtensions
    {
        /// <summary>
        /// Gets list of built-in extension plugins.
        /// </summary>
        public static Dictionary<Guid, BuiltinExtension> ExtensionList { get; } = new Dictionary<Guid, BuiltinExtension>
        {
            { Guid.Parse("E3C26A3D-D695-4CB7-A769-5FF7612C7EDD"), BuiltinExtension.BattleNetLibrary },
            { Guid.Parse("0E2E793E-E0DD-4447-835C-C44A1FD506EC"), BuiltinExtension.BethesdaLibrary },
            { Guid.Parse("00000002-DBD1-46C6-B5D0-B1BA559D10E4"), BuiltinExtension.EpicLibrary },
            { Guid.Parse("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E"), BuiltinExtension.GogLibrary },
            { Guid.Parse("00000001-EBB2-4EEC-ABCB-7C89937A42BB"), BuiltinExtension.ItchioLibrary },
            { Guid.Parse("85DD7072-2F20-4E76-A007-41035E390724"), BuiltinExtension.OriginLibrary },
            { Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB"), BuiltinExtension.SteamLibrary },
            { Guid.Parse("E2A7D494-C138-489D-BB3F-1D786BEEB675"), BuiltinExtension.TwitchLibrary },
            { Guid.Parse("C2F038E5-8B92-4877-91F1-DA9094155FC5"), BuiltinExtension.UplayLibrary },
            { Guid.Parse("000001DB-DBD1-46C6-B5D0-B1BA559D10E4"), BuiltinExtension.IgdbMetadata },
            { Guid.Parse("96e8c4bc-ec5c-4c8b-87e7-18ee5a690626"), BuiltinExtension.HumbleLibrary }
        };

        /// <summary>
        /// Returns if specified plugin is built-in plugin.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <returns></returns>
        public static bool GetIsBuiltInPlugin(Guid pluginId)
        {
            return ExtensionList.ContainsKey(pluginId);
        }

        /// <summary>
        /// Gets extension plugin by plugin's ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static BuiltinExtension GetExtensionFromId(Guid id)
        {
            if (ExtensionList.ContainsKey(id))
            {
                return ExtensionList[id];
            }
            else
            {
                return BuiltinExtension.Unknown;
            }
        }

        /// <summary>
        /// Gets plugin ID for specified built-in extension.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static Guid GetIdFromExtension(BuiltinExtension extension)
        {
            if (ExtensionList.ContainsValue(extension))
            {
                return ExtensionList.First(a => a.Value == extension).Key;
            }
            else
            {
                return Guid.Empty;
            }
        }
    }
}
