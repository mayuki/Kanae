using Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Security.Claims;

namespace Kanae.Web.Infrastracture.Extension
{
    public static class AppBuilderExtension
    {
        /// <summary>
        /// Identityが指定された種類と値にマッチするClaimをもつ場合にはUserロールをセットします。
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IAppBuilder UseSetUserRole(this IAppBuilder app)
        {
            var userClaimType = ConfigurationManager.AppSettings["Kanae:User:ClaimType"];
            var userClaimValueMatch = ConfigurationManager.AppSettings["Kanae:User:ClaimValueMatch"];
            if (!String.IsNullOrWhiteSpace(userClaimType) && !String.IsNullOrWhiteSpace(userClaimValueMatch))
            {
                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.User.Identity != null && ctx.Request.User.Identity is ClaimsIdentity)
                    {
                        var claimsIdentity = ctx.Request.User.Identity as ClaimsIdentity;
                        if (claimsIdentity.Claims.Any(x => x.Type == userClaimType && Regex.IsMatch(x.Value, userClaimValueMatch)))
                        {
                            claimsIdentity.AddClaim(new Claim(claimsIdentity.RoleClaimType, "KanaeUsers"));
                        }
                    }

                    await next();
                });
            }
            return app;
        }
    }
}