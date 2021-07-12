namespace Mara.Plugins.BetterEmbeds.Models.OEmbed
{
    public record Video
    (
        string Html,
        int Width,
        int Height
    ) : IVideo;
}
