using System.Text.Json.Serialization;

namespace Mara.Plugins.BetterEmbeds.Models.Reddit
{
    public record RedditVideo
    (
        [property: JsonPropertyName("fallback_url")] string Url,
        [property: JsonPropertyName("height")] int Height,
        [property: JsonPropertyName("width")] int Width,
        [property: JsonPropertyName("duration")] int Duration,
        [property: JsonPropertyName("is_gif")] bool IsGif
    );
}
