using System.Text.Json.Serialization;

namespace Mara.Plugins.BetterEmbeds.Models.Reddit
{
    public record RedditUser
    (
        [property: JsonPropertyName("display_name_prefixed")] string DisplayNamePrefixed,
        [property: JsonPropertyName("icon_img")] string IconImage
    )
    : IRedditUser;
}
