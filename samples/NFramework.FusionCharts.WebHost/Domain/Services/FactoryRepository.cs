using System.Collections.Generic;
using System.Data;
using NSoft.NFramework.Data;
using NSoft.NFramework.Data.Mappers;
using NSoft.NFramework.Data.SqlServer;
using NSoft.NFramework.FusionCharts.WebHost.Domain.Model;

namespace NSoft.NFramework.FusionCharts.WebHost.Domain.Services {
    public class FactoryRepository {
        #region << logger >>

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        private static readonly object _syncLock = new object();
        public static INameMapper DefaultNameMap = new CapitalizeNameMapper();

        private static IAdoRepository _ado;

        /// <summary>
        ///  �⺻ Database�� ����ϴ� Repository (ȯ�� ���� dataConfiguration�� defaultDatabase�� �ش��ϴ� DB�� ����Ѵ�.)
        /// </summary>
        public static IAdoRepository Ado {
            get {
                if(_ado == null)
                    lock(_syncLock)
                        if(_ado == null) {
                            var ado = new SqlRepositoryImpl();
                            System.Threading.Thread.MemoryBarrier();
                            _ado = ado;
                        }

                return _ado;

                // double-checking locking�� �̿��Ͽ�, �ʱ�ȭ�� �����մϴ�. (�̰� �ӵ��� �� ������, ���ϰ� ���� ����)
                // LazyInitializer.EnsureInitialized(ref _ado, () => new SqlRepositoryImpl());
                // return _ado;
            }
        }

        public static DataTable FindAllFactoryMasterDataTable() {
            const string sql = @"SELECT FactoryId, FactoryName from Factory_Master";

            return Ado.ExecuteDataTableBySqlString(sql);
        }

        public static IList<FactoryMaster> FindAllFactoryMaster() {
            const string sql = @"SELECT FactoryId, FactoryName from Factory_Master";

            IList<FactoryMaster> masters = new List<FactoryMaster>();

            using(var reader = Ado.ExecuteReaderBySqlString(sql)) {
                while(reader.Read()) {
                    masters.Add(new FactoryMaster
                                {
                                    Id = reader.AsInt32("FactoryId"),
                                    Name = reader.AsString("FactoryName")
                                });
                }
            }
            return masters;
        }

        public static IList<FactoryOutput> FindAllFactoryOutputByFactoryId(int factoryId) {
            const string sql =
                @"
SELECT FactoryId, DatePro, Quantity 
  FROM Factory_Output 
 WHERE FactoryId=@FactoryId 
 ORDER BY DatePro";

            return Ado.ExecuteInstanceAsync<FactoryOutput>(DefaultNameMap, sql, new AdoParameter("FactoryId", factoryId)).Result;
        }
    }
}