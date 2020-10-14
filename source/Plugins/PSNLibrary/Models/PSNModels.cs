using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNLibrary.Models
{
    public class TrophyTitles
    {
        public class DefinedTrophies
        {
            public int bronze { get; set; }
            public int silver { get; set; }
            public int gold { get; set; }
            public int platinum { get; set; }
        }

        public class EarnedTrophies
        {
            public int bronze { get; set; }
            public int silver { get; set; }
            public int gold { get; set; }
            public int platinum { get; set; }
        }

        public class FromUser
        {
            public string onlineId { get; set; }
            public int progress { get; set; }
            public EarnedTrophies earnedTrophies { get; set; }
            public bool hiddenFlag { get; set; }
            public DateTime? lastUpdateDate { get; set; }
        }

        public class TrophyTitle
        {
            public string npCommunicationId { get; set; }
            public string trophyTitleName { get; set; }
            public string trophyTitleDetail { get; set; }
            public string trophyTitleIconUrl { get; set; }
            public string trophyTitleSmallIconUrl { get; set; }
            public string trophyTitlePlatfrom { get; set; }
            public bool hasTrophyGroups { get; set; }
            public DefinedTrophies definedTrophies { get; set; }
            public FromUser fromUser { get; set; }

            public override string ToString()
            {
                return trophyTitleName;
            }
        }

        public int totalResults { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }
        public List<TrophyTitle> trophyTitles { get; set; }
    }

    public class AccountTitles
    {
        public class Title
        {
            public string titleId { get; set; }
            public string name { get; set; }
            public string image { get; set; }
            public string privacy { get; set; }

            public override string ToString()
            {
                return name;
            }
        }

        public int start { get; set; }
        public int size { get; set; }
        public int totalResults { get; set; }
        public List<Title> titles { get; set; }
    }

    public class ProfileInfo
    {
        public class Profile
        {
            public string onlineId { get; set; }
        }

        public Profile profile { get; set; }
    }

    public class DownloadListEntitlement
    {
        public class CloudGameMeta
        {
        }

        public class GameMeta
        {
            public string name { get; set; }
            public string icon_url { get; set; }
            public string package_type { get; set; }
            public string package_sub_type { get; set; }
            public string type { get; set; }
        }

        public class RewardMeta
        {
            public int reward_service_type { get; set; }
            public string reward_id { get; set; }
        }

        public class EntitlementAttribute
        {
            public DateTime downloadable_date { get; set; }
            public bool entitlement_key_flag { get; set; }
            public object package_file_size { get; set; }
            public bool placeholder_flag { get; set; }
            public string platform_id { get; set; }
            public string reference_package_url { get; set; }
        }

        public class DrmContent
        {
            public int bitrate { get; set; }
            public string contentId { get; set; }
            public string contentName { get; set; }
            public object contentSize { get; set; }
            public string contentUrl { get; set; }
            public int downloadType { get; set; }
            public int drmContentType { get; set; }
            public int drmType { get; set; }
            public int gracePeriod { get; set; }
            public PlatformHash platformIds { get; set; }
            public int position { get; set; }
            public string spName { get; set; }
            public string titleName { get; set; }
        }

        public class DrmDef
        {
            public bool active_flag { get; set; }
            public bool autoDownload { get; set; }
            public DateTime availableDate { get; set; }
            public string contentName { get; set; }
            public string contentType { get; set; }
            public string downloadableStatus { get; set; }
            public List<DrmContent> drmContents { get; set; }
            public int duration { get; set; }
            public string entitlementId { get; set; }
            public int firstPlayExpiration { get; set; }
            public int firstPlayExpirationHours { get; set; }
            public string image_url { get; set; }
            public int media_type { get; set; }
            public string productId { get; set; }
            public int release_date { get; set; }
            public int runtime { get; set; }
            public string salesType { get; set; }
            public bool startedStreaming { get; set; }
            public int year_release { get; set; }
        }

        public class License
        {
            public string entitlement_id { get; set; }
            public int feature_type { get; set; }
            public bool infinite_duration { get; set; }
            public DateTime start_date { get; set; }
        }

        public DateTime active_date { get; set; }
        public bool active_flag { get; set; }
        public CloudGameMeta cloudGameMeta { get; set; }
        public int entitlement_type { get; set; }
        public int feature_type { get; set; }
        public GameMeta game_meta { get; set; }
        public string id { get; set; }
        public DateTime inactive_date { get; set; }
        public bool is_consumable { get; set; }
        public object meta_revision { get; set; }
        public bool preorder_flag { get; set; }
        public bool preorder_placeholder_flag { get; set; }
        public string product_id { get; set; }
        public object revision_id { get; set; }
        public RewardMeta reward_meta { get; set; }
        public int serviceType { get; set; }
        public bool subs_flag { get; set; }
        public int use_count { get; set; }
        public int use_limit { get; set; }
        public List<EntitlementAttribute> entitlement_attributes { get; set; }
        public int? entitlement_sub_type { get; set; }
        public string sku_id { get; set; }
        public DrmDef drm_def { get; set; }
        public License license { get; set; }
    }

    [Flags]
    public enum PlatformHash : ulong
    {
        PS3 = 2147483648,
        PSP = 1073741824,
        PSVITA = 134217728
    }
}
