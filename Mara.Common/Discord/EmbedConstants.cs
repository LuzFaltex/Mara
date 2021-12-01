using System.Drawing;
using Remora.Discord.API.Objects;

namespace Mara.Common.Discord
{
    /// <summary>
    /// A set of constants which represent restraits on Embeds.
    /// </summary>
    public static class EmbedConstants
    {
        /// <summary> 
        /// Returns the maximum number of fields allowed by Discord. 
        /// </summary>
        public const int MaxFieldCount = 25;
        /// <summary> 
        /// Returns the maximum title length allowed by Discord. 
        /// </summary>
        public const int MaxTitleLength = 256;
        /// <summary> 
        /// Returns the maximum description length allowed by Discord. 
        /// </summary>
        public const int MaxDescriptionLength = 2048;
        /// <summary> 
        /// Returns the maximum total embed length allowed by Discord. 
        /// </summary>
        public const int MaxEmbedLength = 6000;
        /// <summary>
        /// Returns the maximum author name length allowed by Discord.
        /// </summary>
        public const int MaxAuthorNameLength = 256;
        /// <summary>
        /// Returns the maximum footer length allowed by Discord.
        /// </summary>
        public const int MaxFooterTextLength = 2048;

        /// <summary>
        /// The default embed color.
        /// </summary>
        public static readonly Color DefaultColor = Color.FromArgb(95, 186, 125);

        /// <summary>
        /// Default embed footer. "React with ❌ to remove this embed."
        /// </summary>
        public static readonly EmbedFooter DefaultFooter = new("React with ❌ to remove this embed");

        /// <summary>
        /// A set of image assets in url form for use in embeds.
        /// </summary>
        public static class ImageUrls
        {

        }
    }
}
