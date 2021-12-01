using System.Reflection;
using Mara.Plugins.Moderation.Models;
using Microsoft.EntityFrameworkCore;

namespace Mara.Plugins.Moderation
{
    public sealed class ModerationContext : DbContext
    {
        public DbSet<UserInformation> UserInfo { get; }

        public ModerationContext(DbContextOptions<ModerationContext> options) : base(options)
        {

        }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(builder);
        }
    }
}
