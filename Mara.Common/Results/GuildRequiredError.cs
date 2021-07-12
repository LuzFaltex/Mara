using Remora.Results;

namespace Mara.Common.Results
{
    public record GuildRequiredError() : ResultError("This must be executed in a guild context.");
}
