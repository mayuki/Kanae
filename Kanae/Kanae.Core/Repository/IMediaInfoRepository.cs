using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Kanae.Data;

namespace Kanae.Repository
{
    public interface IMediaInfoRepository
    {
        Task Initialize();
        Task Create(MediaInfo mediaInfo);
        Task<MediaInfo> FindByMediaId(Guid mediaId, Boolean ignoreRetentionTime = false);
        Task<IEnumerable<MediaInfo>> Find(String userId = null, Int32 take = 0, DateTime? lastDateTime = null, Boolean ignoreRetentionTime = false);
        Task Delete(MediaInfo mediaInfo);
    }
}