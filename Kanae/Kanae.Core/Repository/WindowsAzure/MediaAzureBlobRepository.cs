using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Kanae.Data;
using Microsoft.Practices.Unity;

namespace Kanae.Repository.WindowsAzure
{
    public class MediaAzureBlobRepository : IMediaRepository
    {
        private CloudBlobClient CreateBlobClient()
        {
            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            return storageAccount.CreateCloudBlobClient();
        }

        private String _connectionString;
        private TimeSpan _retentionTime;

        /// <summary>
        /// AzureのStorage BlobのURLを指すものをコンテントとして返すかどうかを取得、設定します。
        /// </summary>
        public Boolean IsRemoteContentMode { get; set; }

        [InjectionConstructor]
        public MediaAzureBlobRepository([Dependency("Kanae:RetentionTimeSpan")]TimeSpan retentionTime)
            : this(CloudConfigurationManager.GetSetting("Kanae:WindowsAzure:StorageConnectionString"),
                   Boolean.Parse(CloudConfigurationManager.GetSetting("Kanae:WindowsAzure:IsRemoteContentMode")),
                   retentionTime)
        {
        }

        public MediaAzureBlobRepository(String connectionString, Boolean isRemoteContentMode, TimeSpan retentionTime)
        {
            _connectionString = connectionString;
            _retentionTime = retentionTime;

            IsRemoteContentMode = isRemoteContentMode;
        }

        public async Task Initialize()
        {
            var client = CreateBlobClient();
            
            // コンテナを取得してなければ作る(匿名での読み取りは可で)
            var container = client.GetContainerReference("uploaded-images");
            await container.CreateIfNotExistsAsync();
            await container.SetPermissionsAsync(new BlobContainerPermissions() { /*PublicAccess = BlobContainerPublicAccessType.Blob*/ });
        }

        public async Task Create(MediaInfo mediaInfo, byte[] data)
        {
            var client = CreateBlobClient();

            // コンテナを取得
            var container = client.GetContainerReference("uploaded-images");
            
            // UserIdをハッシュしてディレクトリ作る
            var dirRef = container.GetDirectoryReference(mediaInfo.UserId.ToSHA256Hash());

            // Blobのリファレンスをゲットして、ファイルアップロード
            var blobRef = dirRef.GetBlockBlobReference(mediaInfo.MediaId.ToString());
            await blobRef.UploadFromByteArrayAsync(data, 0, data.Length);
            blobRef.Properties.ContentType = mediaInfo.ContentType;
            await blobRef.SetPropertiesAsync();
        }

        public Task Update(MediaInfo mediaInfo, byte[] data)
        {
            return Create(mediaInfo, data);
        }

        public Task<IMediaContent> Get(MediaInfo mediaInfo)
        {
            var client = CreateBlobClient();
            var container = client.GetContainerReference("uploaded-images");
            var dirRef = container.GetDirectoryReference(mediaInfo.UserId.ToSHA256Hash());
            var blobRef = dirRef.GetBlockBlobReference(mediaInfo.MediaId.ToString());

            return Task.FromResult<IMediaContent>(
                IsRemoteContentMode
                ? new AzureBlobRemoteMediaContent(blobRef, (_retentionTime == TimeSpan.MaxValue ? DateTime.UtcNow.AddHours(24) : mediaInfo.CreatedAt + _retentionTime)) as IMediaContent
                    : new AzureBlobMediaContent(blobRef) as IMediaContent
            );
        }

        public async Task Delete(MediaInfo mediaInfo)
        {
            var client = CreateBlobClient();
            var container = client.GetContainerReference("uploaded-images");
            var dirRef = container.GetDirectoryReference(mediaInfo.UserId.ToSHA256Hash());
            var blobRef = dirRef.GetBlockBlobReference(mediaInfo.MediaId.ToString());

            await blobRef.DeleteIfExistsAsync();
        }

        public class AzureBlobMediaContent : IMediaContent
        {
            protected CloudBlockBlob _blob;
            public AzureBlobMediaContent(CloudBlockBlob blob)
            {
                _blob = blob;
            }

            public async Task<byte[]> GetContentAsync()
            {
                await _blob.FetchAttributesAsync();
                var bytes = new Byte[_blob.Properties.Length];
                await _blob.DownloadToByteArrayAsync(bytes, 0);

                return bytes;
            }
        }

        public class AzureBlobRemoteMediaContent : AzureBlobMediaContent, IRemoteMediaContent
        {
            private DateTime _expiryTime;
            public AzureBlobRemoteMediaContent(CloudBlockBlob blob, DateTime expiryTime)
                : base(blob)
            {
                _expiryTime = expiryTime;
            }

            public string GetUrl()
            {
                var sasToken = _blob.Container.GetSharedAccessSignature(new SharedAccessBlobPolicy() {
                    SharedAccessExpiryTime = _expiryTime,
                    Permissions = SharedAccessBlobPermissions.Read
                });
                return _blob.Uri.ToString() + sasToken;
            }
        }
    }
}