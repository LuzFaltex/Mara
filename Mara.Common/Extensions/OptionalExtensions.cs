using System.Diagnostics.CodeAnalysis;
using Remora.Discord.Core;

namespace Mara.Common.Extensions
{
    public static class OptionalExtensions
    {
        public static TValue GetValueOrDefault<TValue>(this Optional<TValue> optional, [NotNull] TValue defaultValue)
            => optional.HasValue
                ? optional.Value
                : defaultValue;
    }
}
