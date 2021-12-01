using System.Threading.Tasks;
using Mara.Plugins.Moderation.Models;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mara.Plugins.Moderation.Services
{
    public sealed class UserService
    {
        private readonly ModerationContext _dbContext;

        public UserService(ModerationContext moderationContext)
        {
            _dbContext = moderationContext;
        }

        public Task<Result<UserInformation>> GetUserInformation(IUser user) 
            => GetUserInformation(user.ID);

        public async Task<Result<UserInformation>> GetUserInformation(Snowflake snowflake)
        {
            var result = await _dbContext.UserInfo.FirstOrDefaultAsync(x => x.Id == snowflake);

            if (result == default)
            {
                return new DatabaseValueNotFoundError();
            }

            return Result<UserInformation>.FromSuccess(result!);
        }
    }
}
