using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Kanae.Data;
using System.Configuration;
using Microsoft.Practices.Unity;
using System.Web.Hosting;

namespace Kanae.Repository.FileSystem
{
    public class MediaFileSystemRepository : IMediaRepository
    {
        private String _dataDirectory;

        [InjectionConstructor]
        public MediaFileSystemRepository()
            : this(HostingEnvironment.MapPath(ConfigurationManager.AppSettings["Kanae:FileSystem:MediaDataDirectory"]))
        { }

        public MediaFileSystemRepository(String dataDirectory)
        {
            _dataDirectory = dataDirectory;
        }

        public Task Initialize()
        {
            return Utility.EmptyTask;
        }

        public Task Create(MediaInfo mediaInfo, byte[] data)
        {
            var filePath = GetPath(mediaInfo);
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllBytes(filePath, data);

            return Utility.EmptyTask;
        }

        public Task Update(MediaInfo mediaInfo, byte[] data)
        {
            return Create(mediaInfo, data);
        }

        public Task<IMediaContent> Get(MediaInfo mediaInfo)
        {
            var filePath = GetPath(mediaInfo);
            if (!File.Exists(filePath))
                return null;

            return Task.FromResult<IMediaContent>(new FileSystemMediaContent(filePath));
        }

        public Task Delete(MediaInfo mediaInfo)
        {
            var filePath = GetPath(mediaInfo);
            if (!File.Exists(filePath))
                return Utility.EmptyTask;

            File.Delete(filePath);

            return Utility.EmptyTask;
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

        public class FileSystemMediaContent : IMediaContent
        {
            public String Path { get; private set; }
            public FileSystemMediaContent(String path)
            {
                Path = path;
            }
            public Task<Byte[]> GetContentAsync()
            {
                return Task.FromResult(File.ReadAllBytes(Path));
            }
        }
    }
}