namespace Mara.Plugins.BetterEmbeds.Models.OEmbed
{
    public interface IPhoto
    {
        string Url { get; init; }
        int Width { get; init; }
        int Height { get; init; }
    }
}
