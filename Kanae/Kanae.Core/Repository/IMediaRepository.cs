using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Kanae.Data;

namespace Kanae.Repository
{
    public interface IMediaRepository
    {
        Task Initialize();
        Task Create(MediaInfo mediaInfo, Byte[] data);
        Task Update(MediaInfo mediaInfo, Byte[] data);
        Task<IMediaContent> Get(MediaInfo mediaInfo);
        Task Delete(MediaInfo mediaInfo);
    }
}