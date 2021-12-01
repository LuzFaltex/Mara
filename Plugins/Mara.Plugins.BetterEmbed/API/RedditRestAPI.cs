using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mara.Plugins.BetterEmbeds.Models.Reddit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Rest;
using Remora.Results;

namespace Mara.Plugins.BetterEmbeds.API
{
    public sealed class RedditRestAPI
    {
        public const string PostUrl = "https://www.reddit.com/r/{0}/comments/{1}/.json";
        public const string ProfileUrl = "https://www.reddit.com/user/{0}/about.json";

        private readonly JsonSerializerOptions _serializerOptions;
        private readonly ILogger<RedditRestAPI> _logger;
        private readonly IRestHttpClient _restClient;

        public RedditRestAPI(IRestHttpClient restClient, IOptions<JsonSerializerOptions> serializerOptions, ILogger<RedditRestAPI> logger)
        {
            _restClient = restClient;
            _serializerOptions = serializerOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// Gets a post using the subreddit and post id.
        /// </summary>
        /// <param name="subredditName">The subreddit this post belongs to.</param>
        /// <param name="postId">The unique id of this post.</param>
        /// <param name="allowNullReturn">Whether or not to allow an empty return value.</param>
        /// <param name="cancellationToken">The cancellation token for this operation.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        public async Task<Result<RedditPost>> GetRedditPostAsync
        (
            string subredditName,
            string postId,
            bool allowNullReturn = false,
            CancellationToken cancellationToken = default
        )
        {
            var redditUrl = string.Format(PostUrl, subredditName, postId);

            return await _restClient.GetAsync<RedditPost>
            (
                redditUrl,
                "$[0].data.children[0].data",
                x => x.Build(),
                allowNullReturn,
                cancellationToken
            );
        }

        public async Task<Result<RedditUser>> GetRedditUserAsync
        (
            string username,
            bool allowNullReturn = false,
            CancellationToken cancellationToken = default
        )
        {
            var redditUrl = string.Format(ProfileUrl, username);

            return await _restClient.GetAsync<RedditUser>
            (
                redditUrl, 
                "$.data.subreddit", 
                x => x.Build(),
                allowNullReturn,
                cancellationToken
            );
        }
    }
}
