using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanae.Repository.EntityFramework
{
    public class EfStorageDbContext : DbContext
    {
        public DbSet<MediaInfoEntity> MediaInfo { get; set; }
        public DbSet<UserInfoEntity> UserInfo { get; set; }

        public EfStorageDbContext()
            : this("Kanae:EntityFramework:ConnectionString")
        {
        }
        public EfStorageDbContext(String nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        [Table("UserInfo")]
        public class UserInfoEntity
        {
            /// <summary>
            /// ユーザーのID
            /// </summary>
            [Key]
            public String UserId { get; set; }
            /// <summary>
            /// 認証ハッシュ
            /// </summary>
            public String AuthHash { get; set; }
        }

        [Table("MediaInfo")]
        public class MediaInfoEntity
        {
            /// <summary>
            /// ユーザーのID
            /// </summary>
            public String UserId { get; set; }
            /// <summary>
            /// メディアのID
            /// </summary>
            [Key]
            public Guid MediaId { get; set; }
            /// <summary>
            /// アップロードした日付
            /// </summary>
            public DateTime CreatedAt { get; set; }
            /// <summary>
            /// Content-Type
            /// </summary>
            public String ContentType { get; set; }
        }
    }
}
