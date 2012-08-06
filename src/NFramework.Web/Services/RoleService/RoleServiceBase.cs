using NSoft.NFramework.Web.Access;

namespace NSoft.NFramework.Web.Services.RoleService
{
    /// <summary>
    /// RoleServiceBase
    /// </summary>
    public abstract class RoleServiceBase : IRoleService
    {
        #region Implementation of IRoleService

        /// <summary>
        /// ������� ��� Role������ ��ȯ�մϴ�.
        /// </summary>
        /// <param name="identity">��û�� ����</param>
        /// <returns>Role ���</returns>
        public abstract string[] GetRoles(IAccessIdentity identity);

        /// <summary>
        /// ������� �ش� Role �Ҽӿ��θ� �Ǵ��մϴ�.
        /// </summary>
        /// <param name="roleName">Role Name</param>
        /// <param name="identity">��û�� ����</param>
        /// <returns>Role �Ҽ� ����</returns>
        public abstract bool IsInRole(string roleName, IAccessIdentity identity);

        #endregion
    }
}