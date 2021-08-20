namespace Mara.Plugins.BetterEmbeds.Models.Reddit
{
    public record RedditUser
    (
        string DisplayNamePrefixed,
        string IconImage
    )
    : IRedditUser;
}
