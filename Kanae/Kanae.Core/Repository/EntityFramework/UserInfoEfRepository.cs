using Kanae.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanae.Repository.EntityFramework
{
    public class UserInfoEfRepository : IUserInfoRepository
    {
        public Task Initialize()
        {
            return Utility.EmptyTask;
        }

        public Task<UserInfo> FindById(string userId)
        {
            using (var ctx = new EfStorageDbContext())
            {
                return Task.FromResult(
                    ctx.UserInfo
                        .Where(x => x.UserId == userId)
                        .Select(x => new UserInfo() { UserId = x.UserId, AuthHash = x.AuthHash })
                        .FirstOrDefault()
                );
            }
        }

        public Task<UserInfo> FindByAuthHash(string authHash)
        {
            using (var ctx = new EfStorageDbContext())
            {
                return Task.FromResult(
                    ctx.UserInfo
                        .Where(x => x.AuthHash == authHash)
                        .Select(x => new UserInfo() { UserId = x.UserId, AuthHash = x.AuthHash })
                        .FirstOrDefault()
                );
            }
        }

        public async Task<bool> InsertOrUpdate(UserInfo userInfo)
        {
            using (var ctx = new EfStorageDbContext())
            {
                var userInfoEntity = ctx.UserInfo
                            .Where(x => x.UserId == userInfo.UserId)
                            .FirstOrDefault();

                if (userInfoEntity == null)
                {
                    userInfoEntity = new EfStorageDbContext.UserInfoEntity() { UserId = userInfo.UserId };
                    ctx.UserInfo.Add(userInfoEntity);
                }
                userInfoEntity.AuthHash = userInfo.AuthHash;

                return (await ctx.SaveChangesAsync() == 1);
            }
        }
    }
}
