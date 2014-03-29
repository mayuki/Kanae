using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kanae.Data
{
    /// <summary>
    /// アップロードされたメディアの情報
    /// </summary>
    public class MediaInfo
    {
        /// <summary>
        /// ユーザーのID
        /// </summary>
        public String UserId { get; set; }
        /// <summary>
        /// メディアのID
        /// </summary>
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