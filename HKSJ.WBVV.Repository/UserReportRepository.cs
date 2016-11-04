



using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HKSJ.WBVV.Common.Assert;
using HKSJ.WBVV.Common.Extender.LinqExtender;
using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Entity.Enums;
using HKSJ.WBVV.Repository.Base;
using HKSJ.WBVV.Repository.Interface;
namespace HKSJ.WBVV.Repository
{

    public class UserReportRepository : BaseRepository, IUserReportRepository
    {
        public IQueryable<UserReport> GetEntityList()
        {
            return base.GetEntityList<UserReport>();
        }

        public IQueryable<UserReport> GetEntityList(OrderCondtion orderCondtion)
        {
            return base.GetEntityList<UserReport>(orderCondtion);
        }

        public IQueryable<UserReport> GetEntityList(IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<UserReport>(orderCondtions);
        }

        public IQueryable<UserReport> GetEntityList(Condtion condtion)
        {
            return base.GetEntityList<UserReport>(condtion);
        }

        public IQueryable<UserReport> GetEntityList(Condtion condtion, OrderCondtion orderCondtion)
        {
            return base.GetEntityList<UserReport>(condtion, orderCondtion);
        }

        public IQueryable<UserReport> GetEntityList(Condtion condtion, IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<UserReport>(condtion, orderCondtions);
        }

        public IQueryable<UserReport> GetEntityList(IList<Condtion> condtions)
        {
            return base.GetEntityList<UserReport>(condtions);
        }

        public IQueryable<UserReport> GetEntityList(IList<Condtion> condtions, OrderCondtion orderCondtion)
        {
            return base.GetEntityList<UserReport>(condtions, orderCondtion);
        }

        public IQueryable<UserReport> GetEntityList(IList<Condtion> condtions, IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<UserReport>(condtions, orderCondtions);
        }
        public UserReport GetEntity(Condtion condtion)
        {
            return base.GetEntity<UserReport>(condtion);
        }

        public UserReport GetEntity(IList<Condtion> condtions)
        {
            return base.GetEntity<UserReport>(condtions);
        }
        public bool CreateEntity(UserReport entity)
        {
            return base.CreateEntity<UserReport>(entity);
        }

        public void CreateEntitys(IList<UserReport> entitys)
        {
            base.CreateEntitys(entitys);
        }

        public bool UpdateEntity(UserReport entity)
        {
            return base.UpdateEntity(entity);
        }

        public void UpdateEntitys(IList<UserReport> entitys)
        {
            base.UpdateEntitys<UserReport>(entitys);
        }

        public bool DeleteEntity(UserReport entity)
        {
            return base.DeleteEntity<UserReport>(entity);
        }

        public void DeleteEntitys(IList<UserReport> entitys)
        {
            base.DeleteEntitys<UserReport>(entitys);
        }

        #region �ٱ���Ƶ
        /// <summary>
        /// �ٱ���Ƶ
        /// </summary>
        /// <param name="loginUserId">��¼�û����</param>
        /// <param name="videoId">��Ƶ���</param>
        /// <returns></returns>
        public bool ReportVideo(int loginUserId, int videoId)
        {
            var success = Execute<bool>((db) =>
            {
                var user = db.User.FirstOrDefault(u => u.State == false && u.Id == loginUserId);
                AssertUtil.IsNotNull(user, "�û������ڻ��߱�����");
                var video = db.Video.FirstOrDefault(v => v.VideoSource & (v.VideoState == 3 || v.VideoState==2) && v.Id == videoId);
                AssertUtil.IsNotNull(video, "��Ƶ�����ڻ�����˲�ͨ��");
                var userReport = db.UserReport.FirstOrDefault(v => v.State == false && v.UserId == user.Id && v.EntityId == video.Id&& v.EntityType == (int)UserReportEnum.Video);
                AssertUtil.IsNull(userReport, "���Ѿ��ٱ�������Ƶ");
                var newUserReport = new UserReport()
                {
                    CreateTime = DateTime.Now,
                    CreateUserId = user.Id,
                    EntityType = (int)UserReportEnum.Video,
                    UserId = user.Id,
                    EntityId = (int)video.Id
                };
                db.UserReport.Add(newUserReport);
                video.ReportCount++;
                db.Entry(video).State= EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;
                return db.SaveChanges() > 0;
            });
            if (success)
            {
                CreateCache<UserReport>();
                CreateCache<Video>();
            }
            return success;
        }
        #endregion

        #region �ٱ�����
        /// <summary>
        /// �ٱ�����
        /// </summary>
        /// <param name="loginUserId">��¼�û����</param>
        /// <param name="commentId">���۱��</param>
        /// <returns></returns>
        public bool ReportComment(int loginUserId, int commentId)
        {
            var success = Execute<bool>((db) =>
            {
                var user = db.User.FirstOrDefault(u => u.State == false && u.Id == loginUserId);
                AssertUtil.IsNotNull(user, "�û������ڻ��߱�����");
                var comment = db.Comments.FirstOrDefault(c => c.State >= 0 & c.Id == commentId);
                AssertUtil.IsNotNull(comment, "���۲����ڻ�����˲�ͨ��");
                var userReport = db.UserReport.FirstOrDefault(v => v.State == false && v.UserId == user.Id && v.EntityId == comment.Id && v.EntityType == (int)UserReportEnum.Comment);
                AssertUtil.IsNull(userReport, "���Ѿ��ٱ���������");
                var newUserReport = new UserReport()
                {
                    CreateTime = DateTime.Now,
                    CreateUserId = user.Id,
                    EntityType = (int)UserReportEnum.Comment,
                    UserId = user.Id,
                    EntityId = comment.Id
                };
                db.UserReport.Add(newUserReport);
                comment.ReportCount++;
                db.Entry(comment).State = EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;
                return db.SaveChanges() > 0;
            });
            if (success)
            {
                CreateCache<UserReport>();
                CreateCache<Comments>();
            }
            return success;
        }
        #endregion
    }
}