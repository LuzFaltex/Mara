using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Mara.Common.Extensions
{
    public static class LoggerExtensions
    {
        /// <summary>
        /// Formats and writes an error log message
        /// </summary>
        /// <typeparam name="T">The object this logger writes for.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="error">An <see cref="IResultError"/> representing the problem that occurred.</param>
        public static void LogError<T>(this ILogger<T> logger, IResultError error)
        {
            if (error is ExceptionError exceptionError)
            {
                logger.LogError(exceptionError.Exception, exceptionError.Message);
            }

            logger.LogError(error.Message);
        }
    }
}
