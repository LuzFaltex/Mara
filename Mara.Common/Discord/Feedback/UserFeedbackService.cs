using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mara.Common.Discord.Feedback.Errors;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace Mara.Common.Discord.Feedback
{
    /// <summary>
    /// Handles sending formatted messages to users
    /// </summary>
    public sealed class UserFeedbackService
    {
        private readonly ContextInjectionService _contextInjection;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestUserAPI _userApi;
        private readonly IDiscordRestWebhookAPI _webhookApi;
        private readonly IdentityInformationConfiguration _identity;

        /// <summary>
        /// Gets a value indicating whether the original interaction message (if any) has been edited)
        /// </summary>
        public bool HasEditedOriginalInteraction { get; private set; }

        public UserFeedbackService(ContextInjectionService contextInjection, IDiscordRestChannelAPI channelApi, IDiscordRestUserAPI userApi, IDiscordRestWebhookAPI webhookApi, IdentityInformationConfiguration identity)
        {
            _contextInjection = contextInjection;
            _channelApi = channelApi;
            _userApi = userApi;
            _webhookApi = webhookApi;
            _identity = identity;
        }

        public ValueTask<Result<IReadOnlyList<IMessage>>> SendConfirmationAsync(
            Snowflake channelId,
            Snowflake? target,
            string contents,
            CancellationToken cancellationToken = default)
            => SendMessageAsync(channelId, target, new ConfirmationMessage(contents), cancellationToken);

        public ValueTask<Result<IReadOnlyList<IMessage>>> SendContextualConfirmationAsync(
            Snowflake? target,
            string contents,
            CancellationToken cancellationToken = default)
            => SendContextualMessageAsync(target, new ConfirmationMessage(contents), cancellationToken);

        public ValueTask<Result<IReadOnlyList<IMessage>>> SendErrorAsync(
            Snowflake channelId,
            Snowflake? target,
            string contents,
            CancellationToken cancellationToken = default)
            => SendMessageAsync(channelId, target, new ErrorMessage(contents), cancellationToken);

        public ValueTask<Result<IReadOnlyList<IMessage>>> SendContextualErrorAsync(
            Snowflake? target,
            string contents,
            CancellationToken cancellationToken = default)
            => SendContextualMessageAsync(target, new ErrorMessage(contents), cancellationToken);

        public ValueTask<Result<IReadOnlyList<IMessage>>> SendWarningAsync(
            Snowflake channelId,
            Snowflake? target,
            string contents,
            CancellationToken cancellationToken = default)
            => SendMessageAsync(channelId, target, new WarningMessage(contents), cancellationToken);

        public ValueTask<Result<IReadOnlyList<IMessage>>> SendContextualWarningAsync(
            Snowflake? target,
            string contents,
            CancellationToken cancellationToken = default)
            => SendContextualMessageAsync(target, new WarningMessage(contents), cancellationToken);

        public ValueTask<Result<IReadOnlyList<IMessage>>> SendQueryAsync(
            Snowflake channelId,
            Snowflake? target,
            string contents,
            CancellationToken cancellationToken = default)
            => SendMessageAsync(channelId, target, new PromptMessage(contents), cancellationToken);

        public ValueTask<Result<IReadOnlyList<IMessage>>> SendContextualQueryAsync(
            Snowflake? target,
            string contents,
            CancellationToken cancellationToken = default)
            => SendContextualMessageAsync(target, new PromptMessage(contents), cancellationToken);


        public ValueTask<Result<IReadOnlyList<IMessage>>> SendInfoAsync(
            Snowflake channel,
            Snowflake? target,
            string contents,
            CancellationToken cancellationToken = default)
            => SendMessageAsync(channel, target, new InfoMessage(contents), cancellationToken);

        public ValueTask<Result<IReadOnlyList<IMessage>>> SendContextualInfoAsync(
            Snowflake? target,
            string contents,
            CancellationToken cancellationToken = default)
            => SendContextualMessageAsync(target, new InfoMessage(contents), cancellationToken);

        /// <summary>
        /// Send a message
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="target"></param>
        /// <param name="userMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ValueTask<Result<IReadOnlyList<IMessage>>> SendMessageAsync(
            Snowflake channelId,
            Snowflake? target,
            UserMessage userMessage,
            CancellationToken cancellationToken = default)
            => SendContentAsync(channelId, target, userMessage.Color, userMessage.Message, cancellationToken);

        /// <summary>
        /// Send a contextual message
        /// </summary>
        /// <param name="target"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ValueTask<Result<IReadOnlyList<IMessage>>> SendContextualMessageAsync(
            Snowflake? target,
            UserMessage message,
            CancellationToken cancellationToken = default)
            => SendContextualContentAsync(target, message.Color, message.Message, cancellationToken);

        /// <summary>
        /// Sends the given embed to the given channel
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="embed"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public ValueTask<Result<IMessage>> SendEmbedAsync(
            Snowflake channelId,
            Embed embed,
            CancellationToken ct = default)
            => new(_channelApi.CreateMessageAsync(channelId, embeds: new List<IEmbed>() { embed }.AsReadOnly(), ct: ct));

        /// <summary>
        /// Sends the given embed to the current context.
        /// </summary>
        /// <param name="embed">The embed.</param>
        /// <param name="cancellationToken">The cancellation token for this operation.</param>
        /// <returns></returns>
        public async ValueTask<Result<IMessage>> SendContextualEmbedAsync(Embed embed, CancellationToken cancellationToken = default)
        {
            if (_contextInjection.Context is null)
            {
                return new UserError("Contextual sends require a context to be available.");
            }

            switch (_contextInjection.Context)
            {
                case MessageContext messageContext:
                    {
                        return await _channelApi.CreateMessageAsync(messageContext.ChannelID, embeds: new List<IEmbed>() { embed }.AsReadOnly(),
                            ct: cancellationToken);
                    }
                case InteractionContext interactionContext:
                    {
                        if (HasEditedOriginalInteraction)
                        {
                            return await _webhookApi.CreateFollowupMessageAsync(
                                _identity.ApplicationId,
                                interactionContext.Token,
                                embeds: new[] { embed },
                                ct: cancellationToken);
                        }

                        var edit = await _webhookApi.EditOriginalInteractionResponseAsync(
                            _identity.ApplicationId,
                            interactionContext.Token,
                            embeds: new[] { embed },
                            ct: cancellationToken);

                        if (edit.IsSuccess)
                        {
                            HasEditedOriginalInteraction = true;
                        }

                        return edit;
                    }
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
        }

        public async ValueTask<Result<IMessage>> SendPrivateEmbedAsync(
            Snowflake user,
            Embed embed,
            CancellationToken cancellationToken = default)
        {
            var getUserDm = await _userApi.CreateDMAsync(user, cancellationToken);
            if (!getUserDm.IsSuccess)
            {
                return Result<IMessage>.FromError(getUserDm);
            }

            var dm = getUserDm.Entity;

            return await SendEmbedAsync(dm.ID, embed, cancellationToken);
        }

        /// <summary>
        /// Creates a feedback embed.
        /// </summary>
        /// <param name="target">The user to mention.</param>
        /// <param name="color">The color of the embed.</param>
        /// <param name="contents">The contents of the embed.</param>
        /// <returns></returns>
        public Embed CreateFeedbackEmbed(Snowflake? target, Color color, string contents)
        {
            return target is null
                ? CreateEmbedBase(color) with { Description = contents }
                : CreateEmbedBase(color) with { Description = $"<@{target}> | {contents}" };
        }

        /// <summary>
        /// Creates a base embed.
        /// </summary>
        /// <param name="color">The color of the embed.</param>
        /// <returns></returns>
        public Embed CreateEmbedBase(Color? color = null)
        {
            color ??= EmbedConstants.DefaultColor;

            var eb = new Embed(Colour: color.Value);
            return eb;
        }

        /// <summary>
        /// Sends the given string as one or more sequential embeds, chunked into sets of 1024 characters.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="target"></param>
        /// <param name="color"></param>
        /// <param name="contents"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async ValueTask<Result<IReadOnlyList<IMessage>>> SendContentAsync(
            Snowflake channelId,
            Snowflake? target,
            Color color,
            string contents,
            CancellationToken cancellationToken = default)
        {
            var sendResults = new List<IMessage>();
            foreach (var chunk in CreateContentChunks(target, color, contents))
            {
                var send = await SendEmbedAsync(channelId, chunk, cancellationToken);
                if (!send.IsSuccess)
                {
                    return Result<IReadOnlyList<IMessage>>.FromError(send);
                }

                sendResults.Add(send.Entity);
            }

            return sendResults;
        }

        /// <summary>
        /// Sends the given string as one or more sequential embeds, chunked into sets of 1024 characters
        /// </summary>
        /// <param name="target">The target user to mention, if any</param>
        /// <param name="color">The embed color</param>
        /// <param name="contents">The contents to send</param>
        /// <param name="cancellationToken">The cancellation token for this operation</param>
        /// <returns></returns>
        private async ValueTask<Result<IReadOnlyList<IMessage>>> SendContextualContentAsync(
            Snowflake? target,
            Color color,
            string contents,
            CancellationToken cancellationToken = default)
        {
            var sendResults = new List<IMessage>();
            foreach (var chunk in CreateContentChunks(target, color, contents))
            {
                var send = await SendContextualEmbedAsync(chunk, cancellationToken);
                if (!send.IsSuccess)
                {
                    return Result<IReadOnlyList<IMessage>>.FromError(send);
                }

                sendResults.Add(send.Entity);
            }

            return sendResults;
        }

        /// <summary>
        /// Chunks an input string into one or more embeds. Discord places an internal limit on embed lengths of 2048 characters, and we collapse that into 1024 for readability's sake.
        /// </summary>
        /// <param name="target">The target user, if any</param>
        /// <param name="color">The color of the embed</param>
        /// <param name="contents">The complete contents of the message</param>
        /// <returns></returns>
        private IEnumerable<Embed> CreateContentChunks(Snowflake? target, Color color, string contents)
        {
            // Sometimes the content is > 2048 in length. We'll chunk it into embeds of 1024 here.
            if (contents.Length < 1024)
            {
                yield return CreateFeedbackEmbed(target, color, contents.Trim());
                yield break;
            }

            var words = contents.Split(' ');
            var messageBuilder = new StringBuilder();
            foreach (var word in words)
            {
                if (messageBuilder.Length >= 1024)
                {
                    yield return CreateFeedbackEmbed(target, color, messageBuilder.ToString().Trim());
                    messageBuilder.Clear();
                }

                messageBuilder.Append(word);
                messageBuilder.Append(' ');
            }

            if (messageBuilder.Length > 0)
            {
                yield return CreateFeedbackEmbed(target, color, messageBuilder.ToString().Trim());
            }
        }
    }
}
