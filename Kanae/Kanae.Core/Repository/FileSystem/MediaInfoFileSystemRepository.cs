using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Kanae.Data;
using Microsoft.Practices.Unity;
using System.Configuration;
using System.Web.Hosting;

namespace Kanae.Repository.FileSystem
{
    public class MediaInfoFileSystemRepository : IMediaInfoRepository
    {
        private String _dataDirectory;
        private TimeSpan _retentionTime;
        private DateTime _retentionTimeLimit;

        [InjectionConstructor]
        public MediaInfoFileSystemRepository([Dependency("Kanae:RetentionTimeSpan")]TimeSpan retentionTime)
            : this(HostingEnvironment.MapPath(ConfigurationManager.AppSettings["Kanae:FileSystem:MediaDataDirectory"]), retentionTime)
        { }

        public MediaInfoFileSystemRepository(String dataDirectory, TimeSpan retentionTime)
        {
            _dataDirectory = dataDirectory;
            _retentionTime = retentionTime;
            _retentionTimeLimit = (_retentionTime == TimeSpan.MaxValue ? DateTime.MinValue : DateTime.UtcNow - _retentionTime);
        }

        public Task Initialize()
        {
            return Utility.EmptyTask;
        }

        public Task Create(MediaInfo mediaInfo)
        {
            // なんにもしない…
            return Utility.EmptyTask;
        }

        public Task<MediaInfo> FindByMediaId(Guid mediaId, Boolean ignoreRetentionTime = false)
        {
            var mediaIdString = mediaId.ToString();
            var mediaInfo = Directory.GetDirectories(_dataDirectory)
                                    .Select(x => Directory.GetFiles(x, mediaIdString + ".*").FirstOrDefault())
                                    .Where(x => x != null)
                                    .Select(CreateMediaInfoFromFilePath)
                                    .Where(x => (ignoreRetentionTime ? true : x.CreatedAt > _retentionTimeLimit))
                                    .FirstOrDefault();
            return Task.FromResult(mediaInfo);
        }

        public Task<IEnumerable<MediaInfo>> Find(String userId = null, Int32 take = 15, DateTime? lastDateTime = null, Boolean ignoreRetentionTime = false)
        {
            var dir = String.IsNullOrWhiteSpace(userId) ? _dataDirectory : Path.Combine(_dataDirectory, Uri.EscapeDataString(userId));
            if (!Directory.Exists(dir))
                return Task.FromResult(Enumerable.Empty<MediaInfo>());

            return Task.FromResult<IEnumerable<MediaInfo>>(
                Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)
                    .Where(x => x.EndsWith(".png") || x.EndsWith(".jpg"))
                    .Select(x => new { Path = x, CreatedAt = File.GetCreationTimeUtc(x) })
                    .OrderByDescending(x => x.CreatedAt)
                    .Where(x => lastDateTime.HasValue ? x.CreatedAt < lastDateTime : true)
                    .Where(x => (ignoreRetentionTime ? true : x.CreatedAt > _retentionTimeLimit))
                    .Take(take)
                    .Select(x => CreateMediaInfoFromFilePath(x.Path))
                    .ToList()
            );
        }

        public Task Delete(MediaInfo mediaInfo)
        {
            // なんにもしない…
            return Utility.EmptyTask;
        }

        private MediaInfo CreateMediaInfoFromFilePath(String filePath)
        {
            return new MediaInfo
            {
                ContentType = filePath.EndsWith(".png") ? "image/png" : filePath.EndsWith(".jpg") ? "image/jpeg" : "image/png",
                MediaId = Guid.Parse(Path.GetFileNameWithoutExtension(filePath)),
                UserId = Uri.UnescapeDataString(Path.GetFileName(Path.GetDirectoryName(filePath))),
                CreatedAt = File.GetCreationTimeUtc(filePath),
            };
        }

        private String GetExtensionFromContentType(String contentType)
        {
            return (contentType == "image/png")
                        ? ".png"
                 : (contentType == "image/pjpeg" || contentType == "image/jpeg")
                        ? ".jpg"
                : ".bin";
        }

        private String GetPath(MediaInfo mediaInfo)
        {
            return Path.Combine(_dataDirectory, Uri.EscapeDataString(mediaInfo.UserId), mediaInfo.MediaId + (GetExtensionFromContentType(mediaInfo.ContentType)));
        }
    }
}