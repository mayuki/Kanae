using Kanae.Data;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanae.Repository.EntityFramework
{
    public class MediaInfoEfRepository : IMediaInfoRepository
    {
        private TimeSpan _retentionTime;
        private DateTime _retentionTimeLimit;

        [InjectionConstructor]
        public MediaInfoEfRepository([Dependency("Kanae:RetentionTimeSpan")]TimeSpan retentionTime)
        {
            _retentionTime = retentionTime;
            _retentionTimeLimit = (_retentionTime == TimeSpan.MaxValue ? new DateTime(1900, 1,1) : DateTime.UtcNow - _retentionTime); // MinValueだと動かなくなる
        }

        public Task Initialize()
        {
            return Utility.EmptyTask;
        }

        public async Task Create(MediaInfo mediaInfo)
        {
            using (var ctx = new EfStorageDbContext())
            {
                var mediaInfoEntity = new EfStorageDbContext.MediaInfoEntity()
                {
                    UserId = mediaInfo.UserId,
                    MediaId = mediaInfo.MediaId,
                    CreatedAt = mediaInfo.CreatedAt,
                    ContentType = mediaInfo.ContentType,
                };
                ctx.MediaInfo.Add(mediaInfoEntity);

                await ctx.SaveChangesAsync();
            }
        }

        public Task<MediaInfo> FindByMediaId(Guid mediaId, Boolean ignoreRetentionTime = false)
        {
            using (var ctx = new EfStorageDbContext())
            {
                return Task.FromResult(
                    ctx.MediaInfo
                        .Where(x => x.MediaId == mediaId && (ignoreRetentionTime ? true : x.CreatedAt > _retentionTimeLimit))
                        .Select(x => new MediaInfo() { UserId = x.UserId, ContentType = x.ContentType, CreatedAt = x.CreatedAt, MediaId = x.MediaId })
                        .FirstOrDefault()
                );
            }
        }

        public Task<IEnumerable<MediaInfo>> Find(string userId = null, int take = 0, DateTime? lastDateTime = null, Boolean ignoreRetentionTime = false)
        {
            using (var ctx = new EfStorageDbContext())
            {
                IQueryable<EfStorageDbContext.MediaInfoEntity> query = ctx.MediaInfo.OrderByDescending(x => x.CreatedAt);
                if (userId != null)
                {
                    query = query.Where(x => x.UserId == userId);
                }

                if (lastDateTime.HasValue)
                {
                    query = query.Where(x => x.CreatedAt < lastDateTime);
                }

                if (!ignoreRetentionTime)
                {
                    query = query.Where(x => x.CreatedAt > _retentionTimeLimit);
                }

                return Task.FromResult<IEnumerable<MediaInfo>>(
                    query
                        .Take(take)
                        .ToList()
                        .Select(x => new MediaInfo() { UserId = x.UserId, ContentType = x.ContentType, CreatedAt = x.CreatedAt, MediaId = x.MediaId })
                        .ToList()
                );
            }
        }

        public async Task Delete(MediaInfo mediaInfo)
        {
            using (var ctx = new EfStorageDbContext())
            {
                var target = ctx.MediaInfo.FirstOrDefault(x => x.MediaId == mediaInfo.MediaId);

                if (target != null)
                {
                    ctx.MediaInfo.Remove(target);
                }

                await ctx.SaveChangesAsync();
            }
        }
    }
}
