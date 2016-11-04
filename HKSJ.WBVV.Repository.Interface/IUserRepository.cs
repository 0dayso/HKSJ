using System;
using System.Data;
using System.Collections.Generic;
using HKSJ.WBVV.Repository.Interface.IBase;
using HKSJ.WBVV.Entity;

namespace HKSJ.WBVV.Repository.Interface
{
    public interface  IUserRepository:IBaseAccess<User>
    {
        void IncomeLogin(string account, string password, string ipAddress);
        void IncomeLogin(int userId, string ipAddress);
        void IncomeRegister(string account, string password, string ipAddress, int type);
        //���������������ʺŲ���¼
        void ThirdPartyBindAndLogin(string account, string password, string typeCode, string relatedId, string nickName, string figureURL, string ipAddress);
        //��������ע�����ʺŲ���
        void ThirdPartyBindAndRegister(string account, string password, string typeCode, string relatedId, string nickName, string figureURL, int type, string ipAddress);
        //���������Զ��������ʺŲ��󶨣�������
        void AutoRegisterAndBindThirdParty(string typeCode, string relatedId, string nickName, string figureURL, string ipAddress);
    }
}