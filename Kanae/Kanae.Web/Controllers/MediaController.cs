using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Kanae.Data;
using Kanae.Repository;
using Kanae.Web.Infrastracture;
using System.Configuration;
using System.Net;

namespace Kanae.Web.Controllers
{
    [RoutePrefix("-")]
    [AllowAnonymousConfigAuthorize(Roles = "KanaeUsers")]
    public class MediaController : BaseController
    {
        public MediaController()
        {
        }

        //
        // GET: /Media/
        [Route("{id}")]
        public async Task<ActionResult> Index(String id)
        {
            // 場合によってはRedirectして画像を直接返すのでキャッシュしてほしくない
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            Response.Cache.SetValidUntilExpires(false);
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            var uploadedInfoRepos = ServiceLocator.GetInstance<IMediaInfoRepository>();
            var mediaRepos = ServiceLocator.GetInstance<IMediaRepository>();

            Guid guid;
            if (!Guid.TryParse(id, out guid))
            {
                return HttpNotFound();
            }

            var mediaInfo = await uploadedInfoRepos.FindByMediaId(guid);
            if (mediaInfo == null)
            {
                return HttpNotFound();
            }

            // text/html を Accept に含んでいたらページを返す
            // もしtext/htmlを含んでいなくて、image/で始まるのを含んでいるときは画像を求めている可能性が高いので画像を返す
            // TODO: あまり筋が良くない気がするので何とかしたい…
            if (Request.AcceptTypes.Contains("text/html"))
            {
                return View(mediaInfo);
            }
            else if (Request.AcceptTypes.Any(x => x.StartsWith("image/")))
            {
                return RedirectToAction("Show", new { id });
            }
            else
            {
                return View(mediaInfo);
            }
        }

        //
        // GET: /Media/Show/
        [Route("{id}/Show")]
        //[OutputCache(Duration = 60, VaryByParam = "id")]
        public async Task<ActionResult> Show(String id)
        {
            var uploadedInfoRepos = ServiceLocator.GetInstance<IMediaInfoRepository>();
            var mediaRepos = ServiceLocator.GetInstance<IMediaRepository>();

            Guid guid;
            if (!Guid.TryParse(id, out guid))
            {
                return HttpNotFound();
            }

            var mediaInfo = await uploadedInfoRepos.FindByMediaId(guid);
            if (mediaInfo == null)
            {
                return HttpNotFound();
            }
            var mediaContent = await mediaRepos.Get(mediaInfo);
            if (mediaContent == null)
            {
                return HttpNotFound();
            }

            return (mediaContent is IRemoteMediaContent)
                        ? (ActionResult)Redirect(((IRemoteMediaContent)mediaContent).GetUrl())
                        : (ActionResult)File(await mediaContent.GetContentAsync(), mediaInfo.ContentType);
        }
    }
}