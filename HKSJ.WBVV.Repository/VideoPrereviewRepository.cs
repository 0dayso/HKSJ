



using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using HKSJ.WBVV.Common.Assert;
using HKSJ.WBVV.Common.Extender.LinqExtender;
using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Entity.Enums;
using HKSJ.WBVV.Repository.Base;
using HKSJ.WBVV.Repository.Interface;
namespace HKSJ.WBVV.Repository
{

    public class VideoPrereviewRepository : BaseRepository, IVideoPrereviewRepository
    {
        public IQueryable<VideoPrereview> GetEntityList()
        {
            return base.GetEntityList<VideoPrereview>();
        }

        public IQueryable<VideoPrereview> GetEntityList(OrderCondtion orderCondtion)
        {
            return base.GetEntityList<VideoPrereview>(orderCondtion);
        }

        public IQueryable<VideoPrereview> GetEntityList(IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<VideoPrereview>(orderCondtions);
        }

        public IQueryable<VideoPrereview> GetEntityList(Condtion condtion)
        {
            return base.GetEntityList<VideoPrereview>(condtion);
        }

        public IQueryable<VideoPrereview> GetEntityList(Condtion condtion, OrderCondtion orderCondtion)
        {
            return base.GetEntityList<VideoPrereview>(condtion, orderCondtion);
        }

        public IQueryable<VideoPrereview> GetEntityList(Condtion condtion, IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<VideoPrereview>(condtion, orderCondtions);
        }

        public IQueryable<VideoPrereview> GetEntityList(IList<Condtion> condtions)
        {
            return base.GetEntityList<VideoPrereview>(condtions);
        }

        public IQueryable<VideoPrereview> GetEntityList(IList<Condtion> condtions, OrderCondtion orderCondtion)
        {
            return base.GetEntityList<VideoPrereview>(condtions, orderCondtion);
        }

        public IQueryable<VideoPrereview> GetEntityList(IList<Condtion> condtions, IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<VideoPrereview>(condtions, orderCondtions);
        }
        public VideoPrereview GetEntity(Condtion condtion)
        {
            return base.GetEntity<VideoPrereview>(condtion);
        }

        public VideoPrereview GetEntity(IList<Condtion> condtions)
        {
            return base.GetEntity<VideoPrereview>(condtions);
        }
        public bool CreateEntity(VideoPrereview entity)
        {
            return base.CreateEntity<VideoPrereview>(entity);
        }

        public void CreateEntitys(IList<VideoPrereview> entitys)
        {
            base.CreateEntitys(entitys);
        }

        public bool UpdateEntity(VideoPrereview entity)
        {
            return base.UpdateEntity(entity);
        }

        public void UpdateEntitys(IList<VideoPrereview> entitys)
        {
            base.UpdateEntitys<VideoPrereview>(entitys);
        }

        public bool DeleteEntity(VideoPrereview entity)
        {
            return base.DeleteEntity<VideoPrereview>(entity);
        }

        public void DeleteEntitys(IList<VideoPrereview> entitys)
        {
            base.DeleteEntitys<VideoPrereview>(entitys);
        }

        #region ����
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="loginUserId">��¼�û����</param>
        /// <param name="videoId">��Ƶ���</param>
        /// <returns></returns>
        public bool Bad(int loginUserId,int videoId)
        {
            var success= Execute<bool>((db) =>
            {
                var user = db.User.FirstOrDefault(u => u.State == false && u.Id == loginUserId);
                AssertUtil.IsNotNull(user,"�û������ڻ��߱�����");
                var video = db.Video.FirstOrDefault(v => v.VideoSource & v.VideoState == 2 && v.Id == videoId);
                AssertUtil.IsNotNull(video, "��Ƶ�����ڻ���ת�벻�ɹ�");
                var videoPrereview = db.VideoPrereview.FirstOrDefault(v => v.State == false && v.UserId == user.Id && v.VideoId == video.Id && v.ReviewState == (int)VideoPrereviewEnum.Bad);
                AssertUtil.IsNull(videoPrereview, "���Ѿ�����������");
                var newVideoPrereview=new VideoPrereview()
                {
                    CreateTime =DateTime.Now,
                    CreateUserId = user.Id,
                    ReviewState = (int)VideoPrereviewEnum.Bad,
                    UserId = user.Id,
                    VideoId = (int)video.Id
                };
                db.VideoPrereview.Add(newVideoPrereview);
                return db.SaveChanges()>0;
            });
            if (success)
            {
                CreateCache<VideoPrereview>();
            }
            return success;
        }
        #endregion

        #region �ػ�
        /// <summary>
        /// �ػ�
        /// </summary>
        /// <param name="loginUserId">��¼�û����</param>
        /// <param name="videoId">��Ƶ���</param>
        /// <returns></returns>
        public bool Good(int loginUserId, int videoId)
        {
            var success = Execute<bool>((db) =>
            {
                var user = db.User.FirstOrDefault(u => u.State == false && u.Id == loginUserId);
                AssertUtil.IsNotNull(user, "�û������ڻ��߱�����");
                var video = db.Video.FirstOrDefault(v => v.VideoSource & v.VideoState == 2 && v.Id == videoId);
                AssertUtil.IsNotNull(video, "��Ƶ�����ڻ���ת�벻�ɹ�");
                var videoPrereview = db.VideoPrereview.FirstOrDefault(v => v.State == false && v.UserId == user.Id && v.VideoId == video.Id && v.ReviewState == (int)VideoPrereviewEnum.Good);
                AssertUtil.IsNull(videoPrereview, "���Ѿ������ػ���");
                var newVideoPrereview = new VideoPrereview()
                {
                    CreateTime = DateTime.Now,
                    CreateUserId = user.Id,
                    ReviewState = (int)VideoPrereviewEnum.Good,
                    UserId = user.Id,
                    VideoId = (int)video.Id
                };
                db.VideoPrereview.Add(newVideoPrereview);
                return db.SaveChanges() > 0;
            });
            if (success)
            {
                CreateCache<VideoPrereview>();
            }
            return success;
        }
        #endregion
    }
}