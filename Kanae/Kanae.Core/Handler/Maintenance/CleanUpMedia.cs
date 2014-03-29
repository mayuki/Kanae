using Kanae.Data;
using Kanae.Handler.Media;
using Kanae.Repository;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanae.Handler.Maintenance
{
    /// <summary>
    /// 指定された時間を過ぎたデータを削除するハンドラです。
    /// </summary>
    public class CleanUpMedia
    {
        private IServiceLocator _serviceLocator;

        public CleanUpMedia(IServiceLocator serviceLocator)
        {
            // PerThread なインスタンスを取得するためにServiceLocatorが必要
            _serviceLocator = serviceLocator;
        }

        public async Task Execute(DateTime retentionTimeLimit)
        {
            var mediaInfoRepos = _serviceLocator.GetInstance<IMediaInfoRepository>();
            IEnumerable<MediaInfo> deleteTargets;
            var count = 0;
            do
            {
                // Azure Table とかは1000件とかだったりするので適当に500件ずつとってくる
                deleteTargets = await mediaInfoRepos.Find(take: 500,
                                                          lastDateTime: retentionTimeLimit,
                                                          ignoreRetentionTime: true); // 保持期間限界よりまえ

                Parallel.ForEach(deleteTargets, new ParallelOptions { MaxDegreeOfParallelism = 5 },
                    mediaInfo =>
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine(mediaInfo.MediaId);

                            // per threadなLifeTimeでインスタンスを作らないとParallelなのでぶっ壊れて死ぬ可能性
                            var mediaReposInner = _serviceLocator.GetInstance<IMediaRepository>("PerThread");
                            var mediaInfoReposInner = _serviceLocator.GetInstance<IMediaInfoRepository>("PerThread");
                            // 削除するお
                            new DeleteMedia(mediaReposInner, mediaInfoReposInner).Execute(mediaInfo).Wait();
                        }
                        catch (Exception ex)
                        {
                            // TODO: どっかログに吐く
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                        }
                    });

                count++; // 10回ぐらい回ったらとりあえず脱出する(まあとはいえ1回で5000件とか大体のケースで消さない)
            } while (deleteTargets.Any() && count < 10);
        }
    }
}
