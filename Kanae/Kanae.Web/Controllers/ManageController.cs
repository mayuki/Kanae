using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Kanae.Handler.Media;
using Kanae.Data;
using Kanae.Repository;
using Kanae.Web.ViewModels.Manage;
using Kanae.Web.Infrastracture;
using Kanae.Web.Infrastracture.Extension;

namespace Kanae.Web.Controllers
{
    [Authorize(Roles = "KanaeUsers")]
    public class ManageController : BaseController
    {
        //
        // GET: /Manage/
        public async Task<ActionResult> Index(DateTime? lastDateTime = null)
        {
            if (lastDateTime.HasValue)
            {
                lastDateTime = lastDateTime.Value.ToUniversalTime();
            }

            var mediaRepos = ServiceLocator.GetInstance<IMediaInfoRepository>();
            var userId = User.Identity.GetApplicationUserId();
            var items = await mediaRepos.Find(userId, take: 15, lastDateTime: lastDateTime);

            return View(new ManageIndexViewModel
            {
                Items = items,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(String id)
        {
            var uploadedInfoRepos = ServiceLocator.GetInstance<IMediaInfoRepository>();
            var userId = User.Identity.GetApplicationUserId();

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
            if (mediaInfo.UserId != userId)
            {
                return new HttpStatusCodeResult(403);
            }

            var result = await Using<DeleteMedia>().Execute(mediaInfo);
            return RedirectToAction("Index", new { Id = (string)null });
        }
    }
}