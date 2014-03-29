using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using Kanae.Data;
using Microsoft.Practices.Unity;
using System.Configuration;
using System.Web.Hosting;

namespace Kanae.Repository.FileSystem
{
    public class UserInfoFileSystemRepository : IUserInfoRepository
    {
        private static XmlSerializer _xmlSerializer;
        private String _dataDirectory;

        [InjectionConstructor]
        public UserInfoFileSystemRepository()
            : this(HostingEnvironment.MapPath(ConfigurationManager.AppSettings["Kanae:FileSystem:DatabaseDataDirectory"]))
        { }

        public UserInfoFileSystemRepository(String dataDirectory)
        {
            _dataDirectory = dataDirectory;
        }
        static UserInfoFileSystemRepository()
        {
            _xmlSerializer = new XmlSerializer(typeof(List<UserInfo>));
        }

        public Task Initialize()
        {
            return Utility.EmptyTask;
        }

        public Task<UserInfo> FindById(string userId)
        {
            return Task.FromResult(GetAllFromFile().Where(x => x.UserId == userId).FirstOrDefault());
        }

        public Task<UserInfo> FindByAuthHash(string authHash)
        {
            return Task.FromResult(GetAllFromFile().Where(x => x.AuthHash == authHash).FirstOrDefault());
        }

        public Task<bool> InsertOrUpdate(UserInfo userInfo)
        {
            var userInfoDict = GetAllFromFile().ToDictionary(x => x.UserId);
            userInfoDict[userInfo.UserId] = userInfo;
            SaveToFile(userInfoDict.Values);

            return Task.FromResult(true);
        }

        private void SaveToFile(IEnumerable<UserInfo> userInfo)
        {
            lock (_xmlSerializer)
            {
                var path = Path.Combine(_dataDirectory, "UserInfoList.xml");
                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(path)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }

                    using (var stream = File.OpenWrite(path))
                    {
                        _xmlSerializer.Serialize(stream, userInfo.ToList());
                    }
                }
                catch
                {
                }
            }
        }

        private IEnumerable<UserInfo> GetAllFromFile()
        {
            lock(_xmlSerializer)
            {
                var path = Path.Combine(_dataDirectory, "UserInfoList.xml");
                if (!File.Exists(path))
                    return Enumerable.Empty<UserInfo>();

                try
                {
                    using (var stream = File.OpenRead(path))
                    {
                        var userInfoList = _xmlSerializer.Deserialize(stream) as List<UserInfo>;
                        return userInfoList;
                    }
                }
                catch
                {
                    return Enumerable.Empty<UserInfo>();
                }
            }
        }
    }
}