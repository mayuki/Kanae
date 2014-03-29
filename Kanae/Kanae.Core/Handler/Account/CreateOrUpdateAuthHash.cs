using Kanae.Data;
using Kanae.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanae.Handler.Account
{
    /// <summary>
    /// 認証キーを生成または更新します。
    /// </summary>
    public class CreateOrUpdateAuthHash
    {
        private IUserInfoRepository _userInfoRepository;
        public CreateOrUpdateAuthHash(IUserInfoRepository userInfoRepository)
        {
            _userInfoRepository = userInfoRepository;
        }

        public async Task<UserInfo> Execute(String userId)
        {
            var userInfo = new UserInfo
            {
                UserId = userId,
                AuthHash = (userId + Guid.NewGuid().ToString()).ToSHA256Hash()
            };

            return await _userInfoRepository.InsertOrUpdate(userInfo) ? userInfo : null;
        }
    }
}
