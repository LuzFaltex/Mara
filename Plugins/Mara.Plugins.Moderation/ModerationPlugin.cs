using System;
using System.Reflection;
using System.Threading.Tasks;
using Mara.Plugins.Moderation;
using Microsoft.Extensions.DependencyInjection;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Abstractions.Attributes;
using Remora.Results;

[assembly:RemoraPlugin(typeof(ModerationPlugin))]

namespace Mara.Plugins.Moderation
{
    public sealed class ModerationPlugin : PluginDescriptor, IMigratablePlugin
    {
        public override string Name => "Moderation";

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
        public override string Description => "Provides extended moderation features for staff members.";

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            base.ConfigureServices(serviceCollection);
        }

        public override ValueTask<Result> InitializeAsync(IServiceProvider serviceProvider)
        {
            return base.InitializeAsync(serviceProvider);
        }

        public async Task<Result> MigratePluginAsync(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> HasCreatedPersistentStoreAsync(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}
