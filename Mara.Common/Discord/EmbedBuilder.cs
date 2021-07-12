using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using Mara.Common.Extensions;
using Mara.Common.Results;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mara.Common.Discord
{
    public class EmbedBuilder
    {
        [MaxLength(EmbedConstants.MaxTitleLength)]
        public string Title { get; set; }

        public EmbedType Type { get; set; }
        [MaxLength(EmbedConstants.MaxDescriptionLength)]
        public string Description { get; set; }
        [Url]
        public string Url { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public Color Color { get; set; }
        public IEmbedFooter Footer { get; set; }
        public IEmbedImage Image { get; set; }
        public IEmbedThumbnail Thumbnail { get; set; }
        public IEmbedVideo Video { get; set; }
        public IEmbedProvider Provider { get; set; }
        public IEmbedAuthor Author { get; set; }
        public IReadOnlyList<IEmbedField> Fields => new ReadOnlyCollection<IEmbedField>(_fields);

        private IList<IEmbedField> _fields;

        public EmbedBuilder() : this(new List<IEmbedField>(EmbedConstants.MaxFieldCount))
        {
        }

        private EmbedBuilder(Optional<IReadOnlyList<IEmbedField>> fields)
            : this(fields.HasValue ? new List<IEmbedField>(fields.Value) : new List<IEmbedField>(EmbedConstants.MaxFieldCount))
        {
        }

        private EmbedBuilder(List<IEmbedField> fields)
        {
            Title = string.Empty;
            Type = EmbedType.Rich;
            Description = string.Empty;
            Url = string.Empty;
            Timestamp = DateTimeOffset.UtcNow;
            Color = EmbedConstants.DefaultColor;
            Footer = EmbedConstants.DefaultFooter;
            Image = null;
            Thumbnail = null;
            Video = null;
            Provider = null;
            _fields = fields;
        }

        public static EmbedBuilder FromEmbed(Embed embed)
            => new(embed.Fields)
            {
                Title = embed.Title.GetValueOrDefault(string.Empty),
                Type = embed.Type.GetValueOrDefault(EmbedType.Rich),
                Description = embed.Description.GetValueOrDefault(string.Empty),
                Url = embed.Url.GetValueOrDefault(string.Empty),
                Timestamp = embed.Timestamp.GetValueOrDefault(DateTimeOffset.UtcNow),
                Color = embed.Colour.GetValueOrDefault(EmbedConstants.DefaultColor),
                Footer = embed.Footer.GetValueOrDefault(EmbedConstants.DefaultFooter),
                Image = embed.Image.GetValueOrDefault(default),
                Thumbnail = embed.Thumbnail.GetValueOrDefault(default),
                Video = embed.Video.GetValueOrDefault(default),
                Provider = embed.Provider.GetValueOrDefault(default)
            };

        public int Length
        {
            get
            {
                int titleLength = Title.Length;
                int authorLength = Author.Name.HasValue
                    ? Author.Name.Value!.Length
                    : 0;
                int descriptionLength = Description.Length;
                int footerLength = Footer?.Text.Length ?? 0;
                int fieldSum = _fields.Sum(field => field.Name.Length + field.Value.Length);

                return titleLength + authorLength + descriptionLength + footerLength + fieldSum;
            }
        }

        /// <summary>
        /// Ensures the resultant embed would be valid (less than <see cref="EmbedConstants.MaxEmbedLength"/>).
        /// </summary>
        /// <returns><c>True</c> if valid; otherwise, <c>False</c>.</returns>
        public Result Ensure()
        {
            return Length < EmbedConstants.MaxEmbedLength
                ? Result.FromSuccess()
                : Result.FromError(new EmbedError("Embed is too long."));
        }

        public EmbedBuilder WithTitle(string title)
        {
            Title = title;
            return this;
        }
        /// <summary> 
        ///     Sets the description of an <see cref="Embed"/>.
        /// </summary>
        /// <param name="description"> The description to be set. </param>
        /// <returns>
        ///     The current builder.
        /// </returns>
        public EmbedBuilder WithDescription(string description)
        {
            Description = description;
            return this;
        }
        /// <summary> 
        ///     Sets the URL of an <see cref="Embed"/>.
        /// </summary>
        /// <param name="url"> The URL to be set. </param>
        /// <returns>
        ///     The current builder.
        /// </returns>
        public EmbedBuilder WithUrl(string url)
        {
            Url = url;
            return this;
        }
        /// <summary> 
        ///     Sets the thumbnail URL of an <see cref="Embed"/>.
        /// </summary>
        /// <param name="thumbnailUrl"> The thumbnail URL to be set. </param>
        /// <returns>
        ///     The current builder.
        /// </returns>
        public EmbedBuilder WithThumbnailUrl(string thumbnailUrl)
        {
            Thumbnail = new EmbedThumbnail(Url: thumbnailUrl);
            return this;
        }
        /// <summary>
        ///     Sets the image URL of an <see cref="Embed"/>.
        /// </summary>
        /// <param name="imageUrl">The image URL to be set.</param>
        /// <returns>
        ///     The current builder.
        /// </returns>
        public EmbedBuilder WithImageUrl(string imageUrl)
        {
            Image = new EmbedImage(Url: imageUrl);
            return this;
        }
        /// <summary>
        ///     Sets the timestamp of an <see cref="Embed" /> to the current time.
        /// </summary>
        /// <returns>
        ///     The current builder.
        /// </returns>
        public EmbedBuilder WithCurrentTimestamp()
        {
            Timestamp = DateTimeOffset.UtcNow;
            return this;
        }
        /// <summary>
        ///     Sets the timestamp of an <see cref="Embed"/>.
        /// </summary>
        /// <param name="dateTimeOffset">The timestamp to be set.</param>
        /// <returns>
        ///     The current builder.
        /// </returns>
        public EmbedBuilder WithTimestamp(DateTimeOffset dateTimeOffset)
        {
            Timestamp = dateTimeOffset;
            return this;
        }
        /// <summary>
        ///     Sets the sidebar color of an <see cref="Embed"/>.
        /// </summary>
        /// <param name="color">The color to be set.</param>
        /// <returns>
        ///     The current builder.
        /// </returns>
        public EmbedBuilder WithColor(Color colour)
        {
            Color = colour;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="EmbedAuthor" /> of an <see cref="Embed"/>.
        /// </summary>
        /// <returns>
        ///     The current builder.
        /// </returns>
        public EmbedBuilder WithAuthor([MaxLength(EmbedConstants.MaxAuthorNameLength)] string name, [Url] string url = default, [Url] string iconUrl = default, [Url] string proxyIconUrl = default)
        {
            Author = new EmbedAuthor(name, url, iconUrl, proxyIconUrl);
            return this;
        }

        public EmbedBuilder WithUserAsAuthor(IUser user)
        {
            var avatarUrlResult = CDN.GetUserAvatarUrl(user, imageSize: 256);

            var avatarUrl = avatarUrlResult.IsSuccess
                ? avatarUrlResult.Entity
                : CDN.GetDefaultUserAvatarUrl(user, imageSize: 256).Entity;

            Author = new EmbedAuthor($"{user.Username}#{user.Discriminator}", avatarUrl!.AbsoluteUri);
            return this;
        }

        /// <summary>
        /// Sets the footer field of an <see cref="Embed" /> with the provided name, icon URL.
        /// </summary>
        /// <param name="text">The title of the footer field.</param>
        /// <param name="iconUrl">The icon URL of the footer field.</param>
        /// <returns>
        ///     The current builder.
        /// </returns>
        public EmbedBuilder WithFooter(string text, [Url] string iconUrl = default, [Url] string proxyIconUrl = default)
        {
            if (text.Length > EmbedConstants.MaxFooterTextLength)
                throw new ArgumentException(
                    $"Footer length must be less than or equal to {EmbedConstants.MaxFooterTextLength}", nameof(text));

            return WithFooter(new EmbedFooter(text, iconUrl, proxyIconUrl));
        }

        public EmbedBuilder WithFooter(IEmbedFooter footer)
        {
            Footer = footer;
            return this;
        }

        /// <summary>
        ///     Adds an <see cref="Embed" /> field with the provided name and value.
        /// </summary>
        /// <param name="name">The title of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <param name="inline">Indicates whether the field is in-line or not.</param>
        /// <returns>
        ///     The current builder.
        /// </returns>
        public EmbedBuilder AddField(string name, string value, bool inline = false)
        {
            return AddField(new EmbedField(name, value, inline));
        }

        /// <summary>
        ///     Adds a field with the provided <see cref="EmbedField" /> to an
        ///     <see cref="Embed"/>.
        /// </summary>
        /// <param name="field">The field builder class containing the field properties.</param>
        /// <exception cref="ArgumentException">Field count exceeds <see cref="MaxFieldCount"/>.</exception>
        /// <returns>
        ///     The current builder.
        /// </returns>
        public EmbedBuilder AddField(EmbedField field)
        {
            if (_fields.Count >= EmbedConstants.MaxFieldCount)
            {
                throw new ArgumentException(message: $"Field count must be less than or equal to {EmbedConstants.MaxFieldCount}.", paramName: nameof(field));
            }

            _fields.Add(field);
            return this;
        }

        public EmbedBuilder SetFields(IList<IEmbedField> fields)
        {
            if (fields.Count >= EmbedConstants.MaxFieldCount)
            {
                throw new ArgumentException(message: $"Field count must be less than or equal to {EmbedConstants.MaxFieldCount}.", paramName: nameof(fields));
            }

            _fields = fields;
            return this;
        }

        /// <summary>
        ///     Builds the <see cref="Embed" /> into a Rich Embed ready to be sent.
        /// </summary>
        /// <returns>
        ///     The built embed object.
        /// </returns>
        /// <exception cref="InvalidOperationException">Total embed length exceeds <see cref="MaxEmbedLength"/>.</exception>
        public Embed Build()
            => Length > EmbedConstants.MaxEmbedLength
                ? throw new InvalidOperationException(
                    $"Total embed length must be less than or equal to {EmbedConstants.MaxEmbedLength}.")
                : new Embed(
                    Title,
                    Type,
                    Description,
                    Url,
                    Timestamp,
                    Color,
                    !string.IsNullOrEmpty(Footer.Text) ? new Optional<IEmbedFooter>(Footer) : new Optional<IEmbedFooter>(),
                    new Optional<IEmbedImage>(Image),
                    new Optional<IEmbedThumbnail>(Thumbnail),
                    new Optional<IEmbedVideo>(Video),
                    new Optional<IEmbedProvider>(Provider),
                    new Optional<IEmbedAuthor>(Author),
                    new Optional<IReadOnlyList<IEmbedField>>(Fields));
    }
}
