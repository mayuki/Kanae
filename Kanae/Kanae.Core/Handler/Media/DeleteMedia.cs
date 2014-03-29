using Kanae.Data;
using Kanae.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Kanae.Handler.Media
{
    /// <summary>
    /// アップロードしたメディアを削除します。
    /// </summary>
    public class DeleteMedia
    {
        private IMediaRepository _mediaRepository;
        private IMediaInfoRepository _uploadedInfoRepository;

        public DeleteMedia(IMediaRepository mediaRepository, IMediaInfoRepository mediaInfoRepository)
        {
            _mediaRepository = mediaRepository;
            _uploadedInfoRepository = mediaInfoRepository;
        }

        public async Task<Boolean> Execute(MediaInfo mediaInfo)
        {
            // トランザクションとかないけど最悪でも画像が消えればいいのでアップロード情報を消すのは後にする
            await _mediaRepository.Delete(mediaInfo);
            await _uploadedInfoRepository.Delete(mediaInfo);

            return true;
        }
    }
}