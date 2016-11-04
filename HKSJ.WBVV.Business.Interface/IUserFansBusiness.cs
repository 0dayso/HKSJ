
using System;
using System.Data;
using System.Collections.Generic;
using HKSJ.WBVV.Business.Interface.Base;
using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Entity.ViewModel;
using HKSJ.WBVV.Entity.ViewModel.Client;
using HKSJ.WBVV.Entity.ViewModel.Manage;
using HKSJ.WBVV.Common.Extender.LinqExtender;

namespace HKSJ.WBVV.Business.Interface
{

    public interface IUserFansBusiness : IBaseBusiness
    {
        //��˿�б�
        PageResult GetUserFunsList(int userId, int loginUserId);

        //�����б�
        PageResult GetUserSubscribeList(int userId, int loginUserId);
      
        /// <summary>
        /// �ҵĶ����û�
        /// </summary>
        /// <param name="loginUserId"></param>
        /// <param name="condtions"></param>
        /// <param name="orderCondtions"></param>
        /// <returns></returns>
        PageResult GetUserSubscribeUserList(int loginUserId);
        /// <summary>
        /// �ҵĶ����û����е���Ƶ
        /// </summary>
        /// <param name="loginUserId"></param>
        /// <param name="condtions"></param>
        /// <param name="orderCondtions"></param>
        /// <returns></returns>
        PageResult GetUserSubscribeVideoList(int loginUserId);

        /// <summary>
        /// �����û��Ƿ񱻶���
        /// </summary>
        /// <param name="createUserId"></param>
        /// <param name="subscribeUserId"></param>
        /// <returns></returns>
        ResultView<bool> IsSubscribe(int createUserId, int subscribeUserId);


        /// <summary>
        /// ���ĺ�ȡ������
        /// </summary>
        /// <param name="createUserId"></param>
        /// <param name="subscribeUserId"></param>
        /// <param name="careState"></param>
        /// <returns></returns>
        bool SaveSubscribe(int createUserId, int subscribeUserId, bool careState);

    }
}