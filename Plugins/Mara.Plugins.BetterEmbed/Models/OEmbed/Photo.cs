namespace Mara.Plugins.BetterEmbeds.Models.OEmbed
{
    public record Photo
    (
        string Url,
        int Width,
        int Height
    ) : IPhoto;
}
