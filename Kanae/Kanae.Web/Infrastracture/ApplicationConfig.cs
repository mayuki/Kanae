using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Kanae.Web.Infrastracture
{
    public class ApplicationConfig
    {
        public static ApplicationConfig Current { get; private set; }

        private Lazy<TimeSpan> _retentionTimeSpan;

        static ApplicationConfig()
        {
            Current = new ApplicationConfig();
        }

        private ApplicationConfig()
        {
            _retentionTimeSpan = new Lazy<TimeSpan>(() =>
            {
                Int32 configValue;
                if (Int32.TryParse(ConfigurationManager.AppSettings["Kanae:RetentionTime"], out configValue) && configValue > 0)
                {
                    return TimeSpan.FromSeconds(configValue);
                }
                return TimeSpan.MaxValue;
            });
        }

        /// <summary>
        /// 画像の保持期間を取得します。
        /// </summary>
        public TimeSpan RetentionTimeSpan { get { return _retentionTimeSpan.Value; } }

        /// <summary>
        /// 画像の保持期間の最大の日を取得します。
        /// </summary>
        public DateTime RetentionTimeLimit
        {
            get
            {
                return (RetentionTimeSpan == TimeSpan.MaxValue)
                    ? DateTime.MinValue
                    : (DateTime.UtcNow - RetentionTimeSpan);
            }
        }
    }
}