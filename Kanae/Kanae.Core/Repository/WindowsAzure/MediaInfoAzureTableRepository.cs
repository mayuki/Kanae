using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;
using Kanae.Data;
using Microsoft.Practices.Unity;

namespace Kanae.Repository.WindowsAzure
{
    public class MediaInfoAzureTableRepository : IMediaInfoRepository
    {
        private CloudTableClient CreateTableClient()
        {
            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            return storageAccount.CreateCloudTableClient();
        }

        private String _connectionString;
        private TimeSpan _retentionTime;
        private DateTime _retentionTimeLimit;
        
        [InjectionConstructor]
        public MediaInfoAzureTableRepository([Dependency("Kanae:RetentionTimeSpan")]TimeSpan retentionTime)
            : this(CloudConfigurationManager.GetSetting("Kanae:WindowsAzure:StorageConnectionString"), retentionTime)
        { }

        public MediaInfoAzureTableRepository(String connectionString, TimeSpan retentionTime)
        {
            _connectionString = connectionString;
            _retentionTime = retentionTime;
            _retentionTimeLimit = (_retentionTime == TimeSpan.MaxValue ? DateTime.MinValue : DateTime.UtcNow - _retentionTime);
        }

        public async Task Initialize()
        {
            var client = CreateTableClient();
            // テーブル作るよ
            var table = client.GetTableReference("MediaInfo");
            await table.CreateIfNotExistsAsync();
        }

        public async Task Create(MediaInfo mediaInfo)
        {
            var client = CreateTableClient();
            
            // テーブル取得するよ
            var table = client.GetTableReference("MediaInfo");

            // データ突っ込むよ
            try
            {
                // RowKeyが01_で始まるのは時刻で(降順)ソートされたデータ、02_で始まるのはGuid
                var tableResultTask = table.ExecuteAsync(TableOperation.InsertOrReplace(new MediaInfoEntity(mediaInfo)));
                var tableSortedResultTask = table.ExecuteAsync(TableOperation.InsertOrReplace(new MediaInfoSortedEntity(mediaInfo)));
                await Task.WhenAll(tableResultTask, tableSortedResultTask);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

        }

        public async Task<MediaInfo> FindByMediaId(Guid mediaId, Boolean ignoreRetentionTime = false)
        {
            var client = CreateTableClient();

            // テーブル取得するよ
            var table = client.GetTableReference("MediaInfo");

            // データ探すよ
            var query = new TableQuery<MediaInfoEntity>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "02_" + mediaId.ToString()));
            var uploadedInfoEntity = (await table.ExecuteQuerySegmentedAsync<MediaInfoEntity>(query, null)).ToList().FirstOrDefault();
            if (uploadedInfoEntity == null || uploadedInfoEntity.CreatedAt < _retentionTimeLimit)
            {
                return null;
            }

            return uploadedInfoEntity.ToMediaInfo();
        }

        public async Task<IEnumerable<MediaInfo>> Find(string userId = null, int take = 0, DateTime? lastDateTime = null, Boolean ignoreRetentionTime = false)
        {
            var client = CreateTableClient();

            // テーブル取得するよ
            var table = client.GetTableReference("MediaInfo");

            // フィルター
            var queryFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, "02_"); // 01_* を探す

            if (userId != null)
            {
                queryFilter = TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId.ToSHA256Hash()),
                    TableOperators.And,
                    queryFilter
                );
            }

            if (lastDateTime.HasValue)
            {
                queryFilter = TableQuery.CombineFilters(
                    queryFilter,
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, "01_" + ConvertDateTimeToRowId(lastDateTime.Value)) // 指定した時刻より前(=IDが大きい)
                );
            }

            if (!ignoreRetentionTime)
            {
                queryFilter = TableQuery.CombineFilters(
                    queryFilter,
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, "01_" + ConvertDateTimeToRowId(_retentionTimeLimit)) // 指定した時刻より後(=IDが小さい)
                );
            }

            // データ探すよ
            return (await table.ExecuteQuerySegmentedAsync<MediaInfoSortedEntity>(new TableQuery<MediaInfoSortedEntity>().Where(queryFilter), null))
                .ToList() // リクエストする
                .Take(take)
                .Select(x => x.ToMediaInfo())
                .ToList();
        }

        public async Task Delete(MediaInfo mediaInfo)
        {
            var client = CreateTableClient();

            // テーブル取得するよ
            var table = client.GetTableReference("MediaInfo");

            // データ探すよ
            var mediaInfoEntity = await table.ExecuteAsync(TableOperation.Retrieve<MediaInfoEntity>(mediaInfo.UserId.ToSHA256Hash(), "02_" + mediaInfo.MediaId.ToString()));
            if (mediaInfoEntity != null)
            {
                await table.ExecuteAsync(TableOperation.Delete(mediaInfoEntity.Result as ITableEntity));
            }
            var mediaInfoSortedEntity = await table.ExecuteAsync(TableOperation.Retrieve<MediaInfoSortedEntity>(mediaInfo.UserId.ToSHA256Hash(), "01_" + ConvertDateTimeToRowId(mediaInfo.CreatedAt)));
            if (mediaInfoSortedEntity != null)
            {
                await table.ExecuteAsync(TableOperation.Delete(mediaInfoSortedEntity.Result as ITableEntity));
            }
        }

        /// <summary>
        /// 日付をRowIdの一部を生成します。(日付が新しいほうが小さい値になる)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private static String ConvertDateTimeToRowId(DateTime dateTime)
        {
            return String.Format("{0:D19}", DateTime.MaxValue.Ticks - dateTime.Ticks);
        }

        public class MediaInfoEntity : TableEntity
        {
            public MediaInfoEntity(MediaInfo mediaInfo)
            {
                PartitionKey = mediaInfo.UserId.ToSHA256Hash();
                RowKey = "02_"+mediaInfo.MediaId.ToString();

                UserId = mediaInfo.UserId;
                MediaId = mediaInfo.MediaId;
                ContentType = mediaInfo.ContentType;
                CreatedAt = mediaInfo.CreatedAt;
            }

            public MediaInfoEntity()
            { }

            public String UserId { get; set; }
            public Guid MediaId { get; set; }
            public String ContentType { get; set; }
            public DateTime CreatedAt { get; set; }

            public MediaInfo ToMediaInfo()
            {
                return new MediaInfo
                {
                    UserId = this.UserId,
                    MediaId = this.MediaId,
                    ContentType = this.ContentType,
                    CreatedAt = this.CreatedAt,
                };
            }
        }

        public class MediaInfoSortedEntity : MediaInfoEntity
        {
            public MediaInfoSortedEntity(MediaInfo mediaInfo) : base(mediaInfo)
            {
                RowKey = "01_" + ConvertDateTimeToRowId(mediaInfo.CreatedAt);
            }

            public MediaInfoSortedEntity()
            { }
        }
    }
}