﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using NHibernate.Cache;

namespace NSoft.NFramework.Data.MongoDB.NHCaches {
    /// <summary>
    /// MongoDB를 캐시 저장소로 사용하는 CacheProvider입니다.
    /// </summary>
    public class MongoCacheProvider : ICacheProvider {
        #region << logger >>

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private static readonly bool IsDebugEnabled = log.IsDebugEnabled;

        #endregion

        private static readonly object _syncLock = new object();
        private static readonly IDictionary<string, ICache> _caches = new ConcurrentDictionary<string, ICache>();

        static MongoCacheProvider() {
            var configs = ConfigurationManager.GetSection(SR.NodeMongoCache) as IList<MongoCacheConfig>;

            if(configs == null)
                return;

            lock(_syncLock) {
                if(log.IsInfoEnabled)
                    log.Info("MongoCacheProvider에서 환경 설정 정보를 읽어, 각 Region 별로 MongoCacheClient를 생성합니다...");

                foreach(var config in configs)
                    _caches.Add(config.Region, new MongoCacheClient(config.Region, config.Properties));
            }
        }

        /// <summary>
        /// Configure the cache
        /// </summary>
        /// <param name="regionName">the name of the cache region</param><param name="properties">configuration settings</param>
        /// <returns/>
        public ICache BuildCache(string regionName, IDictionary<string, string> properties) {
            if(regionName == null)
                regionName = string.Empty;

            ICache result;
            if(_caches.TryGetValue(regionName, out result))
                return result;

            var props = properties ?? new Dictionary<string, string>(1);

            if(IsDebugEnabled) {
                var sb = new StringBuilder();
                sb.AppendFormat("Build MongoCache with region:[{0}], properties:", regionName);

                foreach(var pair in props)
                    sb.AppendFormat("name={0}&value={1};", pair.Key, pair.Value);

                log.Debug(sb.ToString());
            }

            return new MongoCacheClient(regionName, props);
        }

        /// <summary>
        /// generate a timestamp
        /// </summary>
        /// <returns/>
        public long NextTimestamp() {
            return Timestamper.Next();
        }

        /// <summary>
        /// Callback to perform any necessary initialization of the underlying cache implementation during ISessionFactory construction.
        /// </summary>
        /// <param name="properties">current configuration settings</param>
        public void Start(IDictionary<string, string> properties) {
            if(IsDebugEnabled)
                log.Debug("MongoCacheProvider를 시작합니다.");
        }

        /// <summary>
        /// Callback to perform any necessary cleanup of the underlying cache implementation during <see cref="M:NHibernate.ISessionFactory.Close"/>.
        /// </summary>
        public void Stop() {
            if(IsDebugEnabled)
                log.Debug("MongoCacheProvider를 종료합니다.");
        }
    }
}