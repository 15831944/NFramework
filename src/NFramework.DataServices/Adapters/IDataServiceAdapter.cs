using NSoft.NFramework.DataServices.Messages;

namespace NSoft.NFramework.DataServices.Adapters {
    /// <summary>
    /// <see cref="IDataService"/>�� ��û���� �� ���������� byte[] �Ǵ� ���ڿ��� ����� �� �ֵ��� �ϴ� Adapter �Դϴ�.
    /// </summary>
    public interface IDataServiceAdapter {
        /// <summary>
        /// ��û������ ó���Ͽ� ���������� �����ϴ� DataService 
        /// </summary>
        IDataService DataService { get; set; }

        /// <summary>
        /// ����ȭ�� ��û������ ������ȭ�� �����ϴ� <see cref="ISerializer{T}"/> �Դϴ�.
        /// </summary>
        ISerializer<RequestMessage> RequestSerializer { get; set; }

        /// <summary>
        /// ���������� ����ȭ�ϴ� <see cref="ISerializer{T}"/> �Դϴ�.
        /// </summary>
        ISerializer<ResponseMessage> ResponseSerializer { get; set; }

        /// <summary>
        /// 1. ����ȭ�� ������ <see cref="RequestSerializer"/>�� �̿��Ͽ�, ������ȭ�� ����. <see cref="RequestMessage"/>�� ����<br/>
        /// 2. ��û������  <see cref="DataService"/>�� �����Ͽ�, ���� ��, ���������� ��ȯ ����<br/>
        /// 3. ���������� <see cref="ResponseSerializer"/>�� ���� ����ȭ�Ͽ� byte[] �� ��ȯ��.
        /// </summary>
        /// <param name="requestBytes">����ȭ�� ��û Data</param>
        /// <returns>���������� ����ȭ�� byte[]</returns>
        byte[] Execute(byte[] requestBytes);

        /// <summary>
        /// �Է������� <see cref="RequestMessage"/>�� ��ȯ�Ͽ�, ��û �۾� ��, <see cref="ResponseMessage"/>�� ����ȭ�Ͽ� ��ȯ�մϴ�.
        /// </summary>
        /// <param name="requestText">��û ������</param>
        /// <returns>���������� ����ȭ�� ���ڿ�</returns>
        string Execute(string requestText);
    }
}