using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mara.Common.Extensions;
using Mara.Plugins.BetterEmbeds.Models.Reddit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.Rest.Results;
using Remora.Results;

namespace Mara.Plugins.BetterEmbeds.API
{
    public sealed class RedditRestAPI
    {
        private const string PostUrl = "https://www.reddit.com/r/{0}/comments/{1}/.json";
        private const string ProfileUrl = "https://www.reddit.com/user/{0}/about.json";

        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly ILogger<RedditRestAPI> _logger;
            
        public RedditRestAPI(IHttpClientFactory factory, IOptions<JsonSerializerOptions> serializerOptions, ILogger<RedditRestAPI> logger)
        {
            _logger = logger;
            _serializerOptions = serializerOptions.Value;
            _client = factory.CreateClient();
        }

        /// <summary>
        /// Gets a post using the subreddit and post id.
        /// </summary>
        /// <param name="subredditName">The subreddit this post belongs to.</param>
        /// <param name="postId">The unique id of this post.</param>
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

            var response = await _client.GetAsync(redditUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            return await UnpackResponseAsync<RedditPost>(response, "$[0].data.children[0].data", allowNullReturn, cancellationToken);
        }

        public async Task<Result<RedditUser>> GetRedditUserAsync
        (
            string username,
            bool allowNullReturn = false,
            CancellationToken cancellationToken = default
        )
        {
            var redditUrl = string.Format(ProfileUrl, username);

            var response = await _client.GetAsync(redditUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            return await UnpackResponseAsync<RedditUser>(response, "$.data.subreddit", allowNullReturn, cancellationToken);
        }

        private async Task<Result<TEntity>> UnpackResponseAsync<TEntity>
        (
            HttpResponseMessage response,
            string path = "",
            bool allowNullReturn = false,
            CancellationToken cancellationToken = default
        )
        {
            if (!response.IsSuccessStatusCode)
            {
                return new HttpResultError(response.StatusCode, response.ReasonPhrase);
            }

            if (response.Content.Headers.ContentLength == 0)
            {
                return allowNullReturn
                    ? Result<TEntity>.FromSuccess(default!)
                    : throw new InvalidOperationException("Response content null, but null returns not allowed.");
            }

            TEntity? entity;

            if (string.IsNullOrEmpty(path))
            {
                entity = await JsonSerializer.DeserializeAsync<TEntity>
                (
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    new JsonSerializerOptions(JsonSerializerDefaults.Web),
                    cancellationToken
                );
            }
            else
            {
                var doc = await JsonSerializer.DeserializeAsync<JsonDocument>
                (
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    new JsonSerializerOptions(JsonSerializerDefaults.Web),
                    cancellationToken
                );

                var element = doc.SelectElement(path);

                if (!element.HasValue)
                {
                    return allowNullReturn
                        ? Result<TEntity>.FromSuccess(default!)
                        : throw new InvalidOperationException("Response content null, but null returns not allowed.");
                }

                _logger.LogTrace(element.Value.GetRawText());

                entity = element.Value.ToObject<TEntity>(_serializerOptions);
            }

            if (entity is not null)
            {
                return Result<TEntity>.FromSuccess(entity);
            }

            return allowNullReturn
                ? Result<TEntity>.FromSuccess(default!) // Might be TEntity?
                : throw new InvalidOperationException("Response content null, but null returns not allowed.");

        }
    }
}
