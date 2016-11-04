


using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using Autofac;
using HKSJ.Utilities.Base.Common;
using HKSJ.WBVV.Business.Base;
using HKSJ.WBVV.Business.Interface;
using HKSJ.WBVV.Common.Logger;
using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Common.Extender.LinqExtender;
using HKSJ.WBVV.Entity.ApiParaModel;
using HKSJ.WBVV.Entity.ViewModel.Client;
using HKSJ.WBVV.Repository.Interface;
using System.Web;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using System.Threading;
using System.IO;
using HKSJ.WBVV.Entity;
using Lucene.Net.Search;
using HKSJ.WBVV.Business.Search;
using Newtonsoft.Json.Linq;
using PanGu;
using HKSJ.WBVV.Entity.ViewModel;
using HKSJ.WBVV.Common.Extender;
using HKSJ.WBVV.Common;
using Qiniu.RPC;
using Qiniu.RS;
using Directory = System.IO.Directory;
using HKSJ.WBVV.Common.Language;

namespace HKSJ.WBVV.Business
{
    public class VideoBusiness : BaseBusiness, IVideoBusiness
    {
        private readonly IVideoRepository _videoRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IPlateRepository _plateRepository;
        private readonly IPlateVideoRepository _plateVideoRepository;
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IDictionaryItemRepository _dictionaryItemRepository;
        private readonly IUserRepository _userRepository;
        private readonly IManageRepository _manageRepository;
        private readonly IVideoPlayRecordRepository _videoPlayRecordRepository;
        private readonly IQiniuUploadBusiness _qiniuUploadBusiness;
        private ITagsBusiness _tagsBusiness;
        private readonly IUserCollectRepository _userCollectRepository;
        private IVideoApproveBusiness _videoApproveBusiness;
        private readonly IVideoApproveRepository _videoApproveRepository;
        public VideoBusiness(
                IVideoRepository videoRepository,
                IVideoPlayRecordRepository videoPlayRecordRepository,
                ICategoryRepository categoryRepository,
                IPlateRepository plateRepository,
                IPlateVideoRepository plateVideoRepository,
                IDictionaryRepository dictionaryRepository,
                IDictionaryItemRepository dictionaryItemRepository,
                IUserRepository userRepository,
                IManageRepository manageRepository,
                IQiniuUploadBusiness qiniuUploadBusiness,
            IUserCollectRepository userCollectRepository,
            IVideoApproveRepository videoApproveRepository)
        {
            this._videoRepository = videoRepository;
            this._videoPlayRecordRepository = videoPlayRecordRepository;
            this._categoryRepository = categoryRepository;
            this._plateRepository = plateRepository;
            this._plateVideoRepository = plateVideoRepository;
            this._dictionaryRepository = dictionaryRepository;
            this._dictionaryItemRepository = dictionaryItemRepository;
            this._userRepository = userRepository;
            this._manageRepository = manageRepository;
            this._qiniuUploadBusiness = qiniuUploadBusiness;
            _userCollectRepository = userCollectRepository;
            this._videoApproveRepository = videoApproveRepository;
        }

        #region ��ȡ�û�ͷ����ţURL

        public string GetUserPicUrl(int uid)
        {
            var condtion = new Condtion()
            {
                FiledName = "Id",
                FiledValue = uid,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.Equal
            };
            var user = _userRepository.GetEntity(condtion);
            if (user == null || string.IsNullOrWhiteSpace(user.Picture))
            {
                return string.Empty;
            }
            if (user.Picture.Contains("/images/"))//�����û�ѡ���ϵͳ����ͷ��
            {
                return user.Picture;
            }
            return _qiniuUploadBusiness.GetDownloadUrl(user.Picture, "image");
        }


        #endregion

        #region Ĭ�������ѯ��Ƶ
        /// <summary>
        /// ��ȡ���õ���Ƶ���ϣ���SortNum��CreateTime��������
        /// </summary>
        /// <returns></returns>
        IQueryable<Video> GetVideoListSort()
        {
            var orderCodtion = new List<OrderCondtion>()
            {
               OrderCondtionSortNum(true),
               OrderCondtionCreateTime(true)
            };
            var query = this._videoRepository.GetEntityList(CondtionEqualState(), orderCodtion);
            return query;
        }
        /// <summary>
        /// ��ȡ���ͨ������Ƶ
        /// </summary>
        /// <returns></returns>
        IQueryable<Video> GetVideoListState()
        {
            var queryable = GetVideoListSort().Query(CondtionEqualVideoState(3));
            return queryable;
        }

        #endregion

        #region manage
        /// <summary>
        /// �����б�
        /// </summary>
        /// <param name="condtions"></param>
        /// <param name="orderCondtions"></param>
        /// <param name="totalCount"></param>
        /// <param name="totalIndex"></param>
        /// <returns></returns>
        public IList<Entity.ViewModel.Manage.VideoView> GetVideoViews(IList<Condtion> condtions, IList<OrderCondtion> orderCondtions, out int totalCount, out int totalIndex)
        {
            var plate = (from v in this._videoRepository.GetEntityList(CondtionEqualState())
                         join c in this._categoryRepository.GetEntityList(CondtionEqualState()) on v.CategoryId equals c.Id
                         into vc
                         from c in vc.DefaultIfEmpty()
                         join u in this._userRepository.GetEntityList(CondtionEqualState()) on v.CreateManageId equals u.Id
                         into vj
                         from u in vj.DefaultIfEmpty()
                         join va in this._videoApproveRepository.GetEntityList() on v.Id equals va.VideoId
                         into vaa
                         from va in vaa.DefaultIfEmpty()
                         select new HKSJ.WBVV.Entity.ViewModel.Manage.VideoView()
                         {
                             Id = (int)v.Id,
                             Title = (string.IsNullOrEmpty(v.Title) ? "" : v.Title),
                             CategoryId = v.CategoryId,
                             CategoryName = c == null ? "" : c.Name,
                             VideoSource = v.VideoSource,
                             CreateManageId = v.CreateManageId,
                             CreateManageName = (u == null ? "" : (u.NickName == null ? "" : u.NickName)),
                             CreateTime = v.CreateTime,
                             VideoState = v.VideoState,
                             PersistentId = v.PersistentId,
                             Account = (u == null ? "" : u.Account),
                             ApproveContent = va == null ? "" : (va.ApproveContent == null ? "" : va.ApproveContent),
                             ApproveRemark = va == null ? "" : (va.ApproveRemark == null ? "" : va.ApproveRemark),
                             ParentId = c == null ? 0 : (c.ParentId == null ? 0 : c.ParentId),
                             SmallPicturePath = v.SmallPicturePath
                         }).AsQueryable();

            if (condtions != null && condtions.Count > 0)//��ѯ����
            {
                plate = plate.Query(condtions);
            }
            bool isExists = plate.Any();
            if (isExists)
            {
                if (orderCondtions != null && orderCondtions.Count > 0)//��������
                {
                    plate = plate.OrderBy(orderCondtions);
                }
            }
            totalCount = isExists ? plate.Count() : 0;
            if (this.PageSize <= 0)
            {
                totalIndex = 0;
                var queryable = isExists
                    ? plate.ToList()
                    : new List<Entity.ViewModel.Manage.VideoView>();
                return queryable;
            }
            else
            {
                totalIndex = totalCount % this.PageSize == 0
                    ? (totalCount / this.PageSize)
                    : (totalCount / this.PageSize + 1);
                if (this.PageIndex <= 0)
                {
                    this.PageIndex = 1;
                }
                if (this.PageIndex >= totalIndex)
                {
                    this.PageIndex = totalIndex;
                }

                var queryable = isExists
                    ? plate.Skip((this.PageIndex - 1) * this.PageSize).Take(this.PageSize).ToList()
                    : new List<Entity.ViewModel.Manage.VideoView>();

                return queryable;
            }
        }

        /// <summary>
        /// ��ҳ�б�
        /// </summary>
        /// <param name="condtions"></param>
        /// <param name="orderCondtions"></param>
        /// <returns></returns>
        public PageResult GetVideosPageResult(IList<Condtion> condtions, IList<OrderCondtion> orderCondtions)
        {
            int totalCount = 0;
            int totalIndex = 0;
            IList<HKSJ.WBVV.Entity.ViewModel.Manage.VideoView> plateViews = GetVideoViews(condtions, orderCondtions, out totalCount, out totalIndex);
            return new PageResult()
            {
                PageSize = this.PageSize,
                PageIndex = this.PageIndex,
                TotalCount = totalCount,
                TotalIndex = totalIndex,
                Data = plateViews
            };
        }
        #endregion

        #region client

        #region ��ֹ�������
        /// <summary>
        /// ��ȡ���������ļ���
        /// </summary>
        /// <param name="filter">2c3r2c4r</param>
        /// <returns></returns>
        private IDictionary<string, string> GetDictionaryViewList(string filter)
        {
            IDictionary<string, string> dictionaryViews = new Dictionary<string, string>();
            IDictionary<int, int> dictionarys = GetDictionarys('r', 'c', filter);
            if (dictionarys.Count > 0)
            {
                if (dictionarys.Count > 0)
                {
                    foreach (var dictionary in dictionarys)
                    {
                        var dict = this._dictionaryRepository.GetEntity(ConditionEqualId(dictionary.Key));
                        var dictItem = this._dictionaryItemRepository.GetEntity(ConditionEqualId(dictionary.Value));
                        if (dict != null)
                        {
                            if (dictionaryViews.ContainsKey(dict.Name))
                            {
                                dictionaryViews[dict.Name] = dictItem == null ? "" : dictItem.Name;
                            }
                            else
                            {
                                dictionaryViews.Add(dict.Name, dictItem == null ? "" : dictItem.Name);
                            }
                        }
                    }
                }
            }
            return dictionaryViews;
        }
        #endregion

        #region ����ҳ������ȡ��Ƶ��ϸ��Ϣ
        /// <summary>
        /// ��ȡ��Ƶ��ϸ��Ϣ
        /// </summary>
        /// <param name="videoId">��Ƶ���</param>
        /// <returns></returns>
        public VideoDetailView GetVideoDetailView(int videoId, int userId)
        {
            var videoDetailView = new VideoDetailView();
            if (videoId <= 0)
            {
                return videoDetailView;
            }
            var vedio = (from video in this._videoRepository.GetEntityList()
                         where video.State == false //����
                                && video.VideoState == 3 //���ͨ��
                                && video.Id == videoId //��Ƶ���
                         select video).FirstOrDefault();
            if (vedio == null)
            {
                return videoDetailView;
            }
            //�û��ϴ�
            if (vedio.VideoSource)
            {
                videoDetailView = (from video in this._videoRepository.GetEntityList()
                                   join cate in this._categoryRepository.GetEntityList() on video.CategoryId equals cate.Id
                                   join user in this._userRepository.GetEntityList() on video.CreateManageId equals user.Id
                                   where video.State == false //����
                                       && video.VideoState == 3 //���ͨ��
                                       && video.Id == vedio.Id //��Ƶ���
                                       && cate.State == false //����
                                       && user.State == false //����
                                   select new VideoDetailView()
                                   {
                                       CategoryId = cate.Id.ToString(),
                                       CategoryName = cate.Name,
                                       DictionaryViews = GetDictionaryViewList(video.Filter),
                                       Tags = video.Tags,
                                       About = video.About,
                                       Starring = video.Starring,
                                       Title = video.Title,
                                       VideoPath = video.VideoPath,
                                       BigPicturePath = video.BigPicturePath,
                                       SmallPicturePath = video.SmallPicturePath,
                                       CollectionCount = video.CollectionCount,
                                       CommentCount = video.CommentCount,
                                       PraiseCount = video.PraiseCount,
                                       BadCount = video.BadCount,
                                       PlayCount = video.PlayCount,
                                       Director = video.Director,
                                       Id = video.Id,
                                       UserId = user.Id,
                                       NickName = user.NickName,
                                       Picture = user.Picture,
                                       VideoSource = 1,
                                       RewardCount = video.RewardCount
                                   }
                        ).FirstOrDefault();
            }
            else//����Ա�ϴ�
            {
                videoDetailView = (from video in this._videoRepository.GetEntityList()
                                   join cate in this._categoryRepository.GetEntityList() on video.CategoryId equals cate.Id
                                   join manage in this._manageRepository.GetEntityList() on video.CreateManageId equals manage.Id
                                   where video.State == false //����
                                       && video.VideoState == 3 //���ͨ��
                                       && video.Id == vedio.Id //��Ƶ���
                                       && cate.State == false //����
                                       && manage.State == false //����
                                   select new VideoDetailView()
                                   {
                                       CategoryId = cate.Id.ToString(),
                                       CategoryName = cate.Name,
                                       DictionaryViews = GetDictionaryViewList(video.Filter),
                                       Tags = video.Tags,
                                       About = video.About,
                                       Starring = video.Starring,
                                       Title = video.Title,
                                       VideoPath = video.VideoPath,
                                       BigPicturePath = video.BigPicturePath,
                                       SmallPicturePath = video.SmallPicturePath,
                                       CollectionCount = video.CollectionCount,
                                       CommentCount = video.CommentCount,
                                       PraiseCount = video.PraiseCount,
                                       BadCount = video.BadCount,
                                       PlayCount = video.PlayCount,
                                       Director = video.Director,
                                       Id = video.Id,
                                       UserId = manage.Id,
                                       NickName = manage.NickName,
                                       Picture = "",
                                       VideoSource = 0
                                   }
                      ).FirstOrDefault();
            }

            //����е�¼�û������ȡ�û��Ƿ��ղظ���Ƶ
            if (userId > 0 && videoDetailView != null)
            {
                var controller = (from c in this._userCollectRepository.GetEntityList(CondtionEqualState())
                                  where c.CreateUserId == userId
                                  && c.VideoId == videoDetailView.Id
                                  select c).AsQueryable<UserCollect>();

                if (controller.Any())
                    videoDetailView.IsCollected = true;
                else
                    videoDetailView.IsCollected = false;
            }

            return videoDetailView;
        }

        #endregion

        #region һ������ҳ����������Ƶ

        #region ��ȡ������Ƶ��������ʱ�併�򣬰�������������
        /// <summary>
        /// ��ȡ������Ƶ
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public IList<VideoView> GetCategoryVideo(int categoryId)
        {
            var categoryVideos = new List<VideoView>();
            if (categoryId <= 0)
            {
                return categoryVideos;
            }
            var category = this._categoryRepository.GetEntity(ConditionEqualId(categoryId));
            if (category == null)
            {
                return categoryVideos;
            }

            #region û�н��õĹ���Ա�ϴ��ķ�����Ƶ
            var manageVideos = (from c in this._categoryRepository.GetEntityList()
                                join v in this._videoRepository.GetEntityList() on c.Id equals v.CategoryId
                                join m in this._manageRepository.GetEntityList() on v.CreateManageId equals m.Id
                                where c.State == false //����
                                    && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                    && v.State == false //����
                                    && v.VideoState == 3 //���ͨ��
                                    && m.State == false //����
                                select new VideoView()
                                {
                                    DictionaryId = GetDictionarys('r', 'c', v.Filter),
                                    DictionaryViews = GetDictionaryViewList(v.Filter),
                                    CategoryName = c.Name,//���ͷ�������
                                    IsHot = v.IsHot,
                                    IsRecommend = v.IsRecommend,
                                    UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime,
                                    PlayCount = v.PlayCount,
                                    About = v.About,
                                    BigPicturePath = v.BigPicturePath,
                                    CreateTime = v.CreateTime,
                                    Director = v.Director,
                                    Id = v.Id,
                                    IsOfficial = v.IsOfficial,
                                    SmallPicturePath = v.SmallPicturePath,
                                    Starring = v.Starring,
                                    Tags = v.Tags,
                                    SortNum = v.SortNum,
                                    VideoState = v.VideoState,
                                    Title = v.Title,
                                    Filter = v.Filter
                                }
                         ).AsQueryable();
            #endregion

            #region û�н��õ��û��ϴ��ķ�����Ƶ
            var userVideos = (from c in this._categoryRepository.GetEntityList()
                              join v in this._videoRepository.GetEntityList() on c.Id equals v.CategoryId
                              join u in this._userRepository.GetEntityList() on v.CreateManageId equals u.Id
                              where c.State == false //����
                                  && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                  && v.State == false //����
                                  && v.VideoState == 3 //���ͨ��
                                  && u.State == false //����
                              select new VideoView()
                              {
                                  DictionaryId = GetDictionarys('r', 'c', v.Filter),
                                  DictionaryViews = GetDictionaryViewList(v.Filter),
                                  CategoryName = c.Name,//���ͷ�������
                                  IsHot = v.IsHot,
                                  IsRecommend = v.IsRecommend,
                                  UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime,
                                  PlayCount = v.PlayCount,
                                  About = v.About,
                                  BigPicturePath = v.BigPicturePath,
                                  CreateTime = v.CreateTime,
                                  Director = v.Director,
                                  Id = v.Id,
                                  IsOfficial = v.IsOfficial,
                                  SmallPicturePath = v.SmallPicturePath,
                                  Starring = v.Starring,
                                  Tags = v.Tags,
                                  SortNum = v.SortNum,
                                  VideoState = v.VideoState,
                                  Title = v.Title,
                                  Filter = v.Filter
                              }
                         ).AsQueryable();
            #endregion

            #region �ϲ��û���Ƶ
            if (manageVideos.Any())
            {
                categoryVideos.AddRange(manageVideos);
            }
            if (userVideos.Any())
            {
                categoryVideos.AddRange(userVideos);
            }
            #endregion

            #region ����
            var queryable = (from v in categoryVideos
                             orderby v.SortNum descending, //��������������
                                 v.PlayCount descending, //�����Ž���
                                 v.UpdateTime descending
                             //���ϼ�ʱ�併��
                             select v).AsQueryable();
            #endregion

            int pagesize = 10;
            if (category.PageSize <= 0)
            {
                if (this.PageSize > 0)
                {
                    pagesize = this.PageSize;
                }
            }
            else
            {
                pagesize = category.PageSize;
            }
            var pageQueryable = queryable.Take(pagesize);
            if (pageQueryable.Any())
            {
                categoryVideos = pageQueryable.ToList();
            }

            return categoryVideos;
        }
        #endregion

        #region ������Ƶ�ұߣ���������������
        /// <summary>
        /// ������Ƶ�ұ�
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public IList<IndexVideoView> GetCategoryVideoRight(int categoryId)
        {
            var categoryVideos = new List<IndexVideoView>();
            if (categoryId <= 0)
            {
                return categoryVideos;
            }
            var category = this._categoryRepository.GetEntity(ConditionEqualId(categoryId));
            if (category == null)
            {
                return categoryVideos;
            }
            #region û�н��õĹ���Ա�ϴ��ķ�����Ƶ
            var manageVideos = (from c in this._categoryRepository.GetEntityList()
                                join v in this._videoRepository.GetEntityList() on c.Id equals v.CategoryId
                                join m in this._manageRepository.GetEntityList() on v.CreateManageId equals m.Id
                                where c.State == false //����
                                    && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                    && v.State == false //����
                                    && v.VideoState == 3 //���ͨ��
                                    && m.State == false //����
                                    && v.VideoSource == false //����Ա
                                select new IndexVideoView()
                                {
                                    Id = v.Id,
                                    Title = v.Title,
                                    About = v.About,
                                    SortNum = v.SortNum,
                                    SmallPicturePath = v.SmallPicturePath,
                                    BigPicturePath = v.BigPicturePath,
                                    PlayCount = v.PlayCount,
                                    CreateTime = v.CreateTime,
                                    UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime
                                }
                         ).AsQueryable();
            #endregion

            #region û�н��õ��û��ϴ��ķ�����Ƶ
            var userVideos = (from c in this._categoryRepository.GetEntityList()
                              join v in this._videoRepository.GetEntityList() on c.Id equals v.CategoryId
                              join u in this._userRepository.GetEntityList() on v.CreateManageId equals u.Id
                              where c.State == false //����
                                  && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                  && v.State == false //����
                                  && v.VideoState == 3 //���ͨ��
                                  && u.State == false //����
                                  && v.VideoSource == true //�û�
                              select new IndexVideoView()
                              {
                                  Id = v.Id,
                                  Title = v.Title,
                                  About = v.About,
                                  SmallPicturePath = v.SmallPicturePath,
                                  BigPicturePath = v.BigPicturePath,
                                  PlayCount = v.PlayCount,
                                  SortNum = v.SortNum,
                                  UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime,
                                  CreateTime = v.CreateTime
                              }
                         ).AsQueryable();
            #endregion

            #region �ϲ��û���Ƶ
            if (manageVideos.Any())
            {
                categoryVideos.AddRange(manageVideos);
            }
            if (userVideos.Any())
            {
                categoryVideos.AddRange(userVideos);
            }
            #endregion

            #region ����
            var queryable = (from v in categoryVideos
                             orderby v.PlayCount descending, //��������������
                                 v.SortNum descending, //��������������
                                 v.UpdateTime descending
                             //���ϼ�ʱ�併��
                             select v).AsQueryable();
            #endregion

            int pagesize = 10;
            if (category.PageSize <= 0)
            {
                if (this.PageSize > 0)
                {
                    pagesize = this.PageSize;
                }
            }
            else
            {
                pagesize = category.PageSize;
            }
            var pageQueryable = queryable.Take(pagesize);
            if (pageQueryable.Any())
            {
                categoryVideos = pageQueryable.ToList();
            }

            return categoryVideos;
        }

        #endregion

        #region ������Ƶ��ߣ�������ʱ�併��
        /// <summary>
        /// ������Ƶ���,��������������
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public IList<IndexVideoView> GetCategoryVideoLeft(int categoryId)
        {
            var categoryVideos = new List<IndexVideoView>();
            if (categoryId <= 0)
            {
                return categoryVideos;
            }
            var category = this._categoryRepository.GetEntity(ConditionEqualId(categoryId));
            if (category == null)
            {
                return categoryVideos;
            }
            #region û�н��õĹ���Ա�ϴ��ķ�����Ƶ
            var manageVideos = (from c in this._categoryRepository.GetEntityList()
                                join v in this._videoRepository.GetEntityList() on c.Id equals v.CategoryId
                                join m in this._manageRepository.GetEntityList() on v.CreateManageId equals m.Id
                                where c.State == false //����
                                    && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                    && v.State == false //����
                                    && v.VideoState == 3 //���ͨ��
                                    && m.State == false //����
                                    && v.VideoSource == false //����Ա
                                select new IndexVideoView()
                                {
                                    Id = v.Id,
                                    Title = v.Title,
                                    About = v.About,
                                    PlayCount = v.PlayCount,
                                    SortNum = v.SortNum,
                                    SmallPicturePath = v.SmallPicturePath,
                                    BigPicturePath = v.BigPicturePath,
                                    CreateTime = v.CreateTime,
                                    UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime
                                }
                         ).AsQueryable();
            #endregion

            #region û�н��õ��û��ϴ��ķ�����Ƶ
            var userVideos = (from c in this._categoryRepository.GetEntityList()
                              join v in this._videoRepository.GetEntityList() on c.Id equals v.CategoryId
                              join u in this._userRepository.GetEntityList() on v.CreateManageId equals u.Id
                              where c.State == false //����
                                  && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                  && v.State == false //����
                                  && v.VideoState == 3 //���ͨ��
                                  && u.State == false //����
                                  && v.VideoSource == true //�û�
                              select new IndexVideoView()
                              {
                                  Id = v.Id,
                                  Title = v.Title,
                                  About = v.About,
                                  SmallPicturePath = v.SmallPicturePath,
                                  BigPicturePath = v.BigPicturePath,
                                  CreateTime = v.CreateTime,
                                  UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime,
                                  SortNum = v.SortNum,
                                  PlayCount = v.PlayCount
                              }
                         ).AsQueryable();
            #endregion

            #region �ϲ��û���Ƶ
            if (manageVideos.Any())
            {
                categoryVideos.AddRange(manageVideos);
            }
            if (userVideos.Any())
            {
                categoryVideos.AddRange(userVideos);
            }
            #endregion

            #region ����
            var queryable = (from v in categoryVideos
                             orderby v.SortNum descending, //������ʱ�併��
                                     v.UpdateTime descending //��������������
                             select v).AsQueryable();
            #endregion

            int pagesize = 10;
            if (category.PageSize <= 0)
            {
                if (this.PageSize > 0)
                {
                    pagesize = this.PageSize;
                }
            }
            else
            {
                pagesize = category.PageSize;
            }
            var pageQueryable = queryable.Take(pagesize);
            if (pageQueryable.Any())
            {
                categoryVideos = pageQueryable.ToList();
            }

            return categoryVideos;
        }
        #endregion

        /// <summary>
        /// ��ҳ��һ������ҳ������
        /// </summary>
        /// <param name="categoryId">������</param>
        /// <param name="isIndexPage">�Ƿ�Ϊ��ҳ���� true:��ҳ����</param>
        /// <returns></returns>
        public RecommendAndHotCategoryVideoView GetCategoryVideoData(int categoryId, bool isIndexPage = false)
        {
            var result = new RecommendAndHotCategoryVideoView();

            var categoryVideos = new List<IndexVideoView>();
            if (categoryId <= 0)
                return result;

            var category = this._categoryRepository.GetEntity(ConditionEqualId(categoryId));
            if (category == null)
                return result;
            result.Category = new CategorysView()
            {
                ParentCategory = new CategoryView()
                {
                    Id = category.Id,
                    Name = category.Name,
                    ParentId = category.ParentId
                }
            };

            if (isIndexPage)
            {
                #region ��ҳ����

                #region û�н��õĹ���Ա�ϴ��ķ�����Ƶ
                var manageRecommendVideos = (from pv in this._plateVideoRepository.GetEntityList()
                                             join c in this._categoryRepository.GetEntityList() on pv.CategoryId equals c.Id
                                             join v in this._videoRepository.GetEntityList() on pv.VideoId equals v.Id
                                             join m in this._manageRepository.GetEntityList() on v.CreateManageId equals m.Id
                                             where pv.State == false //����
                                                 && pv.IsRecommend == true //�Ƽ�
                                                 && c.State == false //����
                                                 && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                                 && v.State == false //����
                                                 && v.VideoState == 3 //���ͨ��
                                                 && m.State == false//����
                                                 && v.VideoSource == false //����Ա
                                             select new IndexVideoView()
                                             {
                                                 Id = v.Id,
                                                 Title = v.Title,
                                                 About = v.About,
                                                 PlayCount = v.PlayCount,
                                                 SortNum = pv.SortNum,
                                                 CreateTime = v.CreateTime,
                                                 UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime,
                                                 BigPicturePath = v.BigPicturePath,
                                                 SmallPicturePath = v.SmallPicturePath
                                             }
                             ).AsQueryable();

                var manageHotVideos = (from pv in this._plateVideoRepository.GetEntityList()
                                       join c in this._categoryRepository.GetEntityList() on pv.CategoryId equals c.Id
                                       join v in this._videoRepository.GetEntityList() on pv.VideoId equals v.Id
                                       join m in this._manageRepository.GetEntityList() on v.CreateManageId equals m.Id
                                       where pv.State == false //����
                                           && pv.IsHot == true //����
                                           && c.State == false //����
                                           && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                           && v.State == false //����
                                           && v.VideoState == 3 //���ͨ��
                                           && m.State == false//����
                                           && v.VideoSource == false //����Ա
                                       select new IndexVideoView()
                                       {
                                           Id = v.Id,
                                           Title = v.Title,
                                           About = v.About,
                                           PlayCount = v.PlayCount,
                                           SortNum = pv.SortNum,
                                           CreateTime = v.CreateTime,
                                           UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime,
                                           BigPicturePath = v.BigPicturePath,
                                           SmallPicturePath = v.SmallPicturePath
                                       }
                           ).AsQueryable();
                #endregion

                #region û�н��õ��û��ϴ��ķ�����Ƶ
                var userRecommendVideos = (from pv in this._plateVideoRepository.GetEntityList()
                                           join c in this._categoryRepository.GetEntityList() on pv.CategoryId equals c.Id
                                           join v in this._videoRepository.GetEntityList() on pv.VideoId equals v.Id
                                           join u in this._userRepository.GetEntityList() on v.CreateManageId equals u.Id
                                           where pv.State == false //����
                                              && pv.IsRecommend == true //�Ƽ�
                                               && c.State == false //����
                                               && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                               && v.State == false //����
                                               && v.VideoState == 3 //���ͨ��
                                               && u.State == false//����
                                               && v.VideoSource == true //�û�
                                           select new IndexVideoView()
                                           {
                                               Id = v.Id,
                                               Title = v.Title,
                                               About = v.About,
                                               SortNum = pv.SortNum,
                                               PlayCount = v.PlayCount,
                                               SmallPicturePath = v.SmallPicturePath,
                                               BigPicturePath = v.BigPicturePath,
                                               CreateTime = v.CreateTime,
                                               UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime
                                           }
                             ).AsQueryable();

                var userHotVideos = (from pv in this._plateVideoRepository.GetEntityList()
                                     join c in this._categoryRepository.GetEntityList() on pv.CategoryId equals c.Id
                                     join v in this._videoRepository.GetEntityList() on pv.VideoId equals v.Id
                                     join u in this._userRepository.GetEntityList() on v.CreateManageId equals u.Id
                                     where pv.State == false //����
                                         && pv.IsHot == true //����
                                         && c.State == false //����
                                         && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                         && v.State == false //����
                                         && v.VideoState == 3 //���ͨ��
                                         && u.State == false//����
                                         && v.VideoSource == true //�û�
                                     select new IndexVideoView()
                                     {
                                         Id = v.Id,
                                         Title = v.Title,
                                         About = v.About,
                                         SortNum = pv.SortNum,
                                         PlayCount = v.PlayCount,
                                         SmallPicturePath = v.SmallPicturePath,
                                         BigPicturePath = v.BigPicturePath,
                                         CreateTime = v.CreateTime,
                                         UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime
                                     }
                            ).AsQueryable();
                #endregion

                #region �ϲ�������
                //�Ƽ��ϲ�
                if (manageRecommendVideos.Any())
                    categoryVideos.AddRange(manageRecommendVideos);
                if (userRecommendVideos.Any())
                    categoryVideos.AddRange(userRecommendVideos);

                //�Ƽ�
                var queryableLeft = (from v in categoryVideos
                                     orderby v.SortNum descending, //��������������
                                         v.UpdateTime descending
                                     //���ϼ�ʱ�併��
                                     select v).AsQueryable();

                categoryVideos = new List<IndexVideoView>();

                //���źϲ�
                if (manageHotVideos.Any())
                    categoryVideos.AddRange(manageHotVideos);
                if (userHotVideos.Any())
                    categoryVideos.AddRange(userHotVideos);


                //����
                var queryableRight = (from v in categoryVideos
                                      orderby v.SortNum descending, //��������������
                                          v.UpdateTime descending
                                      //���ϼ�ʱ�併��
                                      select v).AsQueryable();
                #endregion

                #region ��ҳ
                int pagesize = 10;
                if (category.PageSize <= 0)
                {
                    if (this.PageSize > 0)
                    {
                        pagesize = this.PageSize;
                    }
                }
                else
                {
                    pagesize = category.PageSize;
                }
                var pageQueryable = queryableLeft.Take(pagesize);
                if (pageQueryable.Any())
                {
                    result.RecommendCategoryVideo = pageQueryable.ToList();
                }
                pageQueryable = queryableRight.Take(pagesize);
                if (pageQueryable.Any())
                {
                    result.HotCategoryVideo = pageQueryable.ToList();
                }
                #endregion
                #endregion
            }
            else
            {
                #region һ����������
                #region û�н��õĹ���Ա�ϴ��ķ�����Ƶ
                var manageVideos = (from c in this._categoryRepository.GetEntityList()
                                    join v in this._videoRepository.GetEntityList() on c.Id equals v.CategoryId
                                    join m in this._manageRepository.GetEntityList() on v.CreateManageId equals m.Id
                                    where c.State == false //����
                                        && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                        && v.State == false //����
                                        && v.VideoState == 3 //���ͨ��
                                        && m.State == false //����
                                        && v.VideoSource == false //����Ա
                                    select new IndexVideoView()
                                    {
                                        Id = v.Id,
                                        Title = v.Title,
                                        About = v.About,
                                        SortNum = v.SortNum,
                                        SmallPicturePath = v.SmallPicturePath,
                                        BigPicturePath = v.BigPicturePath,
                                        PlayCount = v.PlayCount,
                                        CreateTime = v.CreateTime,
                                        UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime
                                    }
                             ).AsQueryable();
                #endregion

                #region û�н��õ��û��ϴ��ķ�����Ƶ
                var userVideos = (from c in this._categoryRepository.GetEntityList()
                                  join v in this._videoRepository.GetEntityList() on c.Id equals v.CategoryId
                                  join u in this._userRepository.GetEntityList() on v.CreateManageId equals u.Id
                                  where c.State == false //����
                                      && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                      && v.State == false //����
                                      && v.VideoState == 3 //���ͨ��
                                      && u.State == false //����
                                      && v.VideoSource == true //�û�
                                  select new IndexVideoView()
                                  {
                                      Id = v.Id,
                                      Title = v.Title,
                                      About = v.About,
                                      SmallPicturePath = v.SmallPicturePath,
                                      BigPicturePath = v.BigPicturePath,
                                      PlayCount = v.PlayCount,
                                      SortNum = v.SortNum,
                                      UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime,
                                      CreateTime = v.CreateTime
                                  }
                             ).AsQueryable();
                #endregion

                #region �ϲ��û���Ƶ
                if (manageVideos.Any())
                {
                    categoryVideos.AddRange(manageVideos);
                }
                if (userVideos.Any())
                {
                    categoryVideos.AddRange(userVideos);
                }
                #endregion

                #region ����
                //�Ƽ���Ƶ
                var queryableLeft = (from v in categoryVideos
                                     orderby v.SortNum descending, //������ʱ�併��
                                             v.UpdateTime descending //��������������
                                     select v).AsQueryable();
                //���ž�ѡ��Ƶ
                var queryableRight = (from v in categoryVideos
                                      orderby v.PlayCount descending, //��������������
                                          v.SortNum descending, //��������������
                                          v.UpdateTime descending
                                      //���ϼ�ʱ�併��
                                      select v).AsQueryable();
                #endregion

                #region ��ҳ
                int pagesize = 10;
                if (category.PageSize <= 0)
                {
                    if (this.PageSize > 0)
                    {
                        pagesize = this.PageSize;
                    }
                }
                else
                {
                    pagesize = category.PageSize;
                }
                var pageQueryable = queryableRight.Take(pagesize);
                if (pageQueryable.Any())
                {
                    result.HotCategoryVideo = pageQueryable.ToList();
                }
                pageQueryable = queryableLeft.Take(pagesize);
                if (pageQueryable.Any())
                {
                    result.RecommendCategoryVideo = pageQueryable.ToList();
                }

                #endregion

                #endregion
            }

            return result;
        }

        #endregion

        #region ��ҳ��һ������ҳ���������Ƶ

        #region ��ȡ�����Ƶ
        /// <summary>
        /// ��ȡ�����Ƶ
        /// </summary>
        /// <param name="plateId"></param>
        /// <returns></returns>
        public IList<VideoView> GetPlateVideo(int plateId)
        {
            var plateVideos = new List<VideoView>();
            if (plateId <= 0)
            {
                return plateVideos;
            }
            var plate = this._plateRepository.GetEntity(ConditionEqualId(plateId));
            if (plate == null)
            {
                return plateVideos;
            }
            #region û�н��õĹ���Ա�ϴ��ķ�����Ƶ
            var manageVideos = (from pv in this._plateVideoRepository.GetEntityList()
                                join p in this._plateRepository.GetEntityList() on pv.PlateId equals p.Id
                                join v in this._videoRepository.GetEntityList() on pv.VideoId equals v.Id
                                join m in this._manageRepository.GetEntityList() on v.CreateManageId equals m.Id
                                join c in this._categoryRepository.GetEntityList(CondtionEqualState()) on v.CategoryId equals c.Id
                                into cJoin//����������� 0 ��ʾ��ҳ���
                                from cateModel in cJoin.DefaultIfEmpty()
                                where pv.State == false //����
                                    && p.State == false //����
                                    && p.Id == plate.Id //�����
                                    && v.State == false //����
                                    && v.VideoState == 3 //���ͨ��
                                    && m.State == false
                                    && v.VideoSource == false //����Ա
                                select new VideoView()
                                {
                                    DictionaryId = GetDictionarys('r', 'c', v.Filter),
                                    DictionaryViews = GetDictionaryViewList(v.Filter),
                                    CategoryName = cateModel == null ? LanguageUtil.Translate("api_Business_Video_GetPlateVideo_CategoryName_homePage_manageVideos") : cateModel.Name,//���ͷ�������
                                    IsHot = v.IsHot,
                                    IsRecommend = v.IsRecommend,
                                    UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime,
                                    PlayCount = v.PlayCount,
                                    About = v.About,
                                    BigPicturePath = v.BigPicturePath,
                                    CreateTime = v.CreateTime,
                                    Director = v.Director,
                                    Id = v.Id,
                                    IsOfficial = v.IsOfficial,
                                    SmallPicturePath = v.SmallPicturePath,
                                    Starring = v.Starring,
                                    Tags = v.Tags,
                                    SortNum = pv.SortNum,
                                    VideoState = v.VideoState,
                                    Title = v.Title,
                                    Filter = v.Filter
                                }
                         ).AsQueryable();
            #endregion

            #region û�н��õ��û��ϴ��ķ�����Ƶ
            var userVideos = (from pv in this._plateVideoRepository.GetEntityList()
                              join p in this._plateRepository.GetEntityList() on pv.PlateId equals p.Id
                              join v in this._videoRepository.GetEntityList() on pv.VideoId equals v.Id
                              join u in this._userRepository.GetEntityList() on v.CreateManageId equals u.Id
                              join c in this._categoryRepository.GetEntityList(CondtionEqualState()) on v.CategoryId equals c.Id
                              into cJoin//����������� 0 ��ʾ��ҳ���
                              from cateModel in cJoin.DefaultIfEmpty()
                              where pv.State == false //����
                                  && p.State == false //����
                                  && p.Id == plate.Id //�����
                                  && v.State == false //����
                                  && v.VideoState == 3 //���ͨ��
                                  && u.State == false
                                  && v.VideoSource == true //�û�
                              select new VideoView()
                              {
                                  DictionaryId = GetDictionarys('r', 'c', v.Filter),
                                  DictionaryViews = GetDictionaryViewList(v.Filter),
                                  CategoryName = cateModel == null ? LanguageUtil.Translate("api_Business_Video_GetPlateVideo_CategoryName_homePage_userVideos") : cateModel.Name,//���ͷ�������
                                  IsHot = v.IsHot,
                                  IsRecommend = v.IsRecommend,
                                  UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime,
                                  PlayCount = v.PlayCount,
                                  About = v.About,
                                  BigPicturePath = v.BigPicturePath,
                                  CreateTime = v.CreateTime,
                                  Director = v.Director,
                                  Id = v.Id,
                                  IsOfficial = v.IsOfficial,
                                  SmallPicturePath = v.SmallPicturePath,
                                  Starring = v.Starring,
                                  Tags = v.Tags,
                                  SortNum = pv.SortNum,
                                  VideoState = v.VideoState,
                                  Title = v.Title,
                                  Filter = v.Filter
                              }
                         ).AsQueryable();
            #endregion

            #region �ϲ��û���Ƶ
            if (manageVideos.Any())
            {
                plateVideos.AddRange(manageVideos);
            }
            if (userVideos.Any())
            {
                plateVideos.AddRange(userVideos);
            }
            #endregion

            #region ����
            var queryable = (from v in plateVideos
                             orderby v.SortNum descending, //��������������
                                     v.PlayCount descending,//�����Ŵ�������
                                     v.UpdateTime descending//���ϼ�ʱ�併��
                             select v).AsQueryable();
            #endregion

            int pagesize = 10;
            if (plate.PageSize <= 0)
            {
                if (this.PageSize > 0)
                {
                    pagesize = this.PageSize;
                }
            }
            else
            {
                pagesize = plate.PageSize;
            }
            var pageQueryable = queryable.Take(pagesize);
            if (pageQueryable.Any())
            {
                plateVideos = pageQueryable.ToList();
            }
            return plateVideos;
        }
        #endregion

        #region ��ȡ���Ű����Ƶ
        /// <summary>
        /// ��ȡ���Ű����Ƶ
        /// </summary>
        /// <param name="plateId"></param>
        /// <returns></returns>
        public IList<IndexVideoView> GetHotPlateVideo(int plateId)
        {
            var hotPlateVideos = new List<IndexVideoView>();
            if (plateId <= 0)
            {
                return hotPlateVideos;
            }
            var plate = this._plateRepository.GetEntity(ConditionEqualId(plateId));
            if (plate == null)
            {
                return hotPlateVideos;
            }
            #region û�н��õĹ���Ա�ϴ��İ����Ƶ
            var manageVideos = (from pv in this._plateVideoRepository.GetEntityList()
                                join p in this._plateRepository.GetEntityList() on pv.PlateId equals p.Id
                                join v in this._videoRepository.GetEntityList() on pv.VideoId equals v.Id
                                join m in this._manageRepository.GetEntityList() on v.CreateManageId equals m.Id
                                join c in this._categoryRepository.GetEntityList(CondtionEqualState()) on v.CategoryId equals c.Id
                                into cJoin//����������� 0 ��ʾ��ҳ���
                                from cateModel in cJoin.DefaultIfEmpty()
                                where pv.State == false //����
                                    && pv.IsHot == true //����
                                    && p.State == false //����
                                    && p.Id == plate.Id //�����
                                    && v.State == false //����
                                    && v.VideoState == 3 //���ͨ��
                                    && m.State == false
                                    && v.VideoSource == false //����Ա
                                select new IndexVideoView()
                                {
                                    Id = v.Id,
                                    Title = v.Title,
                                    About = v.About,
                                    SmallPicturePath = v.SmallPicturePath,
                                    BigPicturePath = v.BigPicturePath,
                                    PlayCount = v.PlayCount,
                                    SortNum = pv.SortNum,
                                    UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime,
                                    CreateTime = v.CreateTime
                                }
                         ).AsQueryable();
            #endregion

            #region û�н��õ��û��ϴ��ķ�����Ƶ
            var userVideos = (from pv in this._plateVideoRepository.GetEntityList()
                              join p in this._plateRepository.GetEntityList() on pv.PlateId equals p.Id
                              join v in this._videoRepository.GetEntityList() on pv.VideoId equals v.Id
                              join u in this._userRepository.GetEntityList() on v.CreateManageId equals u.Id
                              join c in this._categoryRepository.GetEntityList(CondtionEqualState()) on v.CategoryId equals c.Id
                              into cJoin//����������� 0 ��ʾ��ҳ���
                              from cateModel in cJoin.DefaultIfEmpty()
                              where pv.State == false //����
                                  && pv.IsHot == true //����
                                  && p.State == false //����
                                  && p.Id == plate.Id //�����
                                  && v.State == false //����
                                  && v.VideoState == 3 //���ͨ��
                                  && u.State == false
                                  && v.VideoSource == true //�û�
                              select new IndexVideoView()
                              {
                                  Id = v.Id,
                                  Title = v.Title,
                                  About = v.About,
                                  SmallPicturePath = v.SmallPicturePath,
                                  BigPicturePath = v.BigPicturePath,
                                  SortNum = pv.SortNum,
                                  PlayCount = v.PlayCount,
                                  UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime,
                                  CreateTime = v.CreateTime
                              }
                         ).AsQueryable();
            #endregion

            #region �ϲ��û���Ƶ
            if (manageVideos.Any())
            {
                hotPlateVideos.AddRange(manageVideos);
            }
            if (userVideos.Any())
            {
                hotPlateVideos.AddRange(userVideos);
            }
            #endregion

            #region ����
            var queryable = (from v in hotPlateVideos
                             orderby v.SortNum descending, //��������������
                                     v.PlayCount descending//�����Ŵ�������
                             select v).AsQueryable();
            #endregion

            int pagesize = 10;
            if (plate.PageSize <= 0)
            {
                if (this.PageSize > 0)
                {
                    pagesize = this.PageSize;
                }
            }
            else
            {
                pagesize = plate.PageSize;
            }
            var pageQueryable = queryable.Take(pagesize);
            if (pageQueryable.Any())
            {
                hotPlateVideos = pageQueryable.ToList();
            }
            return hotPlateVideos;
        }
        #endregion

        #region ��ȡ�Ƽ������Ƶ
        /// <summary>
        /// ��ȡ�Ƽ������Ƶ
        /// </summary>
        /// <param name="plateId"></param>
        /// <returns></returns>
        public IList<IndexVideoView> GetRecommendPlateVideo(int plateId)
        {
            var recommendPlateVideos = new List<IndexVideoView>();
            if (plateId <= 0)
            {
                return recommendPlateVideos;
            }
            var plate = this._plateRepository.GetEntity(ConditionEqualId(plateId));
            if (plate == null)
            {
                return recommendPlateVideos;
            }
            #region û�н��õĹ���Ա�ϴ��ķ�����Ƶ
            var manageVideos = (from pv in this._plateVideoRepository.GetEntityList()
                                join p in this._plateRepository.GetEntityList() on pv.PlateId equals p.Id
                                join v in this._videoRepository.GetEntityList() on pv.VideoId equals v.Id
                                join m in this._manageRepository.GetEntityList() on v.CreateManageId equals m.Id
                                join c in this._categoryRepository.GetEntityList(CondtionEqualState()) on v.CategoryId equals c.Id
                                into cJoin//����������� 0 ��ʾ��ҳ���
                                from cateModel in cJoin.DefaultIfEmpty()
                                where pv.State == false //����
                                    && pv.IsRecommend == true //�Ƽ�
                                    && p.State == false //����
                                    && p.Id == plate.Id //�����
                                    && v.State == false //����
                                    && v.VideoState == 3 //���ͨ��
                                    && m.State == false
                                    && v.VideoSource == false //����Ա
                                select new IndexVideoView()
                                {
                                    Id = v.Id,
                                    PlayCount = v.PlayCount,
                                    Title = v.Title,
                                    About = v.About,
                                    SmallPicturePath = v.SmallPicturePath,
                                    BigPicturePath = v.BigPicturePath,
                                    UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime,
                                    CreateTime = v.CreateTime,
                                    SortNum = pv.SortNum
                                }
                         ).AsQueryable();
            #endregion

            #region û�н��õ��û��ϴ��ķ�����Ƶ
            var userVideos = (from pv in this._plateVideoRepository.GetEntityList()
                              join p in this._plateRepository.GetEntityList() on pv.PlateId equals p.Id
                              join v in this._videoRepository.GetEntityList() on pv.VideoId equals v.Id
                              join u in this._userRepository.GetEntityList() on v.CreateManageId equals u.Id
                              join c in this._categoryRepository.GetEntityList(CondtionEqualState()) on v.CategoryId equals c.Id
                              into cJoin//����������� 0 ��ʾ��ҳ���
                              from cateModel in cJoin.DefaultIfEmpty()
                              where pv.State == false //����
                                  && pv.IsRecommend == true //�Ƽ�
                                  && p.State == false //����
                                  && p.Id == plate.Id //�����
                                  && v.State == false //����
                                  && v.VideoState == 3 //���ͨ��
                                  && u.State == false
                                  && v.VideoSource == true //�û�
                              select new IndexVideoView()
                              {
                                  Id = v.Id,
                                  Title = v.Title,
                                  PlayCount = v.PlayCount,
                                  About = v.About,
                                  BigPicturePath = v.BigPicturePath,
                                  SmallPicturePath = v.SmallPicturePath,
                                  UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime,
                                  CreateTime = v.CreateTime,
                                  SortNum = pv.SortNum
                              }
                         ).AsQueryable();
            #endregion

            #region �ϲ��û���Ƶ
            if (manageVideos.Any())
            {
                recommendPlateVideos.AddRange(manageVideos);
            }
            if (userVideos.Any())
            {
                recommendPlateVideos.AddRange(userVideos);
            }
            #endregion

            #region ����
            var queryable = (from v in recommendPlateVideos
                             orderby v.SortNum descending, //��������������
                                     v.UpdateTime descending//������ʱ�併��
                             select v).AsQueryable();
            #endregion

            int pagesize = 10;
            if (plate.PageSize <= 0)
            {
                if (this.PageSize > 0)
                {
                    pagesize = this.PageSize;
                }
            }
            else
            {
                pagesize = plate.PageSize;
            }
            var pageQueryable = queryable.Take(pagesize);
            if (pageQueryable.Any())
            {
                recommendPlateVideos = pageQueryable.ToList();
            }
            return recommendPlateVideos;
        }
        #endregion

        #region �Ƽ������Ű����Ƶ
        /// <summary>
        /// �Ƽ������Ű����Ƶ
        /// </summary>
        /// <param name="plateId"></param>
        /// <returns></returns>
        public RecommendAndHotPlateVideoView GetRecommendAndHotPlateVideo(int plateId)
        {
            var plateVideoView = new RecommendAndHotPlateVideoView();
            if (plateId <= 0)
            {
                return plateVideoView;
            }
            var plate = this._plateRepository.GetEntity(ConditionEqualId(plateId));
            if (plate == null)
            {
                return plateVideoView;
            }
            plateVideoView.HotPlateVideo = GetHotPlateVideo(plateId);
            plateVideoView.RecommendPlateVideo = GetRecommendPlateVideo(plateId);
            return plateVideoView;
        }
        #endregion

        #endregion

        #region ��ҳ����������Ƶ

        #region ��ȡ���ŷ�����Ƶ
        /// <summary>
        /// ��ȡ���ŷ�����Ƶ
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public IList<IndexVideoView> GetHotCategoryVideo(int categoryId)
        {
            var hotCategoryVideos = new List<IndexVideoView>();
            if (categoryId <= 0)
            {
                return hotCategoryVideos;
            }
            var category = this._categoryRepository.GetEntity(ConditionEqualId(categoryId));
            if (category == null)
            {
                return hotCategoryVideos;
            }
            #region û�н��õĹ���Ա�ϴ��ķ�����Ƶ
            var manageVideos = (from pv in this._plateVideoRepository.GetEntityList()
                                join c in this._categoryRepository.GetEntityList() on pv.CategoryId equals c.Id
                                join v in this._videoRepository.GetEntityList() on pv.VideoId equals v.Id
                                join m in this._manageRepository.GetEntityList() on v.CreateManageId equals m.Id
                                where pv.State == false //����
                                    && pv.IsHot == true //����
                                    && c.State == false //����
                                    && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                    && v.State == false //����
                                    && v.VideoState == 3 //���ͨ��
                                    && m.State == false//����
                                    && v.VideoSource == false //����Ա
                                select new IndexVideoView()
                                {
                                    Id = v.Id,
                                    Title = v.Title,
                                    About = v.About,
                                    PlayCount = v.PlayCount,
                                    SortNum = pv.SortNum,
                                    SmallPicturePath = v.SmallPicturePath,
                                    BigPicturePath = v.BigPicturePath,
                                    CreateTime = v.CreateTime,
                                    UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime
                                }
                         ).AsQueryable();
            #endregion

            #region û�н��õ��û��ϴ��ķ�����Ƶ
            var userVideos = (from pv in this._plateVideoRepository.GetEntityList()
                              join c in this._categoryRepository.GetEntityList() on pv.CategoryId equals c.Id
                              join v in this._videoRepository.GetEntityList() on pv.VideoId equals v.Id
                              join u in this._userRepository.GetEntityList() on v.CreateManageId equals u.Id
                              where pv.State == false //����
                                  && pv.IsHot == true //����
                                  && c.State == false //����
                                  && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                  && v.State == false //����
                                  && v.VideoState == 3 //���ͨ��
                                  && u.State == false//����
                                  && v.VideoSource == true //�û�
                              select new IndexVideoView()
                              {
                                  Id = v.Id,
                                  Title = v.Title,
                                  About = v.About,
                                  SortNum = pv.SortNum,
                                  PlayCount = v.PlayCount,
                                  SmallPicturePath = v.SmallPicturePath,
                                  BigPicturePath = v.BigPicturePath,
                                  CreateTime = v.CreateTime,
                                  UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime
                              }
                         ).AsQueryable();
            #endregion

            #region �ϲ��û���Ƶ
            if (manageVideos.Any())
            {
                hotCategoryVideos.AddRange(manageVideos);
            }
            if (userVideos.Any())
            {
                hotCategoryVideos.AddRange(userVideos);
            }
            #endregion

            #region ����
            var queryable = (from v in hotCategoryVideos
                             orderby v.SortNum descending, //��������������
                                 v.UpdateTime descending
                             //���ϼ�ʱ�併��
                             select v).AsQueryable();
            #endregion

            int pagesize = 10;
            if (category.PageSize <= 0)
            {
                if (this.PageSize > 0)
                {
                    pagesize = this.PageSize;
                }
            }
            else
            {
                pagesize = category.PageSize;
            }
            var pageQueryable = queryable.Take(pagesize);
            if (pageQueryable.Any())
            {
                hotCategoryVideos = pageQueryable.ToList();
            }

            return hotCategoryVideos;
        }
        #endregion

        #region ��ȡ�Ƽ�������Ƶ
        /// <summary>
        /// ��ȡ�Ƽ�������Ƶ
        /// </summary>
        /// <returns></returns>
        public IList<IndexVideoView> GetRecommendCategoryVideo(int categoryId)
        {
            var recommendCategoryVideos = new List<IndexVideoView>();
            if (categoryId <= 0)
            {
                return recommendCategoryVideos;
            }
            var category = this._categoryRepository.GetEntity(ConditionEqualId(categoryId));
            if (category == null)
            {
                return recommendCategoryVideos;
            }
            #region û�н��õĹ���Ա�ϴ��ķ�����Ƶ
            var manageVideos = (from pv in this._plateVideoRepository.GetEntityList()
                                join c in this._categoryRepository.GetEntityList() on pv.CategoryId equals c.Id
                                join v in this._videoRepository.GetEntityList() on pv.VideoId equals v.Id
                                join m in this._manageRepository.GetEntityList() on v.CreateManageId equals m.Id
                                where pv.State == false //����
                                    && pv.IsRecommend == true //�Ƽ�
                                    && c.State == false //����
                                    && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                    && v.State == false //����
                                    && v.VideoState == 3 //���ͨ��
                                    && m.State == false//����
                                    && v.VideoSource == false //����Ա
                                select new IndexVideoView()
                                {
                                    Id = v.Id,
                                    Title = v.Title,
                                    About = v.About,
                                    PlayCount = v.PlayCount,
                                    SortNum = pv.SortNum,
                                    CreateTime = v.CreateTime,
                                    UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime,
                                    BigPicturePath = v.BigPicturePath,
                                    SmallPicturePath = v.SmallPicturePath
                                }
                         ).AsQueryable();
            #endregion

            #region û�н��õ��û��ϴ��ķ�����Ƶ
            var userVideos = (from pv in this._plateVideoRepository.GetEntityList()
                              join c in this._categoryRepository.GetEntityList() on pv.CategoryId equals c.Id
                              join v in this._videoRepository.GetEntityList() on pv.VideoId equals v.Id
                              join u in this._userRepository.GetEntityList() on v.CreateManageId equals u.Id
                              where pv.State == false //����
                                 && pv.IsRecommend == true //�Ƽ�
                                  && c.State == false //����
                                  && c.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                                  && v.State == false //����
                                  && v.VideoState == 3 //���ͨ��
                                  && u.State == false//����
                                  && v.VideoSource == true //�û�
                              select new IndexVideoView()
                              {
                                  Id = v.Id,
                                  Title = v.Title,
                                  About = v.About,
                                  SortNum = pv.SortNum,
                                  PlayCount = v.PlayCount,
                                  SmallPicturePath = v.SmallPicturePath,
                                  BigPicturePath = v.BigPicturePath,
                                  CreateTime = v.CreateTime,
                                  UpdateTime = v.UpdateTime.HasValue ? Convert.ToDateTime(v.UpdateTime) : v.CreateTime
                              }
                         ).AsQueryable();
            #endregion

            #region �ϲ��û���Ƶ
            if (manageVideos.Any())
            {
                recommendCategoryVideos.AddRange(manageVideos);
            }
            if (userVideos.Any())
            {
                recommendCategoryVideos.AddRange(userVideos);
            }
            #endregion

            #region ����
            var queryable = (from v in recommendCategoryVideos
                             orderby v.SortNum descending, //��������������
                                 v.UpdateTime descending
                             //���ϼ�ʱ�併��
                             select v).AsQueryable();
            #endregion

            int pagesize = 10;
            if (category.PageSize <= 0)
            {
                if (this.PageSize > 0)
                {
                    pagesize = this.PageSize;
                }
            }
            else
            {
                pagesize = category.PageSize;
            }
            var pageQueryable = queryable.Take(pagesize);
            if (pageQueryable.Any())
            {
                recommendCategoryVideos = pageQueryable.ToList();
            }

            return recommendCategoryVideos;
        }
        #endregion

        #region �Ƽ������ŷ�����Ƶ
        /// <summary>
        /// �Ƽ������ŷ�����Ƶ
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public RecommendAndHotCategoryVideoView GetRecommendAndHotCategoryVideo(int categoryId)
        {
            var categoryVideoView = new RecommendAndHotCategoryVideoView();
            var hotCategoryVideos = new List<VideoView>();
            if (categoryId <= 0)
            {
                return categoryVideoView;
            }
            var category = this._categoryRepository.GetEntity(ConditionEqualId(categoryId));
            if (category == null)
            {
                return categoryVideoView;
            }
            categoryVideoView.Category = new CategorysView()
            {
                ParentCategory = new HKSJ.WBVV.Entity.ViewModel.Client.CategoryView()
                {
                    Id = category.Id,
                    Name = category.Name,
                    ParentId = category.ParentId
                },
                ChildCategory = (from c in this._categoryRepository.GetEntityList()
                                 where c.State == false && c.ParentId == category.Id
                                 select new HKSJ.WBVV.Entity.ViewModel.Client.CategoryView()
                                 {
                                     Id = c.Id,
                                     Name = c.Name,
                                     ParentId = c.ParentId
                                 }).ToList()
            };
            categoryVideoView.HotCategoryVideo = GetHotCategoryVideo(categoryId);
            categoryVideoView.RecommendCategoryVideo = GetRecommendCategoryVideo(categoryId);
            return categoryVideoView;
        }
        #endregion

        #endregion

        #region ����ҳ������ȡ����ҳ�����Ƶ�б�

        #region ��ȡ�����µ�������Ƶ
        /// <summary>
        /// ��ȡ�����µ�������Ƶ
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        private IQueryable<VideoView> GetCategoryVideos(int categoryId)
        {
            IQueryable<VideoView> queryable = null;
            if (categoryId <= 0)
            {
                return null;
            }
            var category = this._categoryRepository.GetEntity(ConditionEqualId(categoryId));
            if (category == null)
            {
                return null;
            }
            queryable = (from video in this._videoRepository.GetEntityList()
                         join cate in this._categoryRepository.GetEntityList() on video.CategoryId equals cate.Id
                         where video.State == false //����
                               && video.VideoState == 3 //���ͨ��
                               && cate.State == false //����
                               && cate.LocationPath.StartsWith(category.LocationPath) //ƥ������µ������ӷ���
                         orderby video.SortNum descending,//��������������
                             video.PlayCount descending,//�����Ž���
                             video.CreateTime descending//������ʱ�併��
                         select new VideoView()
                         {
                             DictionaryId = GetDictionarys('r', 'c', video.Filter),
                             DictionaryViews = GetDictionaryViewList(video.Filter),
                             CategoryName = cate.Name,//���ͷ�������
                             IsHot = video.IsHot,
                             IsRecommend = video.IsRecommend,
                             PlayCount = video.PlayCount,
                             About = video.About,
                             BigPicturePath = video.BigPicturePath,
                             CreateTime = video.CreateTime,
                             Director = video.Director,
                             Id = video.Id,
                             IsOfficial = video.IsOfficial,
                             SmallPicturePath = video.SmallPicturePath,
                             Starring = video.Starring,
                             Tags = video.Tags,
                             Title = video.Title,
                             Filter = video.Filter,
                             UpdateTime = video.UpdateTime.HasValue ? Convert.ToDateTime(video.UpdateTime) : video.CreateTime
                         }
                ).AsQueryable();
            return queryable;
        }
        #endregion

        #region ������Ƶ
        /// <summary>
        /// ������Ƶ
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sortName"></param>
        /// <param name="totalCount"></param>
        /// <param name="totalIndex"></param>
        /// <returns></returns>
        public IList<VideoView> GetCategoryVideo(string filter, string sortName, out int totalCount, out int totalIndex)
        {
            IList<VideoView> videoViews = new List<VideoView>();
            if (string.IsNullOrEmpty(filter))
            {
                totalCount = 0;
                totalIndex = 0;
                return videoViews;
            }
            var dictionarys = GetDictionarys('g', filter);
            if (dictionarys.Count <= 0)
            {
                totalCount = 0;
                totalIndex = 0;
                return videoViews;
            }
            IQueryable<VideoView> queryable = null;
            foreach (var dict in dictionarys)
            {
                //����
                if (dict.Key.ToLower() == "c")
                {
                    var dicts = GetDictionarys('r', 'c', dict.Value);
                    if (dicts.Count > 0)
                    {
                        //ֻ��һ������
                        var item = dicts.First();
                        queryable = GetCategoryVideos(item.Key);
                        if (item.Value > 0)
                        {
                            queryable = GetCategoryVideos(item.Value);
                        }
                    }
                }
                //�ֵ�
                else if (dict.Key.ToLower() == "d")
                {
                    var dicts = GetDictionarys('r', 'c', dict.Value);
                    if (dicts.Count > 0)
                    {
                        foreach (var item in dicts)
                        {
                            if (item.Value > 0 && queryable != null)
                            {
                                queryable = queryable.Where(q => GetDictionarys('r', 'c', q.Filter).Any(i => i.Key == item.Key && i.Value == item.Value));
                            }
                        }
                    }
                }
            }
            if (queryable == null)
            {
                totalCount = 0;
                totalIndex = 0;
                return videoViews;
            }
            if (string.IsNullOrEmpty(sortName) || sortName.ToLower().Equals("time"))//������ʱ������
            {
                queryable = queryable.OrderBy(OrderCondtionUpdateTime(true));
            }
            else if (sortName.ToLower().Equals("hot"))
            {
                queryable = queryable.OrderBy(OrderCondtionPlayCount(true));
            }
            bool isExists = queryable.Any();
            totalCount = isExists ? queryable.Count() : 0;
            totalIndex = totalCount % this.PageSize == 0
                ? (totalCount / this.PageSize)
                : (totalCount / this.PageSize + 1);
            if (this.PageIndex <= 0)
            {
                this.PageIndex = 1;
            }
            if (this.PageIndex >= totalIndex)
            {
                this.PageIndex = totalIndex;
            }
            if (isExists)
            {
                videoViews = queryable.Skip((this.PageIndex - 1) * this.PageSize).Take(this.PageSize).ToList();
            }
            return videoViews;
        }
        #endregion

        #region ��ҳ��ʾ������Ƶ
        /// <summary>
        /// ��ҳ��ʾ������Ƶ
        /// </summary>
        /// <param name="filter">��������gc2c3rgd2c4r</param>
        /// <param name="sortName">��������Ĭ������</param>
        /// <returns></returns>
        public PageResult GetFilterVideo(string filter, string sortName)
        {
            int totalCount = 0;
            int totalIndex = 0;
            IList<VideoView> plateViews = GetCategoryVideo(filter, sortName, out totalCount, out totalIndex);
            return new PageResult()
            {
                PageSize = this.PageSize,
                PageIndex = this.PageIndex,
                TotalCount = totalCount,
                TotalIndex = totalIndex,
                Data = plateViews
            };
        }
        #endregion

        #endregion

        #region ������ƵID�����Ƶʵ��
        /// <summary>
        /// ������ƵID�����Ƶʵ��
        /// </summary>
        /// <param name="id">��ƵID</param>
        /// <returns></returns>
        public Video GetAVideoInfoById(int id)
        {
            Video video;
            video = this._videoRepository.GetEntity(ConditionEqualId(id));
            video.VideoPath += "?pm3u8/0";
            video.VideoPath = _qiniuUploadBusiness.GetDownloadUrl(video.VideoPath, "video");
            return video;
        }
        #endregion

        #region ����һ����Ƶʵ��
        /// <summary>
        /// ����һ����Ƶʵ��
        /// </summary>
        /// <param name="videoPara"></param>
        /// <returns></returns>
        public bool UpdateAVideo(MyVideoPara videoPara)
        {
            var video = _videoRepository.GetEntity(ConditionEqualId(videoPara.Id));
            if (video == null || video.Id < 1) return false;
            video.CategoryId = videoPara.CategoryId;
            video.Tags = videoPara.Tags.Trim('|');
            video.Title = videoPara.Title;
            video.About = videoPara.About;
            video.Copyright = videoPara.Copyright;
            video.IsPublic = videoPara.IsPublic > 0 ? true : false;
            video.IsOfficial = videoPara.IsOfficial > 0 ? true : false;
            if (!string.IsNullOrEmpty(videoPara.BigPicturePath) && !video.BigPicturePath.Equals(videoPara.BigPicturePath))
            {
                if (!string.IsNullOrEmpty(video.BigPicturePath))
                {
                    _qiniuUploadBusiness.DeleteQiniuImageByKey(video.BigPicturePath);
                }
                video.BigPicturePath = videoPara.BigPicturePath;
            }
            if (!string.IsNullOrEmpty(videoPara.SmallPicturePath) && !video.SmallPicturePath.Equals(videoPara.SmallPicturePath))
            {
                if (!string.IsNullOrEmpty(video.SmallPicturePath))
                {
                    _qiniuUploadBusiness.DeleteQiniuImageByKey(video.SmallPicturePath);
                }
                video.SmallPicturePath = videoPara.SmallPicturePath;
            }
            if (videoPara.CreateManageId > 0)
            {
                video.CreateManageId = videoPara.CreateManageId;
            }
            if (!string.IsNullOrEmpty(videoPara.Filter))
            {
                video.Filter = videoPara.Filter;
            }
            var flag = this._videoRepository.UpdateEntity(video);
            if (flag)
            {
                UpdateAVideoIndex(video);
            }

            //TODO insert ��ǿ��ӱ�ǩ
            try
            {
                this._tagsBusiness = ((Autofac.IContainer)HttpRuntime.Cache["containerKey"]).Resolve<ITagsBusiness>();
                //�ϴ���Ƶ����
                this._tagsBusiness.UserId = video.CreateManageId;
                this._tagsBusiness.AsyncCreateTags();
            }
            catch (Exception ex)
            {
#if !DEBUG
                      LogBuilder.Log4Net.Error("���±�ǩʧ��", ex.MostInnerException());
#else
                Console.WriteLine(LanguageUtil.Translate("api_Business_Video_UpdateAVideo_updateTagsFailed") + ex.MostInnerException().Message);
#endif
            }

            return flag;
        }
        #endregion

        /// <summary>
        /// ��ȡ�û�����Ƶ������
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetPlayCountByUserId(int userId)
        {
            int playCount = 0;
            //���û�����Ƶ������
            var model = (from video in
                             (from video in _videoRepository.GetEntityList().Where(p => !p.State).Where(p => p.VideoState == 3)
                              where this._videoPlayRecordRepository.GetEntityList()
                                       .Where(p => p.UserId == userId)
                                       .Where(p => p.VideoId == video.Id).Count() > 0
                              select new
                              {
                                  PlayCount = video.PlayCount,
                                  Dummy = "x"
                              })
                         group video by new { video.Dummy } into g
                         select new
                         {
                             PlayCount = g.Sum(p => p.PlayCount)
                         }).AsQueryable();
            if (model.FirstOrDefault() != null)
                playCount = model.FirstOrDefault().PlayCount;
            return playCount;
        }

        public dynamic CreateVideo(MyVideoPara videoPara)
        {
            var video = new Video();
            video.CategoryId = videoPara.CategoryId;
            video.Tags = videoPara.Tags;
            video.Title = videoPara.Title;
            video.About = videoPara.About;
            video.Copyright = videoPara.Copyright;
            video.IsPublic = videoPara.IsPublic > 0 ? true : false;
            video.BigPicturePath = videoPara.BigPicturePath;
            video.CreateManageId = videoPara.CreateManageId;

            _videoRepository.CreateEntity(video);
            return new { Id = video.Id };

        }

        #region ������Ƶ��ҳ
        /// <summary>
        /// ������Ƶ��ҳ
        /// </summary>
        /// <param name="searchKey">�����ؼ���</param>
        /// <param name="totalcount">�ܼ�¼��</param>
        /// <param name="pageSize">ҳ��</param>
        /// <param name="pageIndex">ҳ����</param>
        /// <param name="sortName">������</param>
        /// <returns></returns>
        public List<VideoView> SearchVideoByPage(string searchKey, out int totalcount, int pageSize, int pageIndex, SortName sortName)
        {
            searchKey = searchKey.UrlDecode();
            searchKey = searchKey.Replace(" ", "");
            if (searchKey.Length > 14)
            {
                searchKey = searchKey.Substring(0, 15);
            }
            List<VideoView> videoResult = new List<VideoView>();
            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(IndexManager.indexPath), new NoLockFactory());
            IndexReader reader = IndexReader.Open(directory, true);
            IndexSearcher searcher = new IndexSearcher(reader);
            BooleanQuery queryOr = new BooleanQuery();
            TermQuery querytitle = null;
            TermQuery queryabout = null;
            // TermQuery querytags = null;
            string[] splitWords = SplitContent.SplitWords(searchKey);
            foreach (string word in splitWords)
            {
                querytitle = new TermQuery(new Term("title", word));
                queryabout = new TermQuery(new Term("about", word));
                // querytags = new TermQuery(new Term("tags", word));
                queryOr.Add(querytitle, BooleanClause.Occur.SHOULD);//�������� ����ΪOr��ϵ
                queryOr.Add(queryabout, BooleanClause.Occur.SHOULD);
                //  queryOr.Add(querytags, BooleanClause.Occur.SHOULD);
            }
            BooleanQuery querystate = new BooleanQuery();
            var querystateTerm = new TermQuery(new Term("videoState", "3"));
            querystate.Add(querystateTerm, BooleanClause.Occur.MUST);
            BooleanQuery bqQuery = new BooleanQuery();
            bqQuery.Add(querystate, BooleanClause.Occur.MUST);
            bqQuery.Add(queryOr, BooleanClause.Occur.MUST);
            //--------------------------------------
            TopScoreDocCollector collector = TopScoreDocCollector.create(1000, true);
            ScoreDoc[] docs = null;
            if (sortName > 0)
            {
                string sortField = (int)sortName == 1 ? "createTimeNum" : "playCount";
                Sort sort = new Sort();
                SortField sfdata = null;
                if (sortField == "playCount")
                {
                    sfdata = new SortField(sortField, SortField.INT, true);
                }
                else
                {
                    sfdata = new SortField(sortField, SortField.LONG, true);
                }
                sort.SetSort(sfdata);
                TopFieldDocs topdocss = searcher.Search(bqQuery, null, 1000, sort);
                totalcount = topdocss.totalHits;
                docs = topdocss.scoreDocs;
            }
            else
            {
                TopDocs topdocss = searcher.Search(bqQuery, 1000);
                totalcount = topdocss.totalHits;
                docs = topdocss.scoreDocs;
            }
            //searcher.Search(queryOr, null, collector);
            //totalcount = collector.GetTotalHits();
            ////ScoreDoc[] docs = collector.TopDocs((PageIndex - 1) * PageSize, PageSize).scoreDocs;//ȡǰʮ������  ����ͨ����ʵ��LuceneNet���������ҳ
            //ScoreDoc[] docs = collector.TopDocs(0, totalcount).scoreDocs;
            int startIndex = (pageIndex - 1) * pageSize;
            int endIndex = 0;
            if (pageIndex * pageSize - totalcount < 0)
            {
                endIndex = pageIndex * pageSize;
            }
            else
            {
                endIndex = totalcount;
            }
            for (int i = startIndex; i < endIndex; i++)
            {
                int docId = docs[i].doc;
                Document doc = searcher.Doc(docId);
                VideoView video = new VideoView();
                video.Title = SplitContent.HightLight(searchKey, doc.Get("title")) == "" ? doc.Get("title") : SplitContent.HightLight(searchKey, doc.Get("title"));
                video.About = SplitContent.HightLight(searchKey, doc.Get("about")) == "" ? doc.Get("about") : SplitContent.HightLight(searchKey, doc.Get("about"));
                video.Tags = SplitContent.HightLight(searchKey, doc.Get("tags")) == "" ? doc.Get("tags") : SplitContent.HightLight(searchKey, doc.Get("tags"));
                video.Id = Convert.ToInt32(doc.Get("id"));
                video.SmallPicturePath = doc.Get("smallPicturePath");
                video.BigPicturePath = doc.Get("bigPicturePath");
                video.CreateTime = Convert.ToDateTime(doc.Get("createTime"));
                video.IsOfficial = Convert.ToBoolean(doc.Get("isOfficial"));
                video.PlayCount = Convert.ToInt32(doc.Get("playCount"));
                videoResult.Add(video);
            }

            #region ����list�������ҳ
            //if (sortName > 0)
            //{
            //    if (sortName == SortName.PlayCount)
            //    {
            //        videoResult =
            //            videoResult.OrderByDescending(c => c.PlayCount).ToList();
            //    }
            //    else
            //    {
            //        videoResult =
            //            videoResult.OrderByDescending(c => c.CreateTime).ToList();
            //    }
            //}
            //int count = videoResult.Count;
            //if (pageIndex * pageSize - count <0)
            //{
            //    videoResult =
            //        videoResult.GetRange((pageIndex - 1) * pageSize, pageSize);
            //}
            //else
            //{
            //    videoResult =
            //       videoResult.GetRange((pageIndex - 1) * pageSize, count - (pageIndex - 1) * pageSize);
            //} 
            #endregion
            return videoResult;
        }
        #endregion

        #region ����ҳ�Ƽ���Ƶ

        /// <summary>
        /// ��������Ƽ���Ƶ
        /// </summary>
        /// <param name="searchKey"></param>
        /// <param name="videoId"></param>
        /// <param name="recommendationNum"></param>
        /// <returns></returns>
        public List<VideoView> SearchVideoByRecom(string searchKey, int videoId, int recommendationNum = 6)
        {
            searchKey = searchKey.UrlDecode();
            List<string> tags = new List<string>();
            if (searchKey.IndexOf('|') > 0)
            {
                tags = searchKey.Split('|').ToList();
            }
            else
            {
                tags.Add(searchKey);
            }

            List<VideoView> videoResult = new List<VideoView>();
            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(IndexManager.indexPath), new NoLockFactory());
            IndexReader reader = IndexReader.Open(directory, true);
            IndexSearcher searcher = new IndexSearcher(reader);
            BooleanQuery queryOr = new BooleanQuery();
            TermQuery querytitle = null;
            TermQuery querytrue = null;
            foreach (string word in tags)
            {
                querytitle = new TermQuery(new Term("tags", word));
                queryOr.Add(querytitle, BooleanClause.Occur.SHOULD);
            }
            BooleanQuery querystate = new BooleanQuery();
            var querystateTerm = new TermQuery(new Term("videoState", "3"));
            querystate.Add(querystateTerm, BooleanClause.Occur.MUST);
            BooleanQuery bqQuery = new BooleanQuery();
            bqQuery.Add(querystate, BooleanClause.Occur.MUST);
            bqQuery.Add(queryOr, BooleanClause.Occur.MUST);
            Sort sort = new Sort(new SortField("playCount", SortField.INT, true));
            ScoreDoc[] docs = searcher.Search(bqQuery, null, 100, sort).scoreDocs;
            int lenNum = docs.Length < recommendationNum ? docs.Length : recommendationNum;
            for (int i = 0; i < lenNum; i++)
            {
                int docId = docs[i].doc;
                Document doc = searcher.Doc(docId);
                VideoView video = new VideoView();
                video.Title = SplitContent.HightLight(searchKey, doc.Get("title")) == "" ? doc.Get("title") : SplitContent.HightLight(searchKey, doc.Get("title"));
                video.About = SplitContent.HightLight(searchKey, doc.Get("about")) == "" ? doc.Get("about") : SplitContent.HightLight(searchKey, doc.Get("about"));
                video.Id = Convert.ToInt32(doc.Get("id"));
                if (video.Id == videoId)
                {
                    if (lenNum >= 6)
                    {
                        lenNum++;
                    }
                    continue;
                }
                video.PlayCount = Convert.ToInt32(doc.Get("playCount"));
                video.CommentCount = Convert.ToInt32(doc.Get("commentCount"));
                video.SmallPicturePath = doc.Get("smallPicturePath");
                videoResult.Add(video);
            }
            return videoResult;
        }
        #endregion


        #region ����ͷ����Ƶ
        /// <summary>
        /// ����ͷ����Ƶ
        /// </summary>
        /// <param name="searchKey"></param>
        /// <returns></returns>
        public List<VideoView> SearchVideoByTop(string searchKey)
        {
            searchKey = searchKey.UrlDecode();
            List<VideoView> videoResult = new List<VideoView>();
            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(IndexManager.indexPath), new NoLockFactory());
            IndexReader reader = IndexReader.Open(directory, true);
            IndexSearcher searcher = new IndexSearcher(reader);
            BooleanQuery queryOr = new BooleanQuery();
            TermQuery querytitle = null;
            TermQuery querytrue = null;
            string[] splitWords = SplitContent.SplitWords(searchKey);
            foreach (string word in splitWords)
            {
                querytitle = new TermQuery(new Term("title", word));
                queryOr.Add(querytitle, BooleanClause.Occur.MUST);//�������� ����ΪOr��ϵ
            }
            if (querytitle == null) return videoResult;
            querytrue = new TermQuery(new Term("isofficialtrue", "�ٷ�"));
            queryOr.Add(querytrue, BooleanClause.Occur.MUST);
            TermQuery querystate = null;
            querystate = new TermQuery(new Term("videoState", "3"));
            queryOr.Add(querystate, BooleanClause.Occur.MUST);
            //--------------------------------------
            TopScoreDocCollector collector = TopScoreDocCollector.create(1000, true);
            searcher.Search(queryOr, null, collector);
            ScoreDoc[] docs = collector.TopDocs(0, 3).scoreDocs;//ȡǰ5������  ����ͨ����ʵ��LuceneNet���������ҳ
            for (int i = 0; i < docs.Length; i++)
            {
                int docId = docs[i].doc;
                Document doc = searcher.Doc(docId);
                VideoView video = new VideoView();
                video.Title = SplitContent.HightLight(searchKey, doc.Get("title")) == "" ? doc.Get("title") : SplitContent.HightLight(searchKey, doc.Get("title"));
                video.About = SplitContent.HightLight(searchKey, doc.Get("about")) == "" ? doc.Get("about") : SplitContent.HightLight(searchKey, doc.Get("about"));
                video.Tags = SplitContent.HightLight(searchKey, doc.Get("tags")) == "" ? doc.Get("tags") : SplitContent.HightLight(searchKey, doc.Get("tags"));
                video.Id = Convert.ToInt32(doc.Get("id"));
                video.SmallPicturePath = doc.Get("smallPicturePath");
                video.BigPicturePath = doc.Get("bigPicturePath");
                video.CreateTime = Convert.ToDateTime(doc.Get("createTime"));
                video.IsOfficial = Convert.ToBoolean(doc.Get("isOfficial"));
                video.PlayCount = Convert.ToInt32(doc.Get("playCount"));
                videoResult.Add(video);
            }
            // videoResult = videoResult.OrderByDescending(c => c.CreateTime).ToList();
            return videoResult;
        }
        #endregion

        #region �����ҵ���Ƶ��ҳ
        /// <summary>
        /// �����ҵ���Ƶ��ҳ
        /// </summary>
        /// <param name="searchKey">�����ؼ���</param>
        /// <param name="pageSize">ҳ��</param>
        /// <param name="pageIndex">ҳ����</param>
        /// <param name="userId">�û�ID</param>
        /// <returns></returns>
        public MyVideoViewResult SearchMyVideoByPage(int userId, string searchKey, int pageIndex, int pageSize, int videoState)
        {
            searchKey = searchKey.UrlDecode();
            searchKey = searchKey.Replace(" ", "");
            if (searchKey.Length > 14)
            {
                searchKey = searchKey.Substring(0, 15);
            }
            var videoResult = new MyVideoViewResult() { MyVideoViews = new List<MyVideoView>(), TotalCount = 0 };
            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(IndexManager.indexPath), new NoLockFactory());
            IndexReader reader = IndexReader.Open(directory, true);
            IndexSearcher searcher = new IndexSearcher(reader);
            BooleanQuery queryOr = new BooleanQuery();
            TermQuery querytitle = null;
            string[] splitWords = SplitContent.SplitWords(searchKey);
            foreach (string word in splitWords)
            {
                querytitle = new TermQuery(new Term("title", word));
                queryOr.Add(querytitle, BooleanClause.Occur.SHOULD);
            }
            //--------------------------------------
            TopDocs topdocs=  searcher.Search(queryOr, 1000);
            var docs = topdocs.scoreDocs;
            var docslen = docs.Length;
            List<MyVideoView> list = new List<MyVideoView>();
            for (int i = 0; i < docslen; i++)
            {
                int docId = docs[i].doc;
                Document doc = searcher.Doc(docId);
                MyVideoView video = new MyVideoView();
                video.Title = doc.Get("title");
                video.About = doc.Get("about");
                video.Tags = doc.Get("tags");
                video.Id = Convert.ToInt32(doc.Get("id"));
                video.SmallPicturePath = doc.Get("smallPicturePath");
                video.CreateTime = Convert.ToDateTime(doc.Get("createTime"));
                video.PlayCount = Convert.ToInt32(doc.Get("playCount"));
                video.VideoState = Convert.ToInt16(doc.Get("videoState"));
                video.CreateManageId = Convert.ToInt32(doc.Get("createManageId"));
                list.Add(video);
            }
            var mylist = list.Where(c => c.CreateManageId == userId && (videoState == -1 || (videoState != -1 && c.VideoState == videoState))).ToList();
            videoResult.TotalCount = mylist.Count;
            videoResult.PageCount = videoResult.TotalCount % pageSize == 0
                ? videoResult.TotalCount / pageSize
                : videoResult.TotalCount / pageSize + 1;
            int startIndex = (pageIndex - 1) * pageSize;
            int endIndex = 0;
            videoResult.MyVideoViews = mylist.Skip(startIndex).Take(pageSize).ToList();
            this._videoApproveBusiness = ((Autofac.IContainer)HttpRuntime.Cache["containerKey"]).Resolve<IVideoApproveBusiness>();
            foreach (var myVideoView in videoResult.MyVideoViews)
            {
                if (myVideoView.VideoState == 4)
                {
                    myVideoView.ApproveContent = this._videoApproveBusiness.GetApproveContentByVideoId(myVideoView.Id);
                }
            }
            return videoResult;
        }
        #endregion

        #region ����һ����Ƶ��Ϣ����
        public void AddAVideoIndex(Video videoInfo)
        {
            if (string.IsNullOrEmpty(videoInfo.Title) || string.IsNullOrEmpty(videoInfo.Tags))
            {
                return;
            }
            IndexManager.VideoIndex.Add(videoInfo);
        }
        #endregion

        #region ɾ��һ����Ƶ����
        /// <summary>
        /// ɾ��һ����Ƶ����
        /// </summary>
        /// <param name="vid">��ƵID</param>s
        public void DelAVideoIndex(int vid)
        {
            IndexManager.VideoIndex.Del(vid);
        }
        #endregion

        #region ����һ����Ƶ����
        public void UpdateAVideoIndex(Video videoInfo)
        {
            if (string.IsNullOrEmpty(videoInfo.Title) || string.IsNullOrEmpty(videoInfo.Tags))
            {
                return;
            }
            IndexManager.VideoIndex.Mod(videoInfo);
        }
        #endregion

        #region ɾ��ȫ����������������������Ƶ����

        public void DelAndUpdateAllIndex()
        {
            IndexManager.VideoIndex.delAllIndex();
            List<Entity.Video> videolist = this._videoRepository.GetEntityList().ToList();
            foreach (var item in videolist)
            {
                AddAVideoIndex(item);
            }
        }
        #endregion

        #region ��������ļ��к������ļ��Ƿ���ڣ�û���򴴽�
        public void CheckIndexFile()
        {
            try
            {
                if (Directory.Exists(IndexManager.indexPath) == false)
                {
                    Directory.CreateDirectory(IndexManager.indexPath);
                    FSDirectory directory = FSDirectory.Open(new DirectoryInfo(IndexManager.indexPath), new NativeFSLockFactory());
                    List<Video> videolist = this._videoRepository.GetEntityList().ToList();
                    foreach (var item in videolist)
                    {
                        AddAVideoIndex(item);
                    }
                }
                else
                {
                    FSDirectory directory = FSDirectory.Open(new DirectoryInfo(IndexManager.indexPath), new NativeFSLockFactory());
                    if (IndexReader.IndexExists(directory) == false)
                    {
                        List<Video> videolist = this._videoRepository.GetEntityList().ToList();
                        foreach (var item in videolist)
                        {
                            AddAVideoIndex(item);
                        }
                    }
                }
                StartNewThread();
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    LogBuilder.Log4Net.Info(LanguageUtil.Translate("api_Business_Video_CheckIndexFile_updateTheIndexThreadTerminates"));
                }
                else
                {
#if !DEBUG
                    LogBuilder.Log4Net.Error(ex.MostInnerException().Message);
#else
                    Console.WriteLine(ex.MostInnerException().Message);
#endif
                }
            }

        }
        #endregion

        #region ��ʼ���̹ŷִʲ�������̨���̴�����������
        public void StartNewThread()
        {
            IndexManager.VideoIndex.StartNewThread();
        }
        #endregion

        /// <summary>
        /// ɾ����Ƶ���ݺ��ļ�
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteVideoInfo(int id)
        {
            var video = this._videoRepository.GetEntity(ConditionEqualId(id));
            try
            {
                var flag = this._videoRepository.DeleteEntity(video);
                if (flag)
                {
                    //ɾ����ţ�ϵ��ļ�
                    _qiniuUploadBusiness.DeleteQiniuData(video);

                    //ɾ������
                    DelAVideoIndex(id);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #region ��ȡ�����ȵ���Ƶ
        public List<VideoView> GetYesRecVideos(int num)
        {
            var list = new List<VideoView>();
            string strYes = DateTime.Now.AddDays(-1).ToShortDateString();
            DateTime dtYes1 = Convert.ToDateTime(strYes + " 00:00:00");
            DateTime dtYes2 = Convert.ToDateTime(strYes + " 23:59:59");

            var query = (from a in
                             (
                                 (from vr in _videoPlayRecordRepository.GetEntityList()
                                  where vr.CreateTime >= dtYes1 && vr.CreateTime <= dtYes2
                                  group vr by new { vr.VideoId }
                                      into g
                                      select new
                                      {
                                          VideoId = g.Key.VideoId,
                                          yPlayCount = g.Count(p => true)
                                      }))
                         join v in _videoRepository.GetEntityList(CondtionEqualState()) on new { Id = Convert.ToInt64(a.VideoId) } equals new { v.Id }
                         select new VideoView
                         {
                             Id = (int)v.Id,
                             Title = (string.IsNullOrEmpty(v.Title) ? "" : v.Title),
                             SmallPicturePath = v.SmallPicturePath,
                             About = v.About,
                             PlayCount = a.yPlayCount
                         }).AsQueryable().OrderByDescending(w => w.PlayCount);

            list = query.Take(num).ToList();
            return list;
        }
        #endregion

        #endregion

        #region ����������



        #endregion

        #region �������
        /// <summary>
        /// �Ƚ���Ƶ״̬���
        /// </summary>
        /// <param name="videoState">��Ƶ״̬��0��ת���У�1��ת��ʧ�ܣ�2������У�3�����ͨ����4����˲�ͨ����</param>
        /// <returns></returns>
        private Condtion CondtionEqualVideoState(int videoState)
        {
            var condtion = new Condtion()
            {
                FiledName = "VideoState",
                FiledValue = videoState,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.Equal
            };
            return condtion;
        }
        /// <summary>
        /// �Ƚ��ֵ������
        /// </summary>
        /// <param name="dictionaryId"></param>
        /// <returns></returns>
        private Condtion CondtionEquealDictionaryId(int dictionaryId)
        {
            var condtion = new Condtion()
            {
                FiledName = "DictionaryId",
                FiledValue = dictionaryId,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.Equal
            };
            return condtion;
        }
        /// <summary>
        /// �Ƚ���Ƶ�Ƿ��Ƽ����
        /// </summary>
        /// <param name="isRecommend">Ĭ���Ƽ�</param>
        /// <returns></returns>
        private Condtion CondtionEqualIsRecommend(bool isRecommend = true)
        {
            var condtion = new Condtion()
            {
                FiledName = "IsRecommend",
                FiledValue = isRecommend,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.Equal
            };
            return condtion;
        }
        /// <summary>
        /// �Ƚ���Ƶ����ʱ����ڻ��ߵ���
        /// </summary>
        /// <param name="createTime"></param>
        /// <returns></returns>
        private Condtion CondtionGreaterThanOrEqualCreateTime(DateTime createTime)
        {
            var condtion = new Condtion()
            {
                FiledName = "CreateTime",
                FiledValue = createTime.ToString("yyyy-MM-dd"),
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.GreaterThanOrEqual
            };
            return condtion;
        }
        /// <summary>
        /// �Ƚ���Ƶ���������
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        private Condtion CondtionEqualCategoryId(int categoryId)
        {
            var condtion = new Condtion()
            {
                FiledName = "CategoryId",
                FiledValue = categoryId,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.Equal
            };
            return condtion;
        }
        /// <summary>
        /// �Ƚ���Ƶ����λ��·������
        /// </summary>
        /// <param name="locationPath"></param>
        /// <returns></returns>
        private Condtion CondtionContainsLocationPath(string locationPath)
        {
            var condtion = new Condtion()
            {
                FiledName = "LocationPath",
                FiledValue = locationPath,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.Contains
            };
            return condtion;
        }
        /// <summary>
        /// �Ƚ���Ƶ�Ƿ��������
        /// </summary>
        /// <param name="isHot">Ĭ��������</param>
        /// <returns></returns>
        private Condtion CondtionEqualIsHot(bool isHot = true)
        {
            var condtion = new Condtion()
            {
                FiledName = "IsHot",
                FiledValue = isHot,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.Equal
            };
            return condtion;
        }

        /// <summary>
        /// �Ƚ��ϴ��û�ID���
        /// </summary>
        /// <param name="createManageId"></param>
        /// <returns></returns>
        private Condtion CondtionEqualCreateManageId(int createManageId)
        {
            var condtion = new Condtion()
            {
                FiledName = "CreateManageId",
                FiledValue = createManageId,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.Equal
            };
            return condtion;
        }
        #endregion

        #region �������
        /// <summary>
        /// ��PlayCount����
        /// </summary>
        /// <param name="isDesc"></param>
        /// <returns></returns>
        protected OrderCondtion OrderCondtionPlayCount(bool isDesc)
        {
            var orderCodtion = new OrderCondtion()
            {
                FiledName = "PlayCount",
                IsDesc = isDesc
            };
            return orderCodtion;
        }
        /// <summary>
        /// ��UpdateTime����
        /// </summary>
        /// <param name="isDesc"></param>
        /// <returns></returns>
        protected OrderCondtion OrderCondtionUpdateTime(bool isDesc)
        {
            var orderCodtion = new OrderCondtion()
            {
                FiledName = "UpdateTime",
                IsDesc = isDesc
            };
            return orderCodtion;
        }

        #endregion

        #region ��Ƶ�б�
        /// <summary>
        /// ��ȡ��Ƶ��ҳ����
        /// </summary>
        /// <param name="condtions">��ѯ����</param>
        /// <param name="orderCondtions">��������</param>
        public PageResult GetVideoPageResult(IList<Condtion> condtions, IList<OrderCondtion> orderCondtions)
        {
            int totalCount = 0;
            int totalIndex = 0;
            IList<VideoView> plateViews = GetVideoList(condtions, orderCondtions, out totalCount, out totalIndex);
            return new PageResult()
            {
                PageSize = this.PageSize,
                PageIndex = this.PageIndex,
                TotalCount = totalCount,
                Data = plateViews
            };
        }
        /// <summary>
        /// ��ȡ��Ƶ��ҳ����
        /// </summary>
        /// <param name="condtions">��ѯ����</param>
        /// <param name="orderCondtions">��������</param>
        public PageResult GetVideoByCategoryIdPageResult(int categoryId, IList<Condtion> condtions, IList<OrderCondtion> orderCondtions)
        {
            int totalCount = 0;
            int totalIndex = 0;
            IList<VideoView> plateViews = new List<VideoView>();
            if (categoryId == 0)
            {
                plateViews = GetVideoByCategoryLists(categoryId, condtions, orderCondtions, out totalCount, out totalIndex);
            }
            else
            {
                plateViews = GetVideoByCategoryList(categoryId, condtions, orderCondtions, out totalCount, out totalIndex);
            }
            return new PageResult()
            {
                PageSize = this.PageSize,
                PageIndex = this.PageIndex,
                TotalCount = totalCount,
                Data = plateViews
            };
        }

        /// <summary>
        /// ����һ�����������Ƶ��Ϣ
        /// </summary>
        /// <param name="categoryId">0</param>
        /// <param name="condtions"></param>
        /// <param name="orderCondtions"></param>
        /// <param name="totalCount"></param>
        /// <param name="totalIndex"></param>
        /// <returns></returns>
        public IList<VideoView> GetVideoByCategoryLists(int categoryId, IList<Condtion> condtions, IList<OrderCondtion> orderCondtions, out int totalCount, out int totalIndex)
        {
            var video = (from p in GetVideoList()
                         join c in this._categoryRepository.GetEntityList() on p.CategoryId equals c.Id
                         select new HKSJ.WBVV.Entity.ViewModel.Client.VideoView()
                         {
                             Id = p.Id,
                             Title = string.IsNullOrEmpty(p.Title) ? "" : p.Title,
                             CategoryId = p.CategoryId,
                             SmallPicturePath = p.SmallPicturePath,
                             BigPicturePath = p.BigPicturePath,
                             VideoState = p.VideoState
                         }).AsQueryable();
            if (condtions != null && condtions.Count > 0)//��ѯ����
            {
                video = video.Query(condtions);
            }
            if (orderCondtions != null && orderCondtions.Count > 0)//��������
            {
                video = video.OrderBy(orderCondtions);
            }
            bool isExists = video.Any();
            totalCount = isExists ? video.Count() : 0;
            if (this.PageSize <= 0)
            {
                totalIndex = 0;
                var queryable = isExists
                    ? video.ToList()
                    : new List<Entity.ViewModel.Client.VideoView>();
                return queryable;
            }
            else
            {
                totalIndex = totalCount % this.PageSize == 0
                    ? (totalCount / this.PageSize)
                    : (totalCount / this.PageSize + 1);
                if (this.PageIndex <= 0)
                {
                    this.PageIndex = 1;
                }
                if (this.PageIndex >= totalIndex)
                {
                    this.PageIndex = totalIndex;
                }

                var queryable = isExists
                    ? video.Skip((this.PageIndex - 1) * this.PageSize).Take(this.PageSize).ToList()
                    : new List<Entity.ViewModel.Client.VideoView>();

                return queryable;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="condtions"></param>
        /// <param name="orderCondtions"></param>
        /// <param name="totalCount"></param>
        /// <param name="totalIndex"></param>
        /// <returns></returns>
        public IList<VideoView> GetVideoByCategoryList(int categoryId, IList<Condtion> condtions, IList<OrderCondtion> orderCondtions, out int totalCount, out int totalIndex)
        {
            var category = new Category();
            if (categoryId > 0)
            {
                category = this._categoryRepository.GetEntity(ConditionEqualId(categoryId));
                if (category == null)
                {
                    totalCount = 0;
                    totalIndex = 0;
                    return new List<VideoView>();
                }
            }
            var video = (from p in GetVideoList()
                         join c in this._categoryRepository.GetEntityList() on p.CategoryId equals c.Id
                         where c.LocationPath.StartsWith(category.LocationPath)
                         select new HKSJ.WBVV.Entity.ViewModel.Client.VideoView()
                         {
                             Id = p.Id,
                             Title = p.Title,
                             CategoryId = p.CategoryId,
                             SmallPicturePath = p.SmallPicturePath,
                             BigPicturePath = p.BigPicturePath,
                             VideoState = p.VideoState
                         }).AsQueryable();
            if (condtions != null && condtions.Count > 0)//��ѯ����
            {
                video = video.Query(condtions);
            }
            if (orderCondtions != null && orderCondtions.Count > 0)//��������
            {
                video = video.OrderBy(orderCondtions);
            }
            bool isExists = video.Any();
            totalCount = isExists ? video.Count() : 0;
            if (this.PageSize <= 0)
            {
                totalIndex = 0;
                var queryable = isExists
                    ? video.ToList()
                    : new List<Entity.ViewModel.Client.VideoView>();
                return queryable;
            }
            else
            {
                totalIndex = totalCount % this.PageSize == 0
                    ? (totalCount / this.PageSize)
                    : (totalCount / this.PageSize + 1);
                if (this.PageIndex <= 0)
                {
                    this.PageIndex = 1;
                }
                if (this.PageIndex >= totalIndex)
                {
                    this.PageIndex = totalIndex;
                }

                var queryable = isExists
                    ? video.Skip((this.PageIndex - 1) * this.PageSize).Take(this.PageSize).ToList()
                    : new List<Entity.ViewModel.Client.VideoView>();

                return queryable;
            }
        }



        private IQueryable<Video> GetVideoList()
        {
            return this._videoRepository.GetEntityList(CondtionEqualState());
        }

        /// <summary>
        /// ��ȡ��Ƶ����
        /// </summary>
        /// <param name="condtions">��ѯ����</param>
        /// <param name="orderCondtions">��������</param>
        /// <param name="totalCount">�����ܵ�����</param>
        /// <param name="totalIndex">�����ܵ�ҳ��</param>
        /// <returns></returns>
        public IList<VideoView> GetVideoList(IList<Condtion> condtions, IList<OrderCondtion> orderCondtions, out int totalCount, out int totalIndex)
        {
            var plate = (from p in GetVideoList()
                         join c in this._categoryRepository.GetEntityList() on p.CategoryId equals c.Id
                         into cjoin
                         from cate in cjoin.DefaultIfEmpty()
                         select new HKSJ.WBVV.Entity.ViewModel.Client.VideoView()
                         {
                             Id = p.Id,
                             Title = p.Title,
                             CategoryId = p.CategoryId,
                             SmallPicturePath = p.SmallPicturePath,
                             BigPicturePath = p.BigPicturePath
                         }).AsQueryable();
            if (condtions != null && condtions.Count > 0)//��ѯ����
            {
                plate = plate.Query(condtions);
            }
            if (orderCondtions != null && orderCondtions.Count > 0)//��������
            {
                plate = plate.OrderBy(orderCondtions);
            }
            bool isExists = plate.Any();
            totalCount = isExists ? plate.Count() : 0;
            if (this.PageSize <= 0)
            {
                totalIndex = 0;
                var queryable = isExists
                    ? plate.ToList()
                    : new List<Entity.ViewModel.Client.VideoView>();
                return queryable;
            }
            else
            {
                totalIndex = totalCount % this.PageSize == 0
                    ? (totalCount / this.PageSize)
                    : (totalCount / this.PageSize + 1);
                if (this.PageIndex <= 0)
                {
                    this.PageIndex = 1;
                }
                if (this.PageIndex >= totalIndex)
                {
                    this.PageIndex = totalIndex;
                }

                var queryable = isExists
                    ? plate.Skip((this.PageIndex - 1) * this.PageSize).Take(this.PageSize).ToList()
                    : new List<Entity.ViewModel.Client.VideoView>();

                return queryable;
            }
        }
        #endregion


        /// <summary>
        /// ��ȡ��Ƶ�������ص�ַ
        /// </summary>GetVideoViewListByPlateId
        /// <param name="id">��ƵId</param>
        /// <returns></returns>
        public Entity.ViewModel.Client.VideoView GetVideoById(long id)
        {
            var condtion = new Condtion()
            {
                FiledName = "Id",
                FiledValue = id,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.Equal
            };
            Video info=_videoRepository.GetEntity(condtion);
            if (info == null) return new Entity.ViewModel.Client.VideoView(); ;
            Entity.ViewModel.Client.VideoView model = new Entity.ViewModel.Client.VideoView()
            {
                 Id=info.Id,
                 About=info.About,
                 BigPicturePath = info.BigPicturePath,
                 CategoryId = info.CategoryId,
                 Tags = info.Tags,
                 Title = info.Title,
                 SmallPicturePath = info.SmallPicturePath,
                 VideoState = info.VideoState,
                 IsPublic = info.IsPublic,
                 Copyright = info.Copyright
            };
            return model;
        }


        /// <summary>
        /// ����Դ��Ƶ�ļ���ȡ��Ƶ
        /// </summary>
        /// <param name="key">Դ��Ƶ�ļ�����VideoRosella �ֶΣ�</param>
        /// <returns></returns>
        private Video GetVideoByKey(string key)
        {
            var condtion = new Condtion()
            {
                FiledName = "VideoRosella",
                FiledValue = key,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.Equal
            };
            return _videoRepository.GetEntity(condtion);
        }

    }
}