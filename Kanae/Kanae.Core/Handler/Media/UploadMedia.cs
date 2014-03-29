using ImageMagick;
using Kanae.Data;
using Kanae.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Kanae.Handler.Media
{
    /// <summary>
    /// メディアをアップロードします。
    /// </summary>
    public class UploadMedia
    {
        private IMediaRepository _mediaRepository;
        private IMediaInfoRepository _uploadedInfoRepository;

        public UploadMedia(IMediaRepository mediaRepository, IMediaInfoRepository uploadedInfoRepository)
        {
            _mediaRepository = mediaRepository;
            _uploadedInfoRepository = uploadedInfoRepository;
        }

        /// <summary>
        /// アップロードされたデータの検証を行います。
        /// </summary>
        /// <param name="userId">ユーザーのID</param>
        /// <param name="stream">画像データ</param>
        /// <param name="length">画像データの長さ</param>
        /// <returns></returns>
        public ValidationResult Validate(String userId, Stream stream, Int64 length)
        {
            if (length > 1024 * 1024 * 10)
            {
                return new ValidationResult("10MBを超える画像のアップロードは許可されていません");
            }

            // 画像の種類をとるよ
            var imageInfo = new MagickImageInfo(stream);
            var contentType = (imageInfo.Format == MagickFormat.Png)
                                    ? "image/png"
                                    : (imageInfo.Format == MagickFormat.Jpeg)
                                        ? "image/jpeg"
                                        : (imageInfo.Format == MagickFormat.Gif)
                                            ? "image/gif"
                                            : null;
            stream.Position = 0; // 巻き戻しておく

            if (String.IsNullOrWhiteSpace(contentType))
            {
                return new ValidationResult("アップロードできる画像はPNG/JPEG/GIF形式のみです");
            }
            if (imageInfo.Width >= 10000 || imageInfo.Height >= 10000 || imageInfo.Width * imageInfo.Height * 4 > (45 * 1024 * 1024))
            {
                return new ValidationResult("やめてくださいしんでしまいます^o^");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// アップロードを実行します。
        /// </summary>
        /// <param name="userId">ユーザーのID</param>
        /// <param name="stream">画像データ</param>
        /// <param name="length">画像データの長さ</param>
        /// <returns></returns>
        public async Task<MediaInfo> Execute(String userId, Stream stream, Int64 length)
        {
            // チェックしますよ
            //var result = Validate(userId, stream, length);
            //if (result != ValidationResult.Success)
            //{
            //    throw new InvalidOperationException(result.ErrorMessage);
            //}

            // 読み込みますよ
            var data = new Byte[length];
            stream.Position = 0; // 一応巻き戻しますよ
            stream.Read(data, 0, data.Length);

            // 画像の種類をとるよ
            var imageInfo = new MagickImageInfo(data);
            var contentType = (imageInfo.Format == MagickFormat.Png)
                                    ? "image/png"
                                    : (imageInfo.Format == MagickFormat.Jpeg)
                                        ? "image/jpeg"
                                        : (imageInfo.Format == MagickFormat.Gif)
                                            ? "image/gif"
                                            : null;

            var mediaInfo = new MediaInfo() { UserId = userId, ContentType = contentType, MediaId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };

            // 画像アップロードに失敗したときに情報だけは残るように(画像だけ上がってしまうのは絶対に防ぐ)
            await _uploadedInfoRepository.Create(mediaInfo);
            await _mediaRepository.Create(mediaInfo, data);

            return mediaInfo;
        }
    }
}