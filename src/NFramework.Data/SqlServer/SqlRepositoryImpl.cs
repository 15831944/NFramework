﻿using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace NSoft.NFramework.Data.SqlServer {
    /// <summary>
    /// Microsoft SQL Server 전용의 IAdoRepository입니다. 
    /// 비동기 IO 작업을 통해 확장성이 획기적으로 좋아졌습니다. 동시 처리 허용범위가 커졌습니다.
    /// NOTE: Command를 중복해서 사용하면 안됩니다. (비동기 방식이라 다른 작업에서 Command의 Connection이 닫힐 수 있습니다)
    /// NOTE: 중복 사용을 하고 싶다면, 메소드 호출 전에 Command에 Connection을 미리 지정해 주고, 모든 비동기 작업이 끝난 후에 Connection을 닫아야 합니다.
    /// </summary>
    public class SqlRepositoryImpl : AdoRepositoryImplBase, ISqlRepository {
        #region << logger >>

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private static readonly bool IsDebugEnabled = log.IsDebugEnabled;

        #endregion

        /// <summary>
        /// 생성자
        /// </summary>
        public SqlRepositoryImpl() {
            if(log.IsInfoEnabled)
                log.Info("SQL Server 전용의 SqlRepository를 생성합니다. 비동기 방식 실행이 가능합니다. dbName=[{0}]", AdoTool.DefaultDatabaseName);
        }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="dbName"></param>
        public SqlRepositoryImpl(string dbName)
            : base(dbName) {
            if(log.IsInfoEnabled)
                log.Info("SQL Server 전용의 SqlRepository를 생성합니다. 비동기 방식 실행이 가능합니다. dbName=[{0}]", dbName);
        }

        ///<summary>
        /// Microsoft Enterprise Library DAAB의 Sql Server용 Database
        ///</summary>
        public new SqlDatabase Db {
            get { return (SqlDatabase)base.Db; }
        }

        /// <summary>
        /// <paramref name="cmd"/>를 실행하여, 결과를 DataSet으로 반환합니다.
        /// </summary>
        /// <param name="cmd">실행할 <see cref="DbCommand"/> instance.</param>
        /// <param name="firstResult">첫번째 레코드의 인덱스 (0부터 시작)</param>
        /// <param name="maxResults">최대 레코드 수 (0 이면 무시하고, 마지막 레코드까지 가져온다</param>
        /// <param name="parameters">collectio of parameters of Command</param>
        /// <returns>결과 셋이 담긴 DataSet</returns>
        public override DataSet ExecuteDataSet(DbCommand cmd, int firstResult, int maxResults, params IAdoParameter[] parameters) {
            cmd.ShouldNotBeNull("cmd");

            var result = new DataSet();

            var tables = ExecuteDataTableAsList(cmd, firstResult, maxResults, parameters);
            if(tables != null)
                result.Tables.AddRange(tables.ToArray());

            return result;
        }

        /// <summary>
        /// 여러 ResultSet을 반환할 수 있으므로, DataTable의 컬렉션으로 반환합니다.
        /// </summary>
        public override IList<DataTable> ExecuteDataTableAsList(DbCommand cmd, int firstResult, int maxResults,
                                                                params IAdoParameter[] parameters) {
            cmd.ShouldBeInstanceOf<SqlCommand>("cmd");

            return With.TryFunctionAsync(() => Db.ExecuteDataTableAsListAsync((SqlCommand)cmd,
                                                                              firstResult,
                                                                              maxResults,
                                                                              parameters)
                                                   .Result,
                                         () => new List<DataTable>());
        }

        /// <summary>
        /// 지정된 Command를 실행하여, DataTable을 빌드합니다.
        /// </summary>
        /// <param name="cmd">실행할 Select용 Command</param>
        /// <param name="firstResult">첫번째 레코드의 인덱스 (0부터 시작)</param>
        /// <param name="maxResults">최대 레코드 수 (0 이면 무시하고, 마지막 레코드까지 가져온다</param>
        /// <param name="parameters">DbCommand의 Parameter 정보</param>
        /// <returns>instance of <see cref="DataTable"/></returns>
        public override DataTable ExecuteDataTable(DbCommand cmd, int firstResult, int maxResults, params IAdoParameter[] parameters) {
            cmd.ShouldBeInstanceOf<SqlCommand>("cmd");

            return With.TryFunctionAsync(() => Db.ExecuteDataTableAsync((SqlCommand)cmd,
                                                                        firstResult,
                                                                        maxResults,
                                                                        parameters)
                                                   .Result,
                                         () => (DataTable)null);
        }

        /// <summary>
        /// 지정된 Command의 ExecuteNonQuery 메소드를 실행합니다.
        /// </summary>
        /// <param name="cmd">실행할 Command 객체</param>
        /// <returns></returns>
        protected override int ExecuteNonQueryInternal(DbCommand cmd) {
            cmd.ShouldBeInstanceOf<SqlCommand>("cmd");

            return With.TryFunctionAsync(() => Db.ExecuteNonQueryAsync((SqlCommand)cmd).Result, () => -1);
        }

        /// <summary>
        /// 지정된 Command의 ExecuteScalar 메소드를 실행합니다.
        /// </summary>
        /// <param name="cmd">실행할 Command 객체</param>
        /// <returns></returns>
        protected override object ExecuteScalarInternal(DbCommand cmd) {
            cmd.ShouldBeInstanceOf<SqlCommand>("cmd");

            return With.TryFunctionAsync(() => Db.ExecuteScalarAsync((SqlCommand)cmd).Result, () => null);
        }

        /// <summary>
        /// 지정된 Command의 ExecuteReader 메소드를 실행합니다.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected override IDataReader ExecuteReaderInternal(DbCommand cmd) {
            cmd.ShouldBeInstanceOf<SqlCommand>("cmd");

            return With.TryFunctionAsync(() => Db.ExecuteReaderAsync((SqlCommand)cmd).Result, () => (IDataReader)null);
        }

        /// <summary>
        /// 지정한 Command의 ExecuteXmlReader를 비동기적으로 수행합니다.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public XmlReader ExecuteXmlReader(SqlCommand cmd, params IAdoParameter[] parameters) {
            return With.TryFunctionAsync(() => Db.ExecuteXmlReaderAsync((SqlCommand)cmd).Result, () => (XmlReader)null);
        }
    }
}