using Remora.Results;

namespace Mara.Common.Results
{
    /// <summary>
    /// Represents an error where no database models could be found with the provided query.
    /// </summary>
    public sealed record DatabaseValueNotFoundError() : ResultError("The requested database model(s) could not be found with the provided query.");
}
