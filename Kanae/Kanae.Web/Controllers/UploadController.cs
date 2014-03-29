using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Kanae.Handler.Media;
using Kanae.Repository;
using Kanae.Web.Infrastracture;
using Kanae.Web.Infrastracture.Extension;
using System.ComponentModel.DataAnnotations;
using Kanae.Web.ViewModels.Upload;

namespace Kanae.Web.Controllers
{
    [Authorize(Roles = "KanaeUsers")]
    public class UploadController : BaseController
    {
        //
        // GET: /Upload/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Upload(HttpPostedFileBase uploadedFile, Boolean withXhr = false)
        {
            if (uploadedFile == null)
            {
                return RedirectToAction("Index", "Upload");
            }

            var handler = Using<UploadMedia>();

            // 入力チェック
            var result = handler.Validate(User.Identity.GetApplicationUserId(), uploadedFile.InputStream, uploadedFile.ContentLength);
            if (result != ValidationResult.Success)
            {
                return View("Index", new UploadIndexViewModel() { ValidationResult = result });
            }

            // アップロード実行
            var mediaInfo = await handler.Execute(User.Identity.GetApplicationUserId(), uploadedFile.InputStream, uploadedFile.ContentLength);
            if (mediaInfo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (withXhr)
            {
                return Json(new
                {
                    Success = true,
                    Url = Url.Action("Index", "Media", new { Id = mediaInfo.MediaId.ToString() }),
                    EditUrl = Url.Action("Index", "Draw", new { Id = mediaInfo.MediaId.ToString() })
                });
            }
            else
            {
                return RedirectToAction("Index", "Media", new { Id = mediaInfo.MediaId.ToString() });
            }
        }
    }
}