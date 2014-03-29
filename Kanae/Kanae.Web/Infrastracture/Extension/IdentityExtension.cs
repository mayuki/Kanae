using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace Kanae.Web.Infrastracture.Extension
{
    public static class IdentityExtension
    {
        /// <summary>
        /// ユーザーIDを取得します。
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static String GetApplicationUserId(this IIdentity identity)
        {
            var claimsIdentity = (ClaimsIdentity)identity;
            // Kanae:User:Identify-ClaimType が設定されている場合にはそれをユーザーを認識する一意の値として使う(例えばEmailにするとメールアドレスになるとか)
            // 未指定時にはNameIdentifier(統合Windows認証時にはPrimarySid)
            var identifyClaimType = ConfigurationManager.AppSettings["Kanae:User:Identify-ClaimType"]
                                    ?? (Utility.IsWindowsAuthenticationEnabled(identity) ? ClaimTypes.PrimarySid : ClaimTypes.NameIdentifier);

            return claimsIdentity.AuthenticationType + "-" + claimsIdentity.Claims.Single(x => x.Type == identifyClaimType).Value;
        }
    }
}