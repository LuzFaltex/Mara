using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Remora.Plugins.Abstractions;
using Remora.Results;

namespace Mara.Plugins.Consent
{
    /// <summary>
    /// A plugin which keeps track of user consents on a global basis.
    /// </summary>
    public sealed class ConsentPlugin : PluginDescriptor, IMigratablePlugin
    {
        public override string Name => "Consent";

        /// <inheritdoc />
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);

        public override string Description => "Retrieves and stores user consent on a global basis.";

        public override Result ConfigureServices(IServiceCollection serviceCollection)
        {
            return base.ConfigureServices(serviceCollection);
        }

        public Task<Result> MigrateAsync(IServiceProvider serviceProvider, CancellationToken ct = default)
        {
            return Task.FromResult(Result.FromSuccess());
        }
    }
}
