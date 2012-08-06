using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace NSoft.NFramework.Data.MongoDB {
    /// <summary>
    /// MongoDB�� ���� Data ó���� ���� Repository�� ǥ���մϴ�.
    /// </summary>
    public interface IMongoRepository : IDisposable {
        /// <summary>
        /// MongoDB ����
        /// </summary>
        MongoServer Server { get; }

        /// <summary>
        /// Database ��
        /// </summary>
        string DatabaseName { get; set; }

        /// <summary>
        /// MongoDB Database �ν��Ͻ�
        /// </summary>
        MongoDatabase Database { get; }

        /// <summary>
        /// �÷��� ��
        /// </summary>
        string CollectionName { get; set; }

        /// <summary>
        /// �÷���
        /// </summary>
        MongoCollection<BsonDocument> Collection { get; }

        /// <summary>
        /// Mongo Grid File System
        /// </summary>
        MongoGridFS GridFS { get; }

        /// <summary>
        /// ������ ���� ���ΰ�?
        /// </summary>
        bool IsServerConnected { get; }

        /// <summary>
        /// ���� ����ΰ�?
        /// </summary>
        SafeMode SafeMode { get; }

        /// <summary>
        /// <paramref name="databaseName"/>�� Database�� �����ϴ��� ����
        /// </summary>
        /// <param name="databaseName">Database ��</param>
        /// <returns></returns>
        bool DatabaseExists(string databaseName);

        /// <summary>
        /// <paramref name="databaseName"/>�� Database�� �����ϵ��� ����մϴ�.
        /// </summary>
        /// <param name="databaseName">������ Database ��</param>
        /// <returns>���� ���</returns>
        CommandResult DropDatabase(string databaseName);

        // TODO: Mongo C# Driver���� ���� �������� �ʾҽ��ϴ�.
        // void CopyDatabase(string fromDatabaseName, string toDatabaseName);

        void Reconnect();

        /// <summary>
        /// ������ �̸��� �÷����� �����մϴ�.
        /// </summary>
        /// <param name="collectionName">������ �÷��� ��</param>
        /// <returns></returns>
        CommandResult CreateCollection(string collectionName);

        /// <summary>
        /// ������ collection name�� �ش��ϴ� �÷����� DB���� �����մϴ�. (Generic�� �ƴϹǷ�, �������� ������ ���� �� �ִ� �÷����Դϴ�.)
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        CommandResult DropCollection(string collectionName);

        /// <summary>
        /// �� <see cref="Database"/>�� ��� �÷����� �����մϴ�.
        /// </summary>
        /// <returns>���� ���� ����� �÷���</returns>
        IEnumerable<CommandResult> DropAllCollection();

        /// <summary>
        /// <paramref name="collectionName"/>�� Collection�� ��ȯ�մϴ�.
        /// </summary>
        /// <param name="collectionName">�÷��� ��</param>
        /// <returns>�÷��� ���� �÷��� ��ȯ</returns>
        MongoCollection<BsonDocument> GetCollection(string collectionName);

        /// <summary>
        /// <paramref name="collectionName"/>�� Collection�� ��ȯ�մϴ�.
        /// </summary>
        /// <param name="documentType"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        MongoCollection GetCollection(Type documentType, string collectionName);

        /// <summary>
        /// <paramref name="collectionName"/>�� Collection�� ��ȯ�մϴ�.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName">�÷��� ��</param>
        /// <returns></returns>
        MongoCollection<T> GetCollectionAs<T>(string collectionName);

        /// <summary>
        /// <paramref name="srcName"/>�� �÷����� �̸��� <paramref name="destName"/>���� �����մϴ�.
        /// </summary>
        /// <param name="srcName"></param>
        /// <param name="destName"></param>
        /// <returns></returns>
        CommandResult RenameCollection(string srcName, string destName);

        /// <summary>
        /// <see cref="Database"/>�� ��� Collection�� �Ƹ��� �����մϴ�.
        /// </summary>
        string[] GetAllCollectionNames();

        /// <summary>
        /// Database�� �ִ� ��� Collection�� �����´�.
        /// </summary>
        /// <returns></returns>
        IEnumerable<MongoCollection<BsonDocument>> GetAllCollection();

        /// <summary>
        /// Database�� ��� �÷����� ������ �����ɴϴ�.
        /// </summary>
        /// <returns></returns>
        IEnumerable<MongoCollectionSettings> GetAllCollectionSettings();

        /// <summary>
        /// ������ �÷����� Document ���� ��ȯ�մϴ�.
        /// </summary>
        /// <returns></returns>
        long Count();

        /// <summary>
        /// ������ �÷����� ���ǿ� �´� Document ���� ��ȯ�մϴ�.
        /// </summary>
        /// <param name="query">���� ��ü</param>
        /// <returns></returns>
        long Count(IMongoQuery query);

        /// <summary>
        /// �÷��ǿ��� <paramref name="key"/> �� ������ �ߺ����� �ʵ��� �����ɴϴ�.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<BsonValue> Distinct(string key);

        /// <summary>
        /// �÷��ǿ��� <paramref name="key"/> �� ������ �ߺ����� �ʵ��� �����ɴϴ�.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        IEnumerable<BsonValue> Distinct(string key, IMongoQuery query);

        /// <summary>
        /// ���ǿ� �´� ù��° ��Ҹ� ��ȯ�մϴ�. 
        /// </summary>
        /// <param name="query">���� �� (������ null�� �Է��ϼ���)</param>
        /// <returns></returns>
        BsonDocument FindOne(IMongoQuery query);

        /// <summary>
        /// ���ǿ� �´� ù��° ��Ҹ� ��ȯ�մϴ�. 
        /// </summary>
        /// <param name="documentType">Document Type</param>
        /// <param name="query">���� �� (������ null�� �Է��ϼ���)</param>
        /// <returns></returns>
        object FindOneAs(Type documentType, IMongoQuery query);

        /// <summary>
        /// ���ǿ� �´� ù��° ��Ҹ� ��ȯ�մϴ�.
        /// </summary>
        /// <param name="query">���� �� (������ null�� �Է��ϼ���)</param>
        /// <returns></returns>
        T FindOneAs<T>(IMongoQuery query);

        /// <summary>
        /// Id�� Document ��ȸ�ϱ�
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        BsonDocument FindOneById(BsonValue id);

        /// <summary>
        /// Id�� Document ��ȸ�ϱ�
        /// </summary>
        /// <param name="documentType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        object FindOneByIdAs(Type documentType, BsonValue id);

        /// <summary>
        /// Id�� Document ��ȸ�ϱ�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T FindOneByIdAs<T>(BsonValue id);

        /// <summary>
        /// <paramref name="query"/> ���ǿ� �´� Document�� ��ȸ�մϴ�.
        /// </summary>
        /// <param name="query">����</param>
        /// <returns>���� ��� �÷���</returns>
        MongoCursor<BsonDocument> Find(IMongoQuery query);

        /// <summary>
        /// <paramref name="query"/> ���ǿ� �´� Document�� ��ȸ�մϴ�.
        /// </summary>
        /// <param name="documentType">Document ����</param>
        /// <param name="query">����</param>
        /// <returns>���� ��� �÷���</returns>
        MongoCursor FindAs(Type documentType, IMongoQuery query);

        /// <summary>
        /// <paramref name="query"/> ���ǿ� �´� Document�� ��ȸ�մϴ�.
        /// </summary>
        /// <typeparam name="T">����� ����</typeparam>
        /// <param name="query">����</param>
        /// <returns>���� ��� �÷���</returns>
        MongoCursor<T> FindAs<T>(IMongoQuery query);

        /// <summary>
        /// �÷����� ��� Document�� ��ȯ�մϴ�.
        /// </summary>
        MongoCursor<BsonDocument> FindAll();

        /// <summary>
        /// �÷����� ��� Document�� ��ȯ�մϴ�.
        /// </summary>
        MongoCursor FindAllAs(Type documentType);

        /// <summary>
        /// �÷����� ��� Document�� ������ �������� ��ȯ�մϴ�.
        /// </summary>
        MongoCursor<T> FindAllAs<T>();

        /// <summary>
        /// ��ȸ�� Document ������ �����մϴ�.
        /// </summary>
        /// <param name="query">���͸�</param>
        /// <param name="sortBy">���� ��Ģ</param>
        /// <param name="update">Update </param>
        /// <param name="returnNew">Update�� Document�� ��ȯ�� ���ΰ�?</param>
        /// <returns>��� ���</returns>
        FindAndModifyResult FindAndModify(IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update, bool? returnNew);

        /// <summary>
        /// ��ȸ�� Document ������ �����մϴ�.
        /// </summary>
        /// <param name="query">���͸�</param>
        /// <param name="sortBy">���� ��Ģ</param>
        /// <param name="update">Update </param>
        /// <param name="returnNew">Update�� Document�� ��ȯ�� ���ΰ�?</param>
        /// <param name="upsert">Update�� �� ������ Insert�� �� ���ΰ�?</param>
        /// <returns>��� ���</returns>
        FindAndModifyResult FindAndModify(IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update, bool? returnNew, bool? upsert);

        /// <summary>
        /// ���ǿ� �ش��ϴ� Document�� ã�Ƽ� �����մϴ�.
        /// </summary>
        /// <param name="query">����</param>
        /// <returns>���� ���</returns>
        FindAndModifyResult FindAndRemove(IMongoQuery query);

        /// <summary>
        /// ���ǿ� �ش��ϴ� Document�� ã�Ƽ� �����մϴ�.
        /// </summary>
        /// <param name="query">����</param>
        /// <param name="sortBy">���� ���</param>
        /// <returns>���� ���</returns>
        FindAndModifyResult FindAndRemove(IMongoQuery query, IMongoSortBy sortBy);

        /// <summary>
        /// Group By�� �����մϴ�. (Find() �Ŀ� LINQ �� GroupBy�� ����ϴ°� �� ���� �� �����ϴ�)
        /// </summary>
        /// <param name="query">���͸�</param>
        /// <param name="key">Grouping ���� Ű</param>
        /// <param name="initial">�ʱⰪ�� �ش��ϴ� Document</param>
        /// <param name="reduce">Grouping ���� ó���� ���� ������ ���� Javascript (��: "function(doc, prev) { prev.count += 1; }")</param>
        /// <param name="finalize">������ ���� ���� �����ؾ� �� Javascript</param>
        /// <returns>Grouping�� Document�� �÷���</returns>
        IEnumerable<BsonDocument> Group(IMongoQuery query, string key, BsonDocument initial, BsonJavaScript reduce,
                                        BsonJavaScript finalize);

        /// <summary>
        /// Group By�� �����մϴ�. (Find() �Ŀ� LINQ �� GroupBy�� ����ϴ°� �� ���� �� �����ϴ�)
        /// </summary>
        /// <param name="query">���͸�</param>
        /// <param name="groupBy">Grouping ���� Ű (��: GroupBy.Keys("x"), GroupBy.Function("function(doc) { return { x: doc.x }; }") )</param>
        /// <param name="initial">�ʱⰪ�� �ش��ϴ� Document</param>
        /// <param name="reduce">Grouping ���� ó���� ���� ������ ���� Javascript (��: "function(doc, prev) { prev.count += 1; }")</param>
        /// <param name="finalize">������ ���� ���� �����ؾ� �� Javascript</param>
        /// <returns>Grouping�� Document�� �÷���</returns>
        IEnumerable<BsonDocument> Group(IMongoQuery query, IMongoGroupBy groupBy, BsonDocument initial, BsonJavaScript reduce,
                                        BsonJavaScript finalize);

        /// <summary>
        /// Group By�� �����մϴ�. (Find() �Ŀ� LINQ �� GroupBy�� ����ϴ°� �� ���� �� �����ϴ�)
        /// </summary>
        /// <param name="query">���͸�</param>
        /// <param name="keyFunction">Grouping ���� Ű�� �����ϴ� Javascript (��: "function(doc) { return { x: doc.x }; }") </param>
        /// <param name="initial">�ʱⰪ�� �ش��ϴ� Document</param>
        /// <param name="reduce">Grouping ���� ó���� ���� ������ ���� Javascript (��: "function(doc, prev) { prev.count += 1; }")</param>
        /// <param name="finalize">������ ���� ���� �����ؾ� �� Javascript</param>
        /// <returns>Grouping�� Document�� �÷���</returns>
        IEnumerable<BsonDocument> Group(IMongoQuery query, BsonJavaScript keyFunction, BsonDocument initial, BsonJavaScript reduce,
                                        BsonJavaScript finalize);

        /// <summary>
        /// ��ƼƼ�� ���� �߰��մϴ�.
        /// </summary>
        /// <param name="document">�߰��� Document</param>
        SafeModeResult Insert(BsonDocument document);

        /// <summary>
        /// ��ƼƼ�� ���� �߰��մϴ�.
        /// </summary>
        /// <param name="documentType">�߰��� Document�� ����</param>
        /// <param name="document">�߰��� Document</param>
        SafeModeResult Insert(Type documentType, object document);

        /// <summary>
        /// ��ƼƼ�� ���� �߰��մϴ�.
        /// </summary>
        /// <typeparam name="T">�߰��� Document�� ����</typeparam>
        /// <param name="document">�߰��� Document</param>
        SafeModeResult Insert<T>(T document);

        /// <summary>
        /// ��ƼƼ���� ���� �߰��մϴ�.
        /// </summary>
        /// <param name="documentType">����� ����</param>
        /// <param name="documents">�߰��� ����� ������</param>
        IEnumerable<SafeModeResult> InsertBatch(Type documentType, IEnumerable<object> documents);

        /// <summary>
        /// ��ƼƼ���� ���� �߰��մϴ�.
        /// </summary>
        /// <param name="documents">�߰��� ����� ������</param>
        IEnumerable<SafeModeResult> InsertBatch<T>(IEnumerable<T> documents);

        /// <summary>
        /// Document �� �����մϴ�. Document�� [BsonId] �� ������ Id �Ӽ��� ������ �մϴ�.
        /// </summary>
        /// <param name="documentType"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        SafeModeResult Save(Type documentType, object document);

        /// <summary>
        /// Document �� �����մϴ�. Document�� [BsonId] �� ������ Id �Ӽ��� ������ �մϴ�.
        /// </summary>
        /// <typeparam name="T">�߰��� Document�� ����</typeparam>
        /// <param name="document">�߰��� Document</param>
        SafeModeResult Save<T>(T document);

        /// <summary>
        /// �����Ѵ�.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        SafeModeResult Remove(BsonDocument document);

        /// <summary>
        /// ���� ���ǿ� �ش��ϴ� Document�� �����մϴ�.
        /// </summary>
        /// <param name="query">������ ����� ����</param>
        SafeModeResult Remove(IMongoQuery query);

        /// <summary>
        /// <paramref name="id"/> ���� ������ Document�� �����մϴ�
        /// </summary>
        /// <param name="id">Document Id</param>
        /// <returns></returns>
        SafeModeResult RemoveById(BsonValue id);

        /// <summary>
        /// <paramref name="id"/> ���� ������ Document�� �����մϴ�
        /// </summary>
        /// <param name="documentType">Document ����</param>
        /// <param name="id">Document Id</param>
        SafeModeResult RemoveByIdAs(Type documentType, BsonValue id);

        /// <summary>
        /// <paramref name="id"/> ���� ������ Document�� �����մϴ�
        /// </summary>
        /// <typeparam name="T">Document ����</typeparam>
        /// <param name="id">Document Id</param>
        SafeModeResult RemoveByIdAs<T>(BsonValue id);

        /// <summary>
        /// �÷��ǿ��� ��� ��ƼƼ�� �����մϴ�.
        /// </summary>
        SafeModeResult RemoveAll();
    }
}