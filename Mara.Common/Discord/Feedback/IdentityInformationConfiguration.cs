using Remora.Discord.Core;

namespace Mara.Common.Discord.Feedback
{
    public sealed class IdentityInformationConfiguration
    {
        /// <summary>
        /// Gets the Id of the bot.
        /// </summary>
        public Snowflake Id { get; set; }

        /// <summary>
        /// Gets the application id if the bot.
        /// </summary>
        public Snowflake ApplicationId { get; set; }

        /// <summary>
        /// Gets the id of the bot's owner.
        /// </summary>
        public Snowflake OwnerId { get; set; }
    }
}
