using System.Collections;
using System.Globalization;
using System.Linq;
using NHibernate.Criterion;
using NSoft.NFramework.Data.NHibernateEx;
using NSoft.NFramework.Tools;

namespace NSoft.NFramework.StringResources.NHibernate {
    /// <summary>
    /// <see cref="NHResource"/> �� �ٷ�� Repository
    /// </summary>
    public class NHResourceRepository : NHRepository<NHResource>, INHResourceRepository {
        #region << logger >>

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private static readonly bool IsDebugEnabled = log.IsDebugEnabled;

        #endregion

        /// <summary>
        /// NHibernate Resource Repository�� ĳ�ø� ���� Ű�� ����
        /// </summary>
        public const string KeyFormat = @"{0}|{1}|{2}:{3}";

        /// <summary>
        /// ������ �κ��� ��� ���ҽ� ������ ��ȯ�մϴ�.
        /// </summary>
        public Hashtable GetResources(string application, string section, CultureInfo culture) {
            if(IsDebugEnabled)
                log.Debug(@"Get Resource String... application=[{0}], section=[{1}], culture=[{2}]", application, section, culture);


            var queryOver = BuildDetachedQueryOver(application, section, string.Empty);
            var resources = FindAll(queryOver);

            var map = new Hashtable();


            foreach(var resource in resources) {
                var nhResourceLocale = resource.GetLocaleOrDefault(culture);

                if(nhResourceLocale != null) {
                    if(IsDebugEnabled)
                        log.Debug(@"������ Culture�� �ش��ϴ� ���� �߰��մϴ�. resourceKey=[{0}], resourceValue=[{1}]",
                                  resource.ResourceKey, nhResourceLocale.ResourceValue);

                    map.Add(string.Format(KeyFormat,
                                          application,
                                          StringResourceTool.CLASS_KEY_DELIMITER,
                                          section,
                                          resource.ResourceKey),
                            nhResourceLocale.ResourceValue);
                }
            }

            return map;
        }

        /// <summary>
        /// ������ ���ҽ�Ű�� �ش��ϴ� <see cref="NHResource"/>������ �����´�.
        /// </summary>
        /// <param name="assemblyName">Assembly Name</param>
        /// <param name="sectionName">Section name</param>
        /// <param name="resourceKey">NHResource Key</param>
        /// <returns>NHResource Value�� ���� Entity</returns>
        public NHResource GetResource(string assemblyName, string sectionName, string resourceKey) {
            resourceKey.ShouldNotBeWhiteSpace("resourceKey");

            if(IsDebugEnabled)
                log.Debug(@"���ҽ� ������ DB�κ��� �ε��մϴ�. assemblyName=[{0}], section=[{1}], resourceKey=[{2}]", assemblyName,
                          sectionName, resourceKey);

            //var criteria = CreateDetachedCriteria();

            var queryOver =
                BuildDetachedQueryOver(assemblyName, sectionName, resourceKey)
                    .OrderBy(res => res.AssemblyName).Asc
                    .OrderBy(res => res.Section).Asc;

            // return FindFirst(criteria, orders.ToArray());
            var resources = FindAll(queryOver);

            if(resources.Count == 0)
                return null;

            if(resources.Count == 1)
                return resources[0];

            // 1. NULL�� �ƴ�, Ư�� Application, Ư�� Section�� ���õ� ���� ���� �����Ѵ�.
            var result = resources.FirstOrDefault(r => r.AssemblyName.IsNotWhiteSpace() && r.Section.IsNotWhiteSpace());
            if(result != null)
                return result;

            // 2. Ư�� Assembly�� ���ҽ� ���� �����Ѵ�.
            result = resources.FirstOrDefault(r => r.AssemblyName.IsNotWhiteSpace());
            if(result != null)
                return result;

            // 3. �⺻ ���ҽ��� Ư�� Section�� ���ҽ� ���� �����Ѵ�.
            result = resources.FirstOrDefault(r => r.Section.IsNotWhiteSpace());

            return result ?? null;
        }

        /// <summary>
        /// ������ ���ҽ�Ű�� �ش��ϴ� <see cref="NHResource"/>������ �����´�.
        /// </summary>
        /// <param name="section">Section name or NHResource File name</param>
        /// <param name="resourceKey">NHResource Key</param>
        /// <returns>NHResource Value�� ���� Entity</returns>
        public NHResource GetResource(string section, string resourceKey) {
            resourceKey.ShouldNotBeWhiteSpace("resourceKey");

            return GetResource(string.Empty, section, resourceKey);
        }

        /// <summary>
        /// ������ ���ҽ�Ű�� �ش��ϴ� <see cref="NHResource"/>������ �����´�.
        /// </summary>
        /// <param name="resourceKey">NHResource Key</param>
        /// <returns>NHResource Value�� ���� Entity</returns>
        public NHResource GetResource(string resourceKey) {
            resourceKey.ShouldNotBeWhiteSpace("resourceKey");

            return GetResource(string.Empty, string.Empty, resourceKey);
        }

        /// <summary>
        /// <see cref="QueryOver{T,T}"/>�� �����մϴ�.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="section"></param>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        protected QueryOver<NHResource, NHResource> BuildDetachedQueryOver(string assemblyName, string section, string resourceKey) {
            var queryOver = CreateQueryOverOf();

            if(assemblyName.IsNotWhiteSpace())
                queryOver.AddWhere(res => res.AssemblyName == assemblyName);

            if(section.IsNotWhiteSpace())
                queryOver.AddWhere(r => r.Section == section);

            if(resourceKey.IsNotWhiteSpace())
                queryOver.AddWhere(res => res.ResourceKey == resourceKey);

            return queryOver;
        }
    }
}