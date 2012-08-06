using System;
using System.Security.Principal;
using System.Threading;
using System.Web;
using NSoft.NFramework.Web.Access;
using NSoft.NFramework.Web.Tools;

namespace NSoft.NFramework.Web.Services.AuthenticationService
{
    /// <summary>
    /// ���� ���񽺸� �����ϴ� Base Class�Դϴ�.
    /// </summary>
    public abstract class AuthenticationServiceBase : IAuthenticationService
    {
        #region << logger >>

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private static readonly bool IsDebugEnabled = log.IsDebugEnabled;

        #endregion

        /// <summary>
        /// �α��������� ������ ������� KeyName
        /// </summary>
        protected static readonly string ApplicationKey = AppSettings.ApplicationName;

        /// <summary>
        /// �α��������� ǥ���ϴ� ��ü
        /// </summary>
        public virtual IAccessIdentity Identity
        {
            get { return (WebAppTool.LoadValue(ApplicationKey, default(AccessIdentity))); }
        }

        /// <summary>
        /// �������� Id
        /// </summary>
        public virtual string LoginUserName
        {
            get { return AppSettings.Impersonate ? AppSettings.ImpersonateUserName : WebAppContext.Current.Identity.LoginId; }
        }

        /// <summary>
        /// �α��� Url
        /// </summary>
        public virtual string LoginUrl
        {
            get { return WebAppTool.ResolveUrl(AppSettings.LoginUrl); }
        }

        /// <summary>
        /// �⺻ Url
        /// </summary>
        public virtual string DefaultUrl
        {
            get { return WebAppTool.ResolveUrl(AppSettings.DefaultUrl); }
        }

        /// <summary>
        /// ����� ������ �����մϴ�.
        /// </summary>
        /// <param name="userId">�α���Id</param>
        /// <param name="password">��й�ȣ</param>
        /// <returns>��������</returns>
        public abstract bool VerifyUser(string userId, string password);

        /// <summary>
        /// �α���.
        /// </summary>
        /// <param name="userName">�α���Id</param>
        /// <param name="password">��й�ȣ</param>
        /// <param name="redirectDefault">�α��� �� �⺻������ �̵�����</param>
        /// <remarks>��������� ���� ��, ���������� �����մϴ�.</remarks>
        public void Login(string userName = null, string password = null, bool redirectDefault = true)
        {
            ProcessingLogin(userName, password);
            WriteLoginInfo();

            if(redirectDefault)
                RedirectToDefaultPage(true);
        }

        /// <summary>
        /// ���������� �α��� ó���Ѵ�.
        /// </summary>
        public virtual void ProcessingLogin()
        {
            ProcessingLogin(LoginUserName);
        }

        /// <summary>
        /// �α��� ó���Ѵ�.
        /// </summary>
        /// <param name="userName">�α���Id</param>
        public virtual void ProcessingLogin(string userName)
        {
            if(log.IsDebugEnabled)
                log.Debug("==>>S �α��� ó���մϴ�. enterpriseName:{0}, userName:{1}",
                          AppSettings.EnterpriseName, userName);

            var identity = AppSettings.Impersonate ? CreateIdentityAsImpersonate(userName) : CreateIdentity(userName);

            if(identity == null)
                throw new InvalidOperationException(WebAppTool.GetGlobalResourceString(AppSettings.ResourceMessages,
                                                                                      "NotExistLoginInfo", "�α��� ������ �����ϴ�."));

            SetIdentity(identity);
            SetAuthData(identity.LoginId);

            if(log.IsDebugEnabled)
                log.Debug("==>>E �α��� ó���Ͽ����ϴ�. enterpriseName:{0}, userName:{1}",
                          AppSettings.EnterpriseName, userName);
        }

        /// <summary>
        /// �α��� ó���Ѵ�.
        /// </summary>
        /// <param name="userName">�α���Id</param>
        /// <param name="password">��й�ȣ</param>
        public virtual void ProcessingLogin(string userName, string password)
        {
            if(log.IsDebugEnabled)
                log.Debug("==>>S �α��� ó���մϴ�. enterpriseName:{0}, userName:{1}, password:{2}",
                          AppSettings.EnterpriseName, userName, password);

            if(AppSettings.Impersonate == false)
            {
                bool isVerified = VerifyUser(userName, password);

                if(isVerified == false)
                    throw new InvalidOperationException(WebAppTool.GetGlobalResourceString(
                        AppSettings.ResourceMessages,
                        "LoginDifferentInfo", "�α��� ������ ��ġ���� �ʽ��ϴ�."));
            }

            var identity = AppSettings.Impersonate ? CreateIdentityAsImpersonate(userName) : CreateIdentity(userName);

            SetIdentity(identity);
            SetAuthData(identity.LoginId);

            if(log.IsDebugEnabled)
                log.Debug("==>>E �α��� ó���Ͽ����ϴ�. enterpriseName:{0}, userName:{1}, password:{2}",
                          AppSettings.EnterpriseName, userName, password);
        }

        /// <summary>
        /// �α׾ƿ��Ѵ�.
        /// </summary>
        /// <param name="redirectLogin">�α����������� �̵�</param>
        /// <param name="endReponse">���μ��� ����</param>
        public virtual void Logout(bool redirectLogin = true, bool endReponse = true)
        {
            Logout();

            if(redirectLogin)
                RedirectToLoginPage(endReponse);
        }

        /// <summary>
        /// �α��� �������� �̵�
        /// </summary>
        /// <param name="endReponse">���μ��� ����</param>
        public virtual void RedirectToLoginPage(bool endReponse)
        {
            if(log.IsDebugEnabled)
                log.Debug("==>>S �α��� �������� �̵��۾��� �����մϴ�.");

            var returnPathAndQuery = HttpContext.Current.Request.RawUrl == WebAppTool.ResolveUrl(AppSettings.LogoutUrl)
                                         ? AppSettings.DefaultUrl
                                         : HttpContext.Current.Request.Url.PathAndQuery;

            string url = WebAppTool.UrlParamConcat(LoginUrl, string.Format("ReturnUrl=[{0}]", returnPathAndQuery.UrlEncode()));

            if(log.IsDebugEnabled)
                log.Debug("{0}�� �̵��մϴ�.", url);

            HttpContext.Current.Response.Redirect(url, endReponse);

            if(log.IsDebugEnabled)
                log.Debug("==>>E �α��� �������� �̵��۾��� �Ϸ��մϴ�.");
        }

        /// <summary>
        /// �⺻ �������� �̵�
        /// </summary>
        /// <param name="endReponse">���μ��� ����</param>
        public virtual void RedirectToDefaultPage(bool endReponse)
        {
            if(log.IsDebugEnabled)
                log.Debug("�⺻ �������� �̵��۾��� �����մϴ�...");

            string currentPathAndQuery = HttpContext.Current.Request.Url.PathAndQuery;
            string url = WebAppTool.UrlParamConcat(DefaultUrl, currentPathAndQuery.UrlEncode());

            if(log.IsDebugEnabled)
                log.Debug("������ �̵�... url=[{0}]", url);

            HttpContext.Current.Response.Redirect(url, endReponse);

            if(log.IsDebugEnabled)
                log.Debug("�⺻ �������� �̵��۾��� �Ϸ��մϴ�.");
        }

        /// <summary>
        /// Identity ������ �������� �����Ͽ� ��ȯ�մϴ�.
        /// </summary>
        /// <param name="loginId"></param>
        /// <returns></returns>
        private static IAccessIdentity CreateIdentityAsImpersonate(string loginId)
        {
            return new AccessIdentity
                   {
                       CompanyCode = AppSettings.CompanyCode,
                       OrganizationCode = string.Empty,
                       UserName = AppSettings.ImpersonateUserName,
                       UserCode = loginId,
                       LoginId = loginId,
                   };
        }

        /// <summary>
        /// Identity ������ �����Ͽ� ��ȯ�մϴ�.
        /// </summary>
        /// <param name="loginId">�α���Id</param>
        /// <returns>IAccessIdentity</returns>
        public abstract IAccessIdentity CreateIdentity(string loginId);

        /// <summary>
        /// Principal ������ �����Ͽ� ��ȯ�մϴ�.
        /// </summary>
        /// <param name="identity">IIdentity</param>
        /// <returns>IPrincipal</returns>
        public virtual IPrincipal CreatePrincipal(IIdentity identity)
        {
            return new AccessPrincipal(identity);
        }

        /// <summary>
        /// �α��� ������� ������ �����Ѵ�.
        /// </summary>
        /// <param name="loginId"></param>
        protected abstract void SetAuthData(string loginId);

        /// <summary>
        /// ������ �α��� ����� ������ Clear�մϴ�.
        /// </summary>
        protected abstract void ClearAuthData();

        /// <summary>
        /// ����� ���������� Identity�� ����� ��ü ������ ä���.
        /// </summary>
        protected virtual void SetIdentity(IAccessIdentity rwIdentity)
        {
            WebAppContext.Current.SetCurrent(rwIdentity);

            WebAppTool.SetValue(ApplicationKey, rwIdentity, DateTime.MinValue, string.Empty);
        }

        /// <summary>
        /// ����� ���������� Identity�� ����� ��ü ������ �����.
        /// </summary>
        protected virtual void ClearIdentity()
        {
            if(WebTool.IsWebContext)
            {
                HttpContext.Current.User = null;
                WebAppTool.DeleteValue(ApplicationKey);
            }
            else
            {
                Thread.CurrentPrincipal = null;
                Local.Data[ApplicationKey] = null;
            }
        }

        /// <summary>
        /// �α��������� �ۼ��Ѵ�.
        /// </summary>
        protected virtual void WriteLoginInfo()
        {
            try
            {
                if(log.IsDebugEnabled)
                    log.Debug("�α����̷� ������ �ۼ��� �����մϴ�... Identity:[{0}]", Identity);

                // Write login log


                if(log.IsDebugEnabled)
                    log.Debug("�α����̷� ������ �ۼ��� �Ϸ��մϴ�. Identity:[{0}]", Identity);
            }
            catch(Exception ex)
            {
                if(log.IsWarnEnabled)
                    log.WarnException(string.Format("�α����̷� ���� �ۼ��� ������ �߻��Ͽ����ϴ�. Identity:[{0}]", Identity), ex);
            }
        }
    }
}