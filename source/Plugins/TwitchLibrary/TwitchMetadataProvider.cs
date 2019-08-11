﻿using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLibrary.Models;
using TwitchLibrary.Services;

namespace TwitchLibrary
{
    public class TwitchMetadataProvider : LibraryMetadataProvider
    {
        private ILogger logger = LogManager.GetLogger();
        private TwitchLibrary library;
        private List<Entitlement> entitlements;

        public TwitchMetadataProvider(TwitchLibrary library)
        {
            this.library = library;
            
                var token = library.GetAuthToken();
                if (!token.IsNullOrEmpty())
                {
                try
                {
                    entitlements = AmazonEntitlementClient.GetAccountEntitlements(token);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to get entitlements for Twitch metadata.");
                }
            }
        }

        public override GameMetadata GetMetadata(Game game)
        {
            var gameInfo = new GameInfo
            {
                Links = new List<Link>()
            };

            var metadata = new GameMetadata()
            {
                GameInfo = gameInfo
            };

            gameInfo.Links.Add(new Link("PCGamingWiki", @"http://pcgamingwiki.com/w/index.php?search=" + game.Name));

            var program = Twitch.GetUninstallRecord(game.GameId);
            if (program != null)
            {
                gameInfo.Name = StringExtensions.NormalizeGameName(program.DisplayName);
                if (!string.IsNullOrEmpty(program.DisplayIcon) && File.Exists(program.DisplayIcon))
                {
                    var iconPath = program.DisplayIcon;
                    if (iconPath.EndsWith("ico", StringComparison.OrdinalIgnoreCase))
                    {
                        metadata.Icon = new MetadataFile(program.DisplayIcon);
                    }
                    else
                    {
                        var exeIcon = IconExtension.ExtractIconFromExe(iconPath, true);
                        if (exeIcon != null)
                        {
                            var iconName = Guid.NewGuid() + ".png";
                            metadata.Icon = new MetadataFile(iconName, exeIcon.ToByteArray(System.Drawing.Imaging.ImageFormat.Png));
                        }
                    }
                }
            }

            if (entitlements?.Any() == true)
            {
                var entitlement = entitlements.FirstOrDefault(a => a.product.id == game.GameId);
                if (entitlement != null)
                {
                    if (entitlement.product.productDetail?.iconUrl != null)
                    {
                        metadata.CoverImage = new MetadataFile(entitlement.product.productDetail.iconUrl);
                    }

                    if (entitlement.product.productDetail?.details?.backgroundUrl2 != null)
                    {
                        metadata.BackgroundImage = new MetadataFile(entitlement.product.productDetail.details.backgroundUrl2);
                    }
                }
            }

            return metadata;
        }
    }
}
