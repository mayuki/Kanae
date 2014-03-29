using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Kanae.Data;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanae.Repository.Amazon
{
    public class MediaS3Repository : IMediaRepository
    {
        public Boolean IsRemoteContentMode { get; set; }

        private String _regionName;
        private String _bucketName;
        private String _accessKeyId;
        private String _securetAccessKey;
        private TimeSpan _retentionTime;

        private AmazonS3Client _client;

        [InjectionConstructor]
        public MediaS3Repository([Dependency("Kanae:RetentionTimeSpan")]TimeSpan retentionTime)
            : this(
                ConfigurationManager.AppSettings["Kanae:AmazonS3:RegionName"],
                ConfigurationManager.AppSettings["Kanae:AmazonS3:BucketName"],
                ConfigurationManager.AppSettings["Kanae:AmazonS3:AccessKeyId"],
                ConfigurationManager.AppSettings["Kanae:AmazonS3:SecretAccessKey"],
                Boolean.Parse(ConfigurationManager.AppSettings["Kanae:AmazonS3:IsRemoteContentMode"]),
                retentionTime
            )
        { }

        public MediaS3Repository(String regionName, String bucketName, String accessKeyId, String secretAccessKey, Boolean isRemoteContentMode, TimeSpan retentionTime)
        {
            _regionName = regionName;
            _bucketName = bucketName;
            _accessKeyId = accessKeyId;
            _securetAccessKey = secretAccessKey;
            _retentionTime = retentionTime;

            IsRemoteContentMode = isRemoteContentMode;

            _client = new AmazonS3Client(_accessKeyId, _securetAccessKey, new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(_regionName) });
        }

        public Task Initialize()
        {
            return Utility.EmptyTask;
        }

        public Task Create(MediaInfo mediaInfo, byte[] data)
        {
            var putObject = new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = mediaInfo.UserId.ToSHA256Hash() + "/" + mediaInfo.MediaId.ToString(),
                ContentType = mediaInfo.ContentType,
                InputStream = new MemoryStream(data),
            };
            return _client.PutObjectAsync(putObject);
        }

        public Task Update(MediaInfo mediaInfo, byte[] data)
        {
            return Create(mediaInfo, data);
        }

        public Task<IMediaContent> Get(MediaInfo mediaInfo)
        {
            var key = mediaInfo.UserId.ToSHA256Hash() + "/" + mediaInfo.MediaId.ToString();

            return Task.FromResult(
                IsRemoteContentMode
                    ? new S3RemoteMediaContent(_client, _bucketName, key, (_retentionTime == TimeSpan.MaxValue ? DateTime.UtcNow.AddHours(24) : mediaInfo.CreatedAt + _retentionTime)) as IMediaContent
                    : new S3MediaContent(_client, _bucketName, key) as IMediaContent
            );
        }

        public Task Delete(MediaInfo mediaInfo)
        {
            return _client.DeleteObjectAsync(new DeleteObjectRequest()
            {
                BucketName = _bucketName,
                Key = mediaInfo.UserId.ToSHA256Hash() + "/" + mediaInfo.MediaId.ToString(),
            });
        }

        public class S3MediaContent : IMediaContent
        {
            protected AmazonS3Client _client;
            protected GetPreSignedUrlRequest _request;
            protected String _bucketName;
            protected String _key;

            public S3MediaContent(AmazonS3Client client, String bucketName, String key)
            {
                _client = client;
                _bucketName = bucketName;
                _key = key;
            }

            public async Task<byte[]> GetContentAsync()
            {
                var response = await _client.GetObjectAsync(new GetObjectRequest() { BucketName = _bucketName, Key = _key });
                var data = new Byte[response.ContentLength];

                var pos = 0;
                var len = 0;
                while ((len = await response.ResponseStream.ReadAsync(data, pos, data.Length - pos)) != 0)
                {
                    pos += len;
                }

                return data;
            }
        }

        public class S3RemoteMediaContent : S3MediaContent, IRemoteMediaContent
        {
            private DateTime _expiryTime;

            public S3RemoteMediaContent(AmazonS3Client client, String bucketName, String key, DateTime expiryTime)
                : base(client, bucketName, key)
            {
                _expiryTime = expiryTime;
            }

            public string GetUrl()
            {
                return _client.GetPreSignedURL(new GetPreSignedUrlRequest() { BucketName = _bucketName, Key = _key, Expires = _expiryTime });
            }
        }
    }
}
