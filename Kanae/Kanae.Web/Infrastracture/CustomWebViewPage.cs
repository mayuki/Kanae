using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kanae.Web.Infrastracture
{
    public abstract class CustomWebViewPage<TModel> : WebViewPage<TModel>
    {
        /// <summary>
        /// ページタイトルを設定、取得します。
        /// </summary>
        public String Title
        {
            get
            {
                return ViewData["Title"] as String;
            }
            set
            {
                ViewData["Title"] = value;
            }
        }

        /// <summary>
        /// 現在のコントローラが指定された名前かどうかを返します。
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public Boolean IsCurrentController(String controller)
        {
            return String.Compare((ViewContext.RouteData.Values["controller"] as String), controller, true) == 0;
        }

        /// <summary>
        /// 現在のコントローラが指定された名前かどうかを返します。
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public Boolean IsCurrentController(String controller, String action)
        {
            return String.Compare((ViewContext.RouteData.Values["controller"] as String), controller, true) == 0 &&
                   String.Compare((ViewContext.RouteData.Values["action"] as String), action, true) == 0;
        }
    }
}