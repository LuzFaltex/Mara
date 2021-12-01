using System.Runtime.CompilerServices;
using Mara.Plugins.Core;
using Remora.Plugins.Abstractions.Attributes;

[assembly: RemoraPlugin(typeof(CorePlugin))]
[assembly: InternalsVisibleTo("Mara.Plugins.Core.Tests")]
