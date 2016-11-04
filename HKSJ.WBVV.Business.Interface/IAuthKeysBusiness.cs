using HKSJ.WBVV.Business.Interface.Base;
using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Entity.Enums;

namespace HKSJ.WBVV.Business.Interface
{
    public interface IAuthKeysBusiness : IBaseBusiness
    {
        /// <summary>
        /// ��ȡAPI��֤��Կ
        /// </summary>
        /// <param name="uid">�û����</param>
        /// <param name="userType">�û�����(1:����Ա  2:��ͨ�û�)</param>
        /// <returns>������Կ</returns>
        AuthKeys GetAuthKeys(int uid, AuthUserType userType);

        /// <summary>
        /// ����API��֤��Կ
        /// </summary>
        /// <param name="uid">�û����</param>
        /// <param name="userType">�û�����(1:����Ա  2:��ͨ�û�)</param>
        /// <returns>������Կ</returns>
        string CreatePublicKey(int uid, AuthUserType userType);
    }
}