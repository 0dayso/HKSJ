using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Entity.Enums;
using HKSJ.WBVV.Repository.Interface.IBase;

namespace HKSJ.WBVV.Repository.Interface
{
    public interface IAuthKeysRepository : IBaseAccess<AuthKeys>
    {
        /// <summary>
        /// �����û��Ĺ�Կ,���ؼ���UserId�ļ��ܴ�
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="userType"></param>
        /// <returns></returns>
        string CreatePublicKey(int uid, AuthUserType userType);
    }
}