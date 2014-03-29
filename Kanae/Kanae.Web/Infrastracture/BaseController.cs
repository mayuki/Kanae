using Kanae.Repository;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kanae.Web.Infrastracture
{
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// ServiceLocatorを取得します。
        /// </summary>
        public IServiceLocator ServiceLocator { get { return Microsoft.Practices.ServiceLocation.ServiceLocator.Current; } }

        /// <summary>
        /// ServiceLocatorを利用して指定された型のハンドラのインスタンスを取得します。
        /// ハンドラのコンストラクタのパラメータはServiceLocatorが自動的にセットします。
        /// </summary>
        /// <typeparam name="THandler"></typeparam>
        /// <returns></returns>
        [DebuggerStepThrough]
        protected THandler Using<THandler>() where THandler : class
        {
            return this.ServiceLocator.GetInstance<THandler>();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Repositoryを捨てる
                var instance = this.ServiceLocator.GetInstance<IMediaInfoRepository>() as IDisposable;
                if (instance != null)
                {
                    instance.Dispose();
                }
                instance = this.ServiceLocator.GetInstance<IUserInfoRepository>() as IDisposable;
                if (instance != null)
                {
                    instance.Dispose();
                }
                instance = this.ServiceLocator.GetInstance<IMediaRepository>() as IDisposable;
                if (instance != null)
                {
                    instance.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}