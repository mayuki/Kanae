using ImageMagick;
using Kanae.Data;
using Kanae.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanae.Handler.Draw
{
    /// <summary>
    /// お絵かきした画像をアップロードするハンドラです。
    /// </summary>
    public class UploadDrawing
    {
        private IMediaRepository _mediaRepository;
        private IMediaInfoRepository _mediaInfoRepository;

        public UploadDrawing(IMediaRepository mediaRepository, IMediaInfoRepository mediaInfoInfoRepository)
        {
            _mediaRepository = mediaRepository;
            _mediaInfoRepository = mediaInfoInfoRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentMediaInfo">対象となるメディアの情報</param>
        /// <param name="stream">お絵かき画像データ</param>
        /// <param name="length">お絵かき画像データの長さ</param>
        /// <param name="contentType">お絵かき画像データのコンテントタイプ</param>
        /// <param name="userId">お絵かきしたユーザーのID</param>
        /// <param name="isUpdateOriginal">オリジナルを上書きするかどうか</param>
        /// <returns></returns>
        public ValidationResult Validate(MediaInfo parentMediaInfo, Stream stream, Int32 length, String userId, Boolean isUpdateOriginal)
        {
            if (length > 1024 * 1024 * 10)
            {
                return new ValidationResult("10MBを超える画像のアップロードは許可されていません");
            }
            if (userId != parentMediaInfo.UserId && isUpdateOriginal)
            {
                return new ValidationResult("オリジナルの画像を更新できるのは自分自身がアップロードした画像のみです");
            }
            if (!isUpdateOriginal)
            {
                return new ValidationResult("画像の追加は未実装です");
            }

            // 画像の種類をとるよ
            var imageInfo = new MagickImageInfo(stream);
            var contentType = (imageInfo.Format == MagickFormat.Png)
                                    ? "image/png"
                                    : (imageInfo.Format == MagickFormat.Jpeg)
                                        ? "image/jpeg"
                                        : null;
            stream.Position = 0; // 巻き戻しておく

            if (String.IsNullOrWhiteSpace(contentType))
            {
                return new ValidationResult("アップロードできる画像はPNG/JPEG形式のみです");
            }
            if (imageInfo.Width >= 10000 || imageInfo.Height >= 10000 || imageInfo.Width*imageInfo.Height*4 >= (45*1024*1024))
            {
                return new ValidationResult("やめてくださいしんでしまいます^o^");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentMediaInfo">対象となるメディアの情報</param>
        /// <param name="stream">お絵かき画像データ</param>
        /// <param name="length">お絵かき画像データの長さ</param>
        /// <param name="userId">お絵かきしたユーザーのID</param>
        /// <param name="isUpdateOriginal">オリジナルを上書きするかどうか</param>
        /// <returns></returns>
        public async Task<MediaInfo> Execute(MediaInfo parentMediaInfo, Stream stream, Int32 length, String userId, Boolean isUpdateOriginal)
        {
            // チェックしますよ
            //var result = Validate(parentMediaInfo, stream, length, userId, isUpdateOriginal);
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
                                        : null;
            if (isUpdateOriginal)
            {
                // オリジナルの画像の更新

                // 画像とってくる
                var originalImageMediaContent = await _mediaRepository.Get(parentMediaInfo);
                var originalImageData = await originalImageMediaContent.GetContentAsync();

                // いつかMagick.NETに…。
                using (var originalImage = new Bitmap(new MemoryStream(originalImageData)))
                using (var uploadedImage = new Bitmap(stream))
                using (var newImage = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format32bppArgb))
                using (var g = Graphics.FromImage(newImage))
                {
                    // オリジナルを書いて
                    g.DrawImage(originalImage, 0, 0);
                    // うわがきしますよー
                    g.DrawImage(uploadedImage, 0, 0);

                    // バイト配列に戻しますよー
                    var memStream = new MemoryStream();
                    newImage.Save(memStream, ImageFormat.Png);
                    originalImageData = memStream.ToArray();
                }

                // 保存しますよー
                await _mediaRepository.Update(parentMediaInfo, originalImageData);

                return parentMediaInfo;
            }
            else
            {
                // 画像の追加
                throw new NotImplementedException();
            }
        }
    }
}
