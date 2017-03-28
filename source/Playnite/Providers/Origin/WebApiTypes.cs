using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers.Origin
{
    public class AccountInfoResponse
    {
        public class AcccountInfoData
        {
            public long pidId;
        }

        public string error;
        public AcccountInfoData pid;
    }

    public class AuthTokenResponse
    {
        public string error;
        public string expires_in;
        public string token_type;
        public string access_token;
    }

    public class GameLocalDataResponse
    {
        public class LocalizableAttributes
        {
            public string longDescription;
            public string displayName;
        }

        public class Publishing
        {
            public class Software
            {
                public class FulfillmentAttributes
                {
                    public string executePathOverride;
                    public string installationDirectory;
                    public string installCheckOverride;
                }

                public string softwareId;
                public string softwarePlatform;
                public FulfillmentAttributes fulfillmentAttributes;
            }

            public class SoftwareList
            {
                public List<Software> software;
            }

            public SoftwareList softwareList;
        }

        public string offerId;
        public string offerType;
        public Publishing publishing;
        public LocalizableAttributes localizableAttributes;
    }

    public class GameStoreDataResponse
    {
        public class I18n
        {
            public string longDescription;
            public string officialSiteURL;
            public string gameForumURL;
            public string displayName;
            public string packArtSmall;
            public string packArtMedium;
            public string packArtLarge;
            public string gameManualURL;
        }

        public class Platform
        {
            public string platform;
            public DateTime releaseDate;
        }

        public string offerId;
        public string offerType;
        public List<Platform> platforms;
        public string publisherFacetKey;
        public string developerFacetKey;
        public string imageServer;
        public string itemName;
        public string itemType;
        public string itemId;
        public I18n i18n;
        public string offerPath;
    }

    public class AccountEntitlementsResponse
    {
        public class Entitlement
        {
            public long entitlementId;
            public string offerId;
            public string offerPath;
            public string status;
            public string offerType;
            public string originDisplayType;
            public string masterTitleId;
            public string gameDistributionSubType;
        }

        public string error;
        public List<Entitlement> entitlements;
    }
}
