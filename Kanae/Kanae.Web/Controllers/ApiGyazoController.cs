using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Kanae.Handler.Media;
using Kanae.Repository;
using Kanae.Web.Infrastracture;
using System.ComponentModel.DataAnnotations;

namespace Kanae.Web.Controllers
{
    [RoutePrefix("Api/Gyazo")]
    public class ApiGyazoController : BaseController
    {
        //
        // GET: /Api/Gyazo
        [HttpPost]
        [Route("{hash}")]
        public async Task<ActionResult> Index(String hash, HttpPostedFileBase imagedata)
        {
            if ((String.IsNullOrWhiteSpace(hash)) || (imagedata == null))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            // ハッシュからユーザーを探してくる
            var userInfoRepos = ServiceLocator.GetInstance<IUserInfoRepository>();
            var userInfo = await userInfoRepos.FindByAuthHash(hash);
            if (userInfo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var handler = Using<UploadMedia>();

            // 入力チェック
            var result = handler.Validate(userInfo.UserId, imagedata.InputStream, imagedata.ContentLength);
            if (result != ValidationResult.Success)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // アップロード実行
            var mediaInfo = await handler.Execute(userInfo.UserId, imagedata.InputStream, imagedata.ContentLength);
            if (mediaInfo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var url = Request.Url.Scheme + "://" + Request.Url.Authority + Url.Action("Index", "Media", new { Id = mediaInfo.MediaId.ToString() });
            return Content(url.ToString(), "text/plain");
        }
    }
}