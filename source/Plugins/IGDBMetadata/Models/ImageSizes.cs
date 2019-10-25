using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGDBMetadata.Models
{
    public class ImageSizes
    {
        /// <summary>
        /// 90 x 128     Fit
        /// </summary>
        public const string cover_small = "cover_small";

        /// <summary>
        // 569 x 320 	Lfill, Center gravity
        /// </summary>
        public const string screenshot_med = "screenshot_med";

        /// <summary>
        // 264 x 374 	Fit
        /// </summary>
        public const string cover_big = "cover_big";

        /// <summary>
        // 284 x 160 	Fit
        /// </summary>
        public const string logo_med = "logo_med";

        /// <summary>
        // 889 x 500 	Lfill, Center gravity
        /// </summary>
        public const string screenshot_big = "cover_small";

        /// <summary>
        // 1280 x 720 	Lfill, Center gravity
        /// </summary>
        public const string screenshot_huge = "screenshot_huge";

        /// <summary>
        // 90 x 90 	Thumb, Center gravity
        /// </summary>
        public const string thumb = "thumb";

        /// <summary>
        // 35 x 35 	Thumb, Center gravity
        /// </summary>
        public const string micro = "micro";

        /// <summary>
        // 1280 x 720 	Fit, Center gravity
        /// </summary>
        public const string p720 = "720p";

        /// <summary>
        // 1920 x 1080 	Fit, Center gravity
        /// </summary>
        public const string p1080 = "1080p";

        /// <summary>
        // original size
        /// </summary>
        public const string original = "original";
    }
}
