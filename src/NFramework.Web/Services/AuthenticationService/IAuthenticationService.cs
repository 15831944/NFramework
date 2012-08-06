using System.Security.Principal;
using NSoft.NFramework.Web.Access;

namespace NSoft.NFramework.Web.Services.AuthenticationService
{
    /// <summary>
    /// ��������
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// ������ ����
        /// </summary>
        IAccessIdentity Identity { get; }

        /// <summary>
        /// �������� Id
        /// </summary>
        string LoginUserName { get; }

        /// <summary>
        /// �α��� Url
        /// </summary>
        string LoginUrl { get; }

        /// <summary>
        /// �⺻ Url
        /// </summary>
        string DefaultUrl { get; }

        /// <summary>
        /// ����� ������ �����մϴ�.
        /// </summary>
        /// <param name="userId">�α���Id</param>
        /// <param name="password">��й�ȣ</param>
        /// <returns>��������</returns>
        bool VerifyUser(string userId, string password);

        /// <summary>
        /// �α��� ó��
        /// </summary>
        /// <param name="userName">�α���Id</param>
        /// <param name="password">��й�ȣ</param>
        /// <param name="redirectDefault">�⺻�������� �̵�</param>
        void Login(string userName = null, string password = null, bool redirectDefault = true);

        /// <summary>
        /// Identity ������ �����Ͽ� ��ȯ�մϴ�.
        /// </summary>
        /// <param name="loginId">�α���Id</param>
        /// <returns>IAccessIdentity</returns>
        IAccessIdentity CreateIdentity(string loginId);

        /// <summary>
        /// Principal ������ �����Ͽ� ��ȯ�մϴ�.
        /// </summary>
        /// <param name="identity">IIdentity</param>
        /// <returns>IPrincipal</returns>
        IPrincipal CreatePrincipal(IIdentity identity);

        /// <summary>
        /// �α׾ƿ��Ѵ�.
        /// </summary>
        /// <param name="redirectLogin">�α����������� �̵�</param>
        /// <param name="endReponse">���μ��� ����</param>
        void Logout(bool redirectLogin = true, bool endReponse = true);

        /// <summary>
        /// �α����������� �̵�
        /// </summary>
        /// <param name="endReponse">���μ��� ����</param>
        void RedirectToLoginPage(bool endReponse);
    }
}