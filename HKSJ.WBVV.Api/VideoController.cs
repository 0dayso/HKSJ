
using System;
using System.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using HKSJ.WBVV.Api.Base;
using HKSJ.WBVV.Common;
using HKSJ.WBVV.Common.Extender;
using HKSJ.WBVV.Common.Logger;
using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Entity.ApiModel;
using HKSJ.WBVV.Entity.ApiParaModel;
using HKSJ.WBVV.Entity.ViewModel;
using HKSJ.WBVV.Business.Interface;
using HKSJ.WBVV.Entity.ViewModel.Client;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using HKSJ.WBVV.Api.Filters;
using HKSJ.WBVV.Common.Extender.LinqExtender;
using HKSJ.WBVV.Common.Language;

namespace HKSJ.WBVV.Api
{
    public class VideoController : ApiControllerBase
    {
        private readonly IVideoBusiness _videoBusiness;
        private readonly IBannerVideoBusiness _bannerVideoBusiness;
        private readonly ICategoryBusiness _categoryBusiness;


        public VideoController(IVideoBusiness videoBusiness, IBannerVideoBusiness bannerVideoBusiness, ICategoryBusiness categoryBusiness)
        {
            this._videoBusiness = videoBusiness;
            this._bannerVideoBusiness = bannerVideoBusiness;
            this._categoryBusiness = categoryBusiness;
        }
        /// <summary>
        /// ��ҳ��һ������ҳ�ĵ�����
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="isIndexPage"></param>
        /// <returns></returns>
        [HttpGet]
        public RecommendAndHotCategoryVideoView GetCategoryVideoData(int categoryId, bool isIndexPage = false)
        {
            this._videoBusiness.PageSize = PageSize;
            return this._videoBusiness.GetCategoryVideoData(categoryId, isIndexPage);
        }

        /// <summary>
        /// ������Ƶ�б�
        /// </summary>
        /// <param name="categoryId">������</param>
        /// <param name="filter">��������3c12r 4c19r 5c27r</param>
        /// <param name="sortName">��������Ĭ������</param>
        /// <returns></returns>
        public PageResult GetFilterVideo(string filter, string sortName)
        {
            this._videoBusiness.PageIndex = PageIndex;
            this._videoBusiness.PageSize = PageSize;
            return this._videoBusiness.GetFilterVideo(filter, sortName);
        }

        /// <summary>
        /// һ�������µ�Banner��Ƶ
        /// </summary>
        /// <param name="categoryId">������</param>
        /// <returns></returns>
        [HttpPost]
        public IList<VideoView> GetBannerVideoList(int categoryId)
        {
            this._bannerVideoBusiness.PageSize = 100;
            return this._bannerVideoBusiness.GetBannerVideoList(categoryId, this.OrderCondtions);
        }

        /// <summary>
        /// ��ȡ����ҳ����
        /// </summary>
        [HttpPost]
        public PageResult GetVideoPageResult()
        {
            this._videoBusiness.PageIndex = this.PageIndex;
            this._videoBusiness.PageSize = this.PageSize;
            return this._videoBusiness.GetVideoPageResult(this.Condtions, this.OrderCondtions);
        }

        /// <summary>
        /// ��ȡ����ҳ����
        /// </summary>
        [HttpPost]
        public PageResult GetVideoByCategoryIdPageResult(int categoryId)
        {
            this._videoBusiness.PageIndex = this.PageIndex;
            this._videoBusiness.PageSize = this.PageSize;
            return this._videoBusiness.GetVideoByCategoryIdPageResult(categoryId, this.Condtions, this.OrderCondtions);
        }

        /// <summary>
        /// ����ҳ����Ƶ��Ϣ
        /// </summary>
        /// <param name="videoId">��Ƶ���</param>
        /// <returns></returns>
        [HttpGet]
        public VideoDetailView GetVideoDetailView(int videoId)
        {
            return _videoBusiness.GetVideoDetailView(videoId);
        }

        /// <summary>
        /// ����ҳ����Ƶ��Ϣ,����ȡ��¼���û��Ƿ��ղظ���Ƶ
        /// </summary>
        /// <param name="videoId">��Ƶ���</param>
        /// <param name="userId">��ǰ��¼�û�</param>
        /// <returns></returns>
        [HttpGet]
        public VideoDetailView GetVideoDetailView(int videoId, int userId)
        {
            return _videoBusiness.GetVideoDetailView(videoId, userId);
        }

        /// <summary>
        /// ����Id��ȡһ����Ƶ
        /// </summary>
        /// <param name="id">��Ƶ���</param>
        /// <returns></returns>
        [HttpGet]
        public Video GetAVideoById(int id)
        {
            var video = _videoBusiness.GetAVideoInfoById(id);
            return video;
        }

        /// <summary>
        /// �����Ƶ
        /// </summary>
        /// <param name="plateId"></param>
        /// <returns></returns>
        [HttpGet]
        public IList<VideoView> GetPlateVideo(int plateId)
        {
            this._videoBusiness.PageSize = PageSize;
            return _videoBusiness.GetPlateVideo(plateId);
        }
        /// <summary>
        /// �������Ƽ�
        /// </summary>
        /// <param name="plateId"></param>
        /// <returns></returns>
        [HttpGet]
        public IList<IndexVideoView> GetRecommendPlateVideo(int plateId)
        {
            this._videoBusiness.PageSize = PageSize;
            return _videoBusiness.GetRecommendPlateVideo(plateId);
        }
        /// <summary>
        /// ������ž�ѡ
        /// </summary>
        /// <param name="plateId"></param>
        /// <returns></returns>
        [HttpGet]
        public IList<IndexVideoView> GetHotPlateVideo(int plateId)
        {
            this._videoBusiness.PageSize = PageSize;
            return _videoBusiness.GetHotPlateVideo(plateId);
        }
        /// <summary>
        /// �Ƽ������Ű����Ƶ
        /// </summary>
        /// <param name="plateId"></param>
        /// <returns></returns>
        [HttpGet]
        public RecommendAndHotPlateVideoView GetRecommendAndHotPlateVideo(int plateId)
        {
            this._videoBusiness.PageSize = PageSize;
            return _videoBusiness.GetRecommendAndHotPlateVideo(plateId);
        }
        /// <summary>
        /// һ���������
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet]
        public IList<IndexVideoView> GetCategoryVideoLeft(int categoryId)
        {
            this._videoBusiness.PageSize = PageSize;
            return _videoBusiness.GetCategoryVideoLeft(categoryId);
        }
        /// <summary>
        /// һ�������ұ�
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet]
        public IList<IndexVideoView> GetCategoryVideoRight(int categoryId)
        {
            this._videoBusiness.PageSize = PageSize;
            return _videoBusiness.GetCategoryVideoRight(categoryId);
        }

        /// <summary>
        /// ������Ƶ
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet]
        public IList<VideoView> GetCategoryVideo(int categoryId)
        {
            this._videoBusiness.PageSize = PageSize;
            return _videoBusiness.GetCategoryVideo(categoryId);
        }
        /// <summary>
        /// ��������Ƽ�
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet]
        public IList<IndexVideoView> GetRecommendCategoryVideo(int categoryId)
        {
            this._videoBusiness.PageSize = PageSize;
            return _videoBusiness.GetRecommendCategoryVideo(categoryId);
        }
        /// <summary>
        /// �������ž�ѡ
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet]
        public IList<IndexVideoView> GetHotCategoryVideo(int categoryId)
        {
            this._videoBusiness.PageSize = PageSize;
            return _videoBusiness.GetHotCategoryVideo(categoryId);
        }
        /// <summary>
        /// �Ƽ������ŷ�����Ƶ
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet]
        public RecommendAndHotCategoryVideoView GetRecommendAndHotCategoryVideo(int categoryId)
        {
            this._videoBusiness.PageSize = PageSize;
            return _videoBusiness.GetRecommendAndHotCategoryVideo(categoryId);
        }

        /// <summary>
        /// ��ȡ����µ���Ƶ
        /// </summary>
        /// <param name="plateId"></param>
        /// <returns></returns>
        [HttpGet]
        public IList<VideoView> GetPlateVideoList(int plateId)
        {
            return _videoBusiness.GetPlateVideo(plateId);
        }

        #region ��Ƶ��ҳ����
        /// <summary>
        /// ������Ƶ��ҳ
        /// </summary>
        /// <param name="searchKey">�����ؼ���</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpGet]
        public dynamic GetSearchListByPage(string searchKey, int pageSize = 10, int pageIndex = 1, SortName sortName = SortName.CreateTime)
        {

            List<VideoView> videolist = new List<VideoView>();
            int _totalcount = 0;
            if (string.IsNullOrEmpty(searchKey))
            {
                return new { msg = LanguageUtil.Translate("api_Controller_Video_GetSearchListByPage_msg") };
            }
            try
            {
                videolist = _videoBusiness.SearchVideoByPage(searchKey, out _totalcount, pageSize, pageIndex, sortName);
                int _totalPage = (int)Math.Ceiling(_totalcount / (pageSize * 1.0));
                return new { msg = "success", page = new { totalNum = _totalcount, totalPage = _totalPage }, listdata = videolist };

            }
            catch (Exception ex)
            {
                return new { msg = ex.Message };
            }

        }
        #endregion

        #region ����ҳͷ���ٷ���Ƶ����
        [HttpGet]
        public dynamic SearchVideoByTop(string searchKey)
        {
            List<VideoView> videoList = new List<VideoView>();
            if (string.IsNullOrEmpty(searchKey))
            {
                return new { msg = LanguageUtil.Translate("api_Controller_Video_SearchVideoByTop_msg") };
            }
            try
            {
                videoList = _videoBusiness.SearchVideoByTop(searchKey);

                return new { msg = "success", topdata = videoList };
            }
            catch (Exception ex)
            {
                return new { msg = ex.Message };
            }

        }
        #endregion

        #region ����ҳ�����Ƶ�Ƽ�
        [HttpGet]
        public dynamic SearchVideoByRecom(string searchKey, int videoId, int recommendationNum = 6)
        {
            List<VideoView> videoList = new List<VideoView>();
            if (string.IsNullOrEmpty(searchKey))
            {
                return new { msg = LanguageUtil.Translate("api_Controller_Video_SearchVideoByRecom_msg") };
            }
            try
            {
                videoList = _videoBusiness.SearchVideoByRecom(searchKey, videoId, recommendationNum);

                return new { msg = "success", topdata = videoList };
            }
            catch (Exception ex)
            {
                return new { msg = ex.Message };
            }

        }
        #endregion


        [HttpGet]
        public string DelAndUpdateAllIndex()
        {
            _videoBusiness.DelAndUpdateAllIndex();
            return "OK";
        }


        #region ɾ��һ����Ƶ��Ϣ
        /// <summary>
        /// ɾ��һ����Ƶ��Ϣ
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpPost]
        public Result DeleteAVideo()
        {
            return CommonResult(() => this._videoBusiness.DeleteVideoInfo(JObject.Value<int>("vid")), (r) => Console.WriteLine(r.ToJSON()));
        }
        #endregion


        #region ����һ����Ƶ��Ϣ
        /// <summary>
        /// ����һ����Ƶ��Ϣ
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public Result UpdateAVideo()
        {
            var video = JObject.Value<string>("dataModel");
            var myVideoPara = video.FromJSON<MyVideoPara>();
            return CommonResult(() => this._videoBusiness.UpdateAVideo(myVideoPara), (r) => Console.WriteLine((r.ToJSON())));
        }

        #endregion

        [HttpGet]
        public Result GetVideoById(int id)
        {
            return CommonResult(() => this._videoBusiness.GetVideoById(id), (r) => Console.WriteLine((r.ToJSON())));
        }

        #region ��ȡ��Ƶ��ҳ����
        /// <summary>
        /// ��ȡ��Ƶ��ҳ����
        /// </summary>
        [HttpPost]
        public PageResult GetVideosPageResult()
        {
            this._videoBusiness.PageIndex = this.PageIndex;
            this._videoBusiness.PageSize = this.PageSize;
            return this._videoBusiness.GetVideosPageResult(this.Condtions, this.OrderCondtions);
        }
        #endregion

        [HttpGet]
        public string CheckIndexFile()
        {
            try
            {
                _videoBusiness.CheckIndexFile();
            }
            catch (Exception ex)
            {
#if !DEBUG
                     LogBuilder.Log4Net.Error(LanguageUtil.Translate("api_Controller_Video_CheckIndexFile_Error"),ex.MostInnerException());
#else
                Console.WriteLine("�������ʧ��" + ex.MostInnerException().Message);
#endif

                return ex.InnerException.Message;
            }
            return "OK";
        }

        [HttpGet]
        public string StartNewThread()
        {
            try
            {
                _videoBusiness.StartNewThread();
            }
            catch (Exception ex)
            {
                return ex.InnerException.Message;
            }
            return "OK";
        }

        [HttpGet]
        public Result GetYesRecVideos(int num)
        {
            return CommonResult(() => this._videoBusiness.GetYesRecVideos(num), (r) => Console.WriteLine((r.ToJSON())));
        }

        [HttpGet]
        public Result SearchMyVideoByPage(int userId, string mysearchKey, int pageIndex = 1, int pageSize = 5, int videoState=-1)
        {
            return CommonResult(() => this._videoBusiness.SearchMyVideoByPage(userId, mysearchKey, pageIndex, PageSize,videoState),
                (r) => Console.WriteLine(r.ToJSON()));
        }
    }
}