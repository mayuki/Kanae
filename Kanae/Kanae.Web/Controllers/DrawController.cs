using Kanae.Data;
using Kanae.Handler.Draw;
using Kanae.Repository;
using Kanae.Web.Infrastracture;
using Kanae.Web.Infrastracture.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Kanae.Web.Controllers
{
    [Authorize(Roles = "KanaeUsers")]
    public class DrawController : BaseController
    {
        //
        // GET: /Draw/
        public async Task<ActionResult> Index(String id)
        {
            var uploadedInfoRepos = ServiceLocator.GetInstance<IMediaInfoRepository>();

            Guid guid;
            MediaInfo mediaInfo;
            if (!Guid.TryParse(id, out guid) || (mediaInfo = await uploadedInfoRepos.FindByMediaId(guid)) == null)
            {
                return HttpNotFound();
            }

            if (mediaInfo.UserId != User.Identity.GetApplicationUserId())
            {
                return new HttpStatusCodeResult(403);
            }

            return View(mediaInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public async Task<ActionResult> IndexPost(String id, String uploadData, Boolean isOverwrite)
        {
            var uploadedInfoRepos = ServiceLocator.GetInstance<IMediaInfoRepository>();

            Guid guid;
            MediaInfo mediaInfo;
            if (!Guid.TryParse(id, out guid) || (mediaInfo = await uploadedInfoRepos.FindByMediaId(guid)) == null)
            {
                return HttpNotFound();
            }

            // 一旦Data URLを戻す
            var pos = uploadData.IndexOf(',');
            var base64Str = uploadData.Substring(pos + 1);
            var uploadDataBytes = Convert.FromBase64String(base64Str);
            var stream = new MemoryStream(uploadDataBytes);

            // ハンドラでまずはバリデーション
            var handler = Using<UploadDrawing>();
            var result = handler.Validate(mediaInfo, stream, uploadDataBytes.Length, User.Identity.GetApplicationUserId(), isOverwrite);
            if (result != ValidationResult.Success)
            {
                return Json(new {
                    Success = false,
                    ErrorMessage = result.ErrorMessage
                });
            }

            // 更新
            mediaInfo = await handler.Execute(mediaInfo, stream, uploadDataBytes.Length, User.Identity.GetApplicationUserId(), isOverwrite);

            return Json(new {
                Success = true,
                Url = Url.Action("Index", "Media", new { id = mediaInfo.MediaId.ToString() }),
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Crop(String id, Int32 x, Int32 y, Int32 width, Int32 height)
        {
            var uploadedInfoRepos = ServiceLocator.GetInstance<IMediaInfoRepository>();

            Guid guid;
            MediaInfo mediaInfo;
            if (!Guid.TryParse(id, out guid) || (mediaInfo = await uploadedInfoRepos.FindByMediaId(guid)) == null)
            {
                return HttpNotFound();
            }

            var result = await Using<Crop>().Execute(mediaInfo, x, y, width, height);

            if (result != ValidationResult.Success)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return RedirectToAction("Index", "Media", new { id = mediaInfo.MediaId.ToString() });
        }
    }
}