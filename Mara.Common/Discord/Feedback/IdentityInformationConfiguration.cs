using System;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Core;

namespace Mara.Common.Discord.Feedback
{
    /// <summary>
    /// Provides information about the identity of the current application.
    /// </summary>
    public sealed class IdentityInformationConfiguration
    {
        /// <summary>
        /// Gets the Id of the bot.
        /// </summary>
        public Snowflake Id { get; set; }

        /// <summary>
        /// Gets the application behind this bot.
        /// </summary>
        public IApplication Application { get; set; }
    }
}
