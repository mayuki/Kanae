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
    public class UserInfoAzureTableRepository : IUserInfoRepository
    {
        private CloudTableClient CreateTableClient()
        {
            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            return storageAccount.CreateCloudTableClient();
        }

        private String _connectionString;
        
        [InjectionConstructor]
        public UserInfoAzureTableRepository()
            : this(CloudConfigurationManager.GetSetting("Kanae:WindowsAzure:StorageConnectionString"))
        { }

        public UserInfoAzureTableRepository(String connectionString)
        {
            _connectionString = connectionString;
        }

        public class UserInfoEntity : TableEntity
        {
            public UserInfoEntity(UserInfo userInfo)
            {
                PartitionKey = userInfo.UserId.ToSHA256Hash(); // そのままだとPartition Keyに許可されていない文字列が入ってしまうので
                RowKey = userInfo.AuthHash;

                UserId = userInfo.UserId;
                AuthHash = userInfo.AuthHash;
            }

            public UserInfoEntity() { }

            public String UserId { get; set; }
            public String AuthHash { get; set; }
        }

        public async Task Initialize()
        {
            var client = CreateTableClient();
            // テーブル作るよ
            var table = client.GetTableReference("UserInfo");
            await table.CreateIfNotExistsAsync();
        }

        public async Task<UserInfo> FindById(string userId)
        {
            var client = CreateTableClient();

            // テーブル取得するよ
            var table = client.GetTableReference("UserInfo");

            // データ探すよ
            var query = new TableQuery<UserInfoEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId.ToSHA256Hash()));
            var userInfoEntity = (await table.ExecuteQuerySegmentedAsync<UserInfoEntity>(query, null)).ToList().FirstOrDefault();
            if (userInfoEntity == null)
            {
                return null;
            }

            return new UserInfo
            {
                UserId = userInfoEntity.UserId,
                AuthHash = userInfoEntity.AuthHash,
            };
        }

        public async Task<UserInfo> FindByAuthHash(string authHash)
        {
            var client = CreateTableClient();

            // テーブル取得するよ
            var table = client.GetTableReference("UserInfo");

            // データ探すよ
            var query = new TableQuery<UserInfoEntity>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, authHash));
            var userInfoEntity = (await table.ExecuteQuerySegmentedAsync<UserInfoEntity>(query, null)).ToList().FirstOrDefault();
            if (userInfoEntity == null)
            {
                return null;
            }

            return new UserInfo
            {
                UserId = userInfoEntity.UserId,
                AuthHash = userInfoEntity.AuthHash,
            };
        }

        public Task<bool> InsertOrUpdate(UserInfo userInfo)
        {
            var client = CreateTableClient();

            // テーブル取得するよ
            var table = client.GetTableReference("UserInfo");

            // データ突っ込むよ
            var tableResult = table.Execute(TableOperation.InsertOrReplace(new UserInfoEntity(userInfo)));

            return Task.FromResult(tableResult != null);
        }
    }
}