using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Kanae.Data
{
    /// <summary>
    /// メディアを表すインターフェースです。
    /// </summary>
    public interface IMediaContent
    {
        Task<Byte[]> GetContentAsync();
    }

    /// <summary>
    /// 実体が外部のストレージにあり、URLでアクセスできるメディアのインターフェースです。
    /// </summary>
    public interface IRemoteMediaContent : IMediaContent
    {
        String GetUrl();
    }
}