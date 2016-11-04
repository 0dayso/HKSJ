using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HKSJ.WBVV.Common.Extender.LinqExtender;
using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Repository.Base;
using HKSJ.WBVV.Repository.Interface;
using HKSJ.WBVV.Common.Assert;

namespace HKSJ.WBVV.Repository
{

    public class VideoRepository : BaseRepository, IVideoRepository
    {
        public IQueryable<Video> GetEntityList()
        {
            return base.GetEntityList<Video>();
        }

        public IQueryable<Video> GetEntityList(OrderCondtion orderCondtion)
        {
            return base.GetEntityList<Video>(orderCondtion);
        }

        public IQueryable<Video> GetEntityList(IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<Video>(orderCondtions);
        }

        public IQueryable<Video> GetEntityList(Condtion condtion)
        {
            return base.GetEntityList<Video>(condtion);
        }

        public IQueryable<Video> GetEntityList(Condtion condtion, OrderCondtion orderCondtion)
        {
            return base.GetEntityList<Video>(condtion, orderCondtion);
        }

        public IQueryable<Video> GetEntityList(Condtion condtion, IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<Video>(condtion, orderCondtions);
        }

        public IQueryable<Video> GetEntityList(IList<Condtion> condtions)
        {
            return base.GetEntityList<Video>(condtions);
        }

        public IQueryable<Video> GetEntityList(IList<Condtion> condtions, OrderCondtion orderCondtion)
        {
            return base.GetEntityList<Video>(condtions, orderCondtion);
        }

        public IQueryable<Video> GetEntityList(IList<Condtion> condtions, IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<Video>(condtions, orderCondtions);
        }
        public Video GetEntity(Condtion condtion)
        {
            return base.GetEntity<Video>(condtion);
        }

        public Video GetEntity(IList<Condtion> condtions)
        {
            return base.GetEntity<Video>(condtions);
        }
        public bool CreateEntity(Video entity)
        {
            return base.CreateEntity<Video>(entity);
        }

        public void CreateEntitys(IList<Video> entitys)
        {
            base.CreateEntitys<Video>(entitys);
        }

        public bool UpdateEntity(Video entity)
        {
            return base.UpdateEntity<Video>(entity);
        }

        public void UpdateEntitys(IList<Video> entitys)
        {
            base.UpdateEntitys<Video>(entitys);
        }

        public bool DeleteEntity(Video entity)
        {
            return base.DeleteEntity<Video>(entity);
        }

        public void DeleteEntitys(IList<Video> entitys)
        {
            base.DeleteEntitys<Video>(entitys);
        }

        #region �ۿ���Ƶ

        /// <summary>
        /// �ۿ���Ƶ
        /// </summary>
        /// <param name="loginUserId"></param>
        /// <param name="videoId"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public void IncomeWatch(int loginUserId, long videoId, string ipAddress)
        {
            AssertUtil.AreBigger(videoId, 0, "��Ƶ������");
            var success = Execute<bool>((db) =>
            {
                var video = db.Video.FirstOrDefault(v => v.State == false && v.VideoState == 3 && v.Id == videoId);
                AssertUtil.IsNotNull(video, "��Ƶ�����ڻ��߱�����");
                if (video.VideoSource)
                {
                    var uploadUser = db.User.FirstOrDefault(u => u.State == false && u.Id == video.CreateManageId);
                    AssertUtil.IsNotNull(uploadUser, "�ϴ��û������ڻ��߱�����");
                }
                else
                {
                    var manage = db.Manage.FirstOrDefault(m => m.State == false && m.Id == video.CreateManageId);
                    AssertUtil.IsNotNull(manage, "�ϴ�����Ա�����ڻ��߱�����");
                }
                var playRecord = new VideoPlayRecord()
                {
                    IpAddress = ipAddress,
                    UserId = 0, //0��ʾ������δ��¼
                    VideoId = videoId,
                    CreateTime = DateTime.Now
                };
                VideoPlayRecord videoPlayRecord;
                if (loginUserId > 0)
                {
                    var loginUser = db.User.FirstOrDefault(u => u.State == false && u.Id == loginUserId);
                    AssertUtil.IsNotNull(loginUser, "�û������ڻ��߱�����");
                    //��ѯ�û������һ�����ż�¼
                    videoPlayRecord = (from v in db.VideoPlayRecord
                                       where v.UserId == loginUser.Id && v.VideoId == video.Id
                                       orderby v.CreateTime descending
                                       select v
                        ).FirstOrDefault();
                    playRecord.UserId = loginUser.Id;
                }
                else
                {
                    AssertUtil.NotNullOrWhiteSpace(ipAddress, "Ip��ַδ����");
                    //��ѯδ��¼�û�IP�����һ�����ż�¼
                    videoPlayRecord = (from v in db.VideoPlayRecord
                                       where v.IpAddress == ipAddress && v.VideoId == video.Id
                                       orderby v.CreateTime descending
                                       select v
                        ).FirstOrDefault();
                }
                var flag = true;

                #region ��֤���첥��
                if (videoPlayRecord != null)
                {
                    string currentlogintime = DateTime.Now.ToString("yyyy-MM-dd");
                    string lastlogintime = videoPlayRecord.CreateTime.ToString("yyyy-MM-dd");
                    TimeSpan span = Convert.ToDateTime(currentlogintime) - Convert.ToDateTime(lastlogintime);
                    flag = span.TotalDays >= 1;
                }
                #endregion

                if (flag)
                {
                    video.PlayCount++;
                    db.Entry<Video>(video).State = EntityState.Modified;
                    db.VideoPlayRecord.Add(playRecord);
                    db.Configuration.ValidateOnSaveEnabled = false;
                    return db.SaveChanges() > 0;
                }
                return false;
            });
            if (success)
            {
                CreateCache<VideoPlayRecord>();
                CreateCache<Video>();
                CreateCache<User>();
            }
        }
        #endregion

        #region �ϴ���Ƶ

        /// <summary>
        /// �ϴ���Ƶ����˺�
        /// </summary>
        /// <param name="approveContent"></param>
        /// <param name="approveRemark"></param>
        /// <param name="videoId"></param>
        /// <param name="status"></param>
        /// <param name="createAdminId"></param>
        /// <returns></returns>
        public void IncomeUpload(string approveContent, string approveRemark, long videoId, bool status, int createAdminId)
        {
            AssertUtil.NotNullOrWhiteSpace(approveContent, "������ݲ���Ϊ��");
            AssertUtil.AreBigger(videoId, 0, "��˵���Ƶ������");
            AssertUtil.AreBigger(createAdminId, 0, "����Աδ��¼");
            var success = Execute<bool>((db) =>
            {
                var manage = db.Manage.FirstOrDefault(m => m.State == false && m.Id == createAdminId);
                AssertUtil.IsNotNull(manage, "����Ա�����ڻ��߱�����");
                var video = db.Video.FirstOrDefault(v => v.State == false && v.Id == videoId && v.VideoState > 1);
                AssertUtil.IsNotNull(video, "��Ƶ�����ڻ���ת�벻�ɹ�");
                //if (status)
                //{
                //    //AssertUtil.IsFalse(video.VideoState == 4, "��Ƶ�Ѿ�����˲�ͨ��״̬");
                //}
                //else
                //{
                //    //AssertUtil.IsFalse(video.VideoState == 3, "��Ƶ�Ѿ������ͨ��״̬");
                //}
                var videoApprove = db.VideoApprove.FirstOrDefault(v => v.VideoId == video.Id);
                if (videoApprove == null)
                {
                    var newVideoApprove = new VideoApprove()
                    {
                        VideoId = video.Id,
                        ApproveContent = approveContent,
                        ApproveRemark = approveRemark,
                        ApproveId = manage.Id,
                        CreateTime = DateTime.Now,
                        Status = status,
                        CreateAdminId = manage.Id
                    };
                    db.VideoApprove.Add(newVideoApprove);
                }
                else
                {
                    videoApprove.ApproveContent = approveContent;
                    videoApprove.ApproveRemark = approveRemark;
                    videoApprove.UpdateTime = DateTime.Now;
                    videoApprove.UpdateAdminId = manage.Id;
                    videoApprove.Status = status;
                    db.Entry<VideoApprove>(videoApprove).State = EntityState.Modified;
                }
                video.VideoState = (short)(status ? 4 : 3);
                db.Entry<Video>(video).State = EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;
                return db.SaveChanges() > 0;
            });
            if (success)
            {
                CreateCache<Video>();
                CreateCache<VideoApprove>();
                CreateCache<User>();
            }
        }
        #endregion

        #region ������Ƶ���㲥����GuoShisheng�������߻�ò���(�ϴ��߻�ò����ɡ������¼����ۿ���Ƶ����������)
        public void IncomeShare(int videoId, int demandUserId, string ipAddress, int shareUserId, string shareUserIp)
        {

            var success = Execute<bool>((db) =>
            {
                AssertUtil.AreBigger(videoId, 0, "��Ƶ������");
                var video = db.Video.FirstOrDefault(v => v.State == false && v.VideoState == 3 && v.Id == videoId);
                AssertUtil.IsNotNull(video, "��Ƶ�����ڻ��߱�����");
                if (shareUserId > 0)
                {
                    var shareUser = db.User.FirstOrDefault(u => u.State == false && u.Id == shareUserId);
                    if (shareUser != null)
                    {
                        if (demandUserId > 0)
                        {
                            var user = db.User.FirstOrDefault(u => u.State == false && u.Id == demandUserId);
                            if (user != null)
                            {
                                var usershare =
                                    db.UserShare.FirstOrDefault(
                                        us =>
                                            us.UserId == user.Id && us.VideoId == video.Id && us.CreateUserId == shareUserId);
                                if (usershare != null)
                                {
                                    usershare.UpdateTime = DateTime.Now;
                                    usershare.UpdateUserId = user.Id;
                                    usershare.IpAddress = ipAddress;
                                    db.Entry(usershare).State = EntityState.Modified;
                                }
                                else
                                {
                                    var newusershare = new UserShare()
                                    {
                                        IpAddress = ipAddress,
                                        UserId = demandUserId,
                                        VideoId = videoId,
                                        WatchTime = 1,
                                        CreateTime = DateTime.Now,
                                        CreateUserId = shareUserId,
                                        UpdateTime = DateTime.Now,
                                        UpdateUserId = shareUserId
                                    };
                                    db.UserShare.Add(newusershare);
                                }
                                return db.SaveChanges() > 0;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(ipAddress))
                            {
                                var usershare =
                                    db.UserShare.FirstOrDefault(
                                        us =>
                                            us.IpAddress == ipAddress && us.VideoId == video.Id &&
                                            us.CreateUserId == shareUserId);
                                if (usershare != null)
                                {
                                    usershare.UpdateTime = DateTime.Now;
                                    usershare.IpAddress = ipAddress;
                                    db.Entry(usershare).State = EntityState.Modified;
                                }
                                else
                                {
                                    var newusershare = new UserShare()
                                    {
                                        IpAddress = ipAddress,
                                        UserId = demandUserId,
                                        VideoId = videoId,
                                        WatchTime = 1,
                                        CreateTime = DateTime.Now,
                                        CreateUserId = shareUserId,
                                        UpdateTime = DateTime.Now,
                                        UpdateUserId = shareUserId
                                    };
                                    db.UserShare.Add(newusershare);
                                }
                                return db.SaveChanges() > 0;
                            }
                        }
                    }
                }
                return true;
            });
            if (success)
            {
                CreateCache<UserShare>();
            }
        }
        #endregion
    }
}