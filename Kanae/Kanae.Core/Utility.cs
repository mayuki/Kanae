using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Kanae
{
    public static class Utility
    {
        /// <summary>
        /// 空の結果を持つTaskです。Taskが戻り値な時に使ったりします。
        /// </summary>
        public static Task EmptyTask = Task.FromResult<Object>(null);

        /// <summary>
        /// 文字列のSHA256ハッシュを返します。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static String ToSHA256Hash(this String value)
        {
            return String.Join("", SHA256.Create().ComputeHash(new UTF8Encoding(false).GetBytes(value)).Select(x => x.ToString("x2")));
        }

        /// <summary>
        /// 統合Windows認証が有効かどうかを返します。
        /// </summary>
        /// <returns></returns>
        public static Boolean IsWindowsAuthenticationEnabled(IIdentity identity)
        {
            return identity is WindowsIdentity;
        }
    }
}
