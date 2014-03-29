using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Kanae.Data;

namespace Kanae.Repository
{
    public interface IUserInfoRepository
    {
        Task Initialize();
        Task<UserInfo> FindById(String userId);
        Task<UserInfo> FindByAuthHash(String authHash);
        Task<Boolean> InsertOrUpdate(UserInfo userInfo);
    }
}