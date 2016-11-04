
using System;
using System.Data;
using System.Collections.Generic;
using System.Web.Http;
using HKSJ.WBVV.Api.Base;
using HKSJ.WBVV.Business.Interface;
using HKSJ.WBVV.Common.Extender;
using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Entity.ViewModel;
using HKSJ.WBVV.Entity.ViewModel.Client;
using HKSJ.WBVV.Repository.Interface;
using HKSJ.WBVV.Entity.ApiModel;
using Newtonsoft.Json.Linq;

namespace HKSJ.WBVV.Api
{
    public class UserRoomChooseController : ApiControllerBase
    {
        private readonly IUserRoomChooseBusiness _iuserRoomChooseBusiness;
        public UserRoomChooseController(IUserRoomChooseBusiness iuserRoomChooseBusiness)
        {
            this._iuserRoomChooseBusiness = iuserRoomChooseBusiness;

        }

        /// <summary> 
        /// ��ȡ�û��ռ���Ƶ����
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IList<VideoView> GetUserRoomVideoData(int userId, int dataNum = 12)
        {
            return this._iuserRoomChooseBusiness.GetUserRoomVideoData(userId, dataNum);
        }

        /// <summary> 
        /// ��ȡ�û��ռ���Ƶ���� ���з�ҳ
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public PageResult GetUserRoomVideoList()
        {
            int userId = this.JObject.Value<int>("userId");
            this._iuserRoomChooseBusiness.PageIndex = this.PageIndex;
            this._iuserRoomChooseBusiness.PageSize = this.PageSize;
            return this._iuserRoomChooseBusiness.GetUserVideoList(userId, this.Condtions, this.OrderCondtions);
        }

        /// <summary> 
        /// ��ȡ�û��ռ�ר����ͼ
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public SpecialView GetUserRoomSpecialData(int userId, int dataNum = 3, int videoNum = 0)
        {
            return this._iuserRoomChooseBusiness.GetUserRoomSpecialData(userId, dataNum, videoNum);
        }


        /// <summary>
        /// �����Ƶ���ռ�չʾ����
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public Result AddVideoToUserRoom()
        {
            return CommonResult(() => this._iuserRoomChooseBusiness.AddVideoToUserRoom(JObject.Value<int>("userId"), JObject.Value<string>("videoIds")), (r) => Console.WriteLine(r.ToJSON()));
        }

        /// <summary>
        /// ����Ƶ�ӿռ�չʾ����ɾ��
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public Result RemoveVideoToUserRoom()
        {
            return CommonResult(() => this._iuserRoomChooseBusiness.RemoveVideoToUserRoom(JObject.Value<int>("userId"), JObject.Value<int>("videoId")), (r) => Console.WriteLine(r.ToJSON()));
        }


        /// <summary>
        /// ���ר�����ռ�չʾ����
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public Result AddAlbumToUserRoom()
        {
            return CommonResult(() => this._iuserRoomChooseBusiness.AddAlbumToUserRoom(JObject.Value<int>("userId"), JObject.Value<string>("albumIds")), (r) => Console.WriteLine(r.ToJSON()));
        }

        /// <summary>
        /// ��ר���ӿռ�չʾ����ɾ��
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public Result RemoveAlbumtToUserRoom()
        {
            return CommonResult(() => this._iuserRoomChooseBusiness.RemoveAlbumtToUserRoom(JObject.Value<int>("userId"), JObject.Value<int>("albumId")), (r) => Console.WriteLine(r.ToJSON()));
        }

        /// <summary>
        /// ��ȡ�û��ϴ���Ƶ,��������ӵ��ռ���Ƶ
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="escapeStatestate"></param>
        /// <returns></returns>
        [HttpGet]
        public Result GetUserVideoViews(int userId, int pageIndex = 1, int pageSize = 5)
        {
            return CommonResult(() => this._iuserRoomChooseBusiness.GetUserVideoViews(userId, pageIndex, pageSize),
                (r) => Console.WriteLine(r.ToJSON()));
        }


        /// <summary>
        /// ��ȡ�û�ר����ͼ
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public Result GetUserAlbumsViews(int userId, int pageIndex = 1, int pageSize = 5)
        {
            return CommonResult(() => this._iuserRoomChooseBusiness.GetUserAlbumsViews(userId, pageIndex, pageSize), (r) => Console.WriteLine(r.ToJSON()));
        }




    }
}