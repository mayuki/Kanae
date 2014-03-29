using ImageMagick;
using Kanae.Data;
using Kanae.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanae.Handler.Draw
{
    /// <summary>
    /// 切り抜き操作のハンドラです。
    /// </summary>
    public class Crop
    {
        private IMediaRepository _mediaRepository;
        private IMediaInfoRepository _mediaInfoRepository;

        public Crop(IMediaRepository mediaRepository, IMediaInfoRepository mediaInfoInfoRepository)
        {
            _mediaRepository = mediaRepository;
            _mediaInfoRepository = mediaInfoInfoRepository;
        }

        public async Task<ValidationResult> Execute(MediaInfo mediaInfo, Int32 x, Int32 y, Int32 width, Int32 height)
        {
            if (x < 0 || y < 0 || width <= 0 || height <= 0)
            {
                return new ValidationResult("サイズの指定が不正です。");
            }

            var mediaContent = await _mediaRepository.Get(mediaInfo);
            if (mediaContent == null)
            {
                return new ValidationResult("指定されたメディアが見つかりませんでした。");
            }

            var data = await mediaContent.GetContentAsync();
            var imageInfo = new MagickImageInfo(data);
            if (imageInfo.Width < (x + width) || imageInfo.Height < (y + height))
            {
                return new ValidationResult("指定されたサイズは画像のサイズを超えています。");
            }

            // リサイズするよ!
            using (var image = new MagickImage(data))
            {
                image.Crop(new MagickGeometry(x, y, width, height));
                image.Page = new MagickGeometry(0, 0, width, height);
                
                // そして更新
                await _mediaRepository.Update(mediaInfo, image.ToByteArray());
            }

            return ValidationResult.Success;
        }
    }
}
