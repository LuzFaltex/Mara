using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Results;

namespace Mara.Plugins.BetterEmbeds.Services
{
    public interface IQuoteService
    {
        /// <summary>
        /// Build an embed quote for the provided message. Returns null if the message could not be quoted.
        /// </summary>
        /// <param name="message">The message to quote.</param>
        /// <param name="executingUser">The user that is doing the quoting.</param>
        Task<Result<IEmbed>> BuildQuoteEmbedAsync(IMessage message, IUser executingUser);
    }
}
