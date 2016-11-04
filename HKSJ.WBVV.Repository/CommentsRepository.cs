


using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using HKSJ.WBVV.Common.Extender.LinqExtender;
using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Repository.Base;
using HKSJ.WBVV.Repository.Interface;
using HKSJ.WBVV.Common.Assert;
using HKSJ.WBVV.Entity.Enums;
using HKSJ.WBVV.Common.Language;

namespace HKSJ.WBVV.Repository
{

    public class CommentsRepository : BaseRepository, ICommentsRepository
    {
        public IQueryable<Comments> GetEntityList()
        {
            return base.GetEntityList<Comments>();
        }

        public IQueryable<Comments> GetEntityList(OrderCondtion orderCondtion)
        {
            return base.GetEntityList<Comments>(orderCondtion);
        }

        public IQueryable<Comments> GetEntityList(IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<Comments>(orderCondtions);
        }

        public IQueryable<Comments> GetEntityList(Condtion condtion)
        {
            return base.GetEntityList<Comments>(condtion);
        }

        public IQueryable<Comments> GetEntityList(Condtion condtion, OrderCondtion orderCondtion)
        {
            return base.GetEntityList<Comments>(condtion, orderCondtion);
        }

        public IQueryable<Comments> GetEntityList(Condtion condtion, IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<Comments>(condtion, orderCondtions);
        }

        public IQueryable<Comments> GetEntityList(IList<Condtion> condtions)
        {
            return base.GetEntityList<Comments>(condtions);
        }

        public IQueryable<Comments> GetEntityList(IList<Condtion> condtions, OrderCondtion orderCondtion)
        {
            return base.GetEntityList<Comments>(condtions, orderCondtion);
        }

        public IQueryable<Comments> GetEntityList(IList<Condtion> condtions, IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<Comments>(condtions, orderCondtions);
        }
        public Comments GetEntity(Condtion condtion)
        {
            return base.GetEntity<Comments>(condtion);
        }

        public Comments GetEntity(IList<Condtion> condtions)
        {
            return base.GetEntity<Comments>(condtions);
        }
        public bool CreateEntity(Comments entity)
        {
            return base.CreateEntity<Comments>(entity);
        }

        public void CreateEntitys(IList<Comments> entitys)
        {
            base.CreateEntitys<Comments>(entitys);
        }

        public bool UpdateEntity(Comments entity)
        {
            return base.UpdateEntity<Comments>(entity);
        }

        public void UpdateEntitys(IList<Comments> entitys)
        {
            base.UpdateEntitys<Comments>(entitys);
        }

        public bool DeleteEntity(Comments entity)
        {
            return base.DeleteEntity<Comments>(entity);
        }

        public void DeleteEntitys(IList<Comments> entitys)
        {
            base.DeleteEntitys<Comments>(entitys);
        }

        #region ��ȡLocalPath

        /// <summary>
        /// ��ȡ���۵�LocalPath,��󳤶�Ϊ9λ
        /// </summary>
        /// <param name="prevLocalPath">�ϴ�λ��</param>
        /// <param name="commentId">��ǰ���۱��</param>
        /// <returns></returns>
        protected string GetLocalPath(string prevLocalPath, int commentId)
        {
            var currLocalPath = "";
            if (commentId < 10)
            {
                currLocalPath = "00000000" + commentId;
            }
            else if (commentId >= 10 && commentId < 100)
            {
                currLocalPath = "0000000" + commentId;
            }
            else if (commentId >= 100 & commentId < 1000)
            {
                currLocalPath = "000000" + commentId;
            }
            else if (commentId >= 1000 & commentId < 10000)
            {
                currLocalPath = "00000" + commentId;
            }
            else if (commentId >= 10000 & commentId < 100000)
            {
                currLocalPath = "0000" + commentId;
            }
            else if (commentId >= 100000 & commentId < 1000000)
            {
                currLocalPath = "000" + commentId;
            }
            else if (commentId >= 1000000 & commentId < 10000000)
            {
                currLocalPath = "00" + commentId;
            }
            else if (commentId >= 10000000 & commentId < 100000000)
            {
                currLocalPath = "0" + commentId;
            }
            else
            {
                currLocalPath = commentId.ToString();
            }
            if (string.IsNullOrEmpty(prevLocalPath))
            {
                return currLocalPath;
            }
            else
            {
                return prevLocalPath + "," + currLocalPath;
            }
        }
        #endregion

        #region ��Ƶ
        /// <summary>
        /// ������Ƶ����
        /// </summary>
        /// <param name="loginUserId"></param>
        /// <param name="commentContent"></param>
        /// <param name="videoId"></param>
        /// <returns></returns>
        public int CreateVideoComment(int loginUserId, int videoId, string commentContent)
        {
            var success = false;

            #region ��֤�����Ƿ���ȷ
            AssertUtil.AreBigger(loginUserId, 0, LanguageUtil.Translate("api_Repository_Comments_CreateVideoComment_loginUserId"));
            AssertUtil.AreBigger(videoId, 0, LanguageUtil.Translate("api_Repository_Comments_CreateVideoComment_videoId"));
            AssertUtil.NotNullOrWhiteSpace(commentContent, LanguageUtil.Translate("api_Repository_Comments_CreateVideoComment_commentContent"));
            AssertUtil.AreSmallerOrEqual(commentContent.Length, 140, LanguageUtil.Translate("api_Repository_Comments_CreateVideoComment_commentContentLength"));
            #endregion

            var commentId = Execute<int>((db) =>
            {
                #region ��֤���ݿ��Ƿ����
                var loginUser = db.User.FirstOrDefault(u => u.State == false && u.Id == loginUserId);
                AssertUtil.IsNotNull(loginUser, LanguageUtil.Translate("api_Repository_Comments_CreateVideoComment_loginUser"));
                var video = db.Video.FirstOrDefault(v => v.State == false && v.Id == videoId);
                AssertUtil.IsNotNull(video, LanguageUtil.Translate("api_Repository_Comments_CreateVideoComment_video"));
                #endregion

                var comment = new Comments()
                {
                    FromUserId = loginUser.Id,
                    ToUserId = 0,
                    EntityType = (int)CommentEnum.Video,
                    EntityId = (int)video.Id,
                    Content = commentContent,
                    CreateUserId = loginUser.Id,
                    CreateTime = DateTime.Now,
                    LocalPath = ""//������Ƶ���۵�λ��Ϊ���""
                };
                db.Comments.Add(comment);
                //��Ƶ���۴�����һ
                video.CommentCount = video.CommentCount + 1;
                db.Entry<Video>(video).State = EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;
                success = db.SaveChanges() > 0;
                if (success)
                {
                    comment.LocalPath = GetLocalPath(comment.LocalPath, comment.Id);
                    db.Entry<Comments>(comment).State = EntityState.Modified;
                    //�������۵��˲����ϴ���
                    if (video.CreateManageId != loginUser.Id)
                    {
                        //�����Ϣ֪ͨ
                        var sysMessage = new SysMessage()
                        {
                            Content = comment.Content,
                            CreateManageId = 1,
                            CreateTime = DateTime.Now,
                            EntityId = comment.Id,
                            EntityType = (int)SysMessageEnum.VideoComment,
                            SendType = (int)SystemMessageEnum.ByUser,
                            ToUserIds = video.CreateManageId + ""//֪ͨ�ϴ���
                        };
                        db.SysMessage.Add(sysMessage);
                    }
                    db.Configuration.ValidateOnSaveEnabled = false;
                    success = db.SaveChanges() > 0;
                }
                return comment.Id;
            });
            if (success)
            {
                CreateCache<Video>();
                CreateCache<Comments>();
                CreateCache<SysMessage>();
            }
            return commentId;
        }
        /// <summary>
        /// �ظ���Ƶ����
        /// </summary>
        /// <param name="loginUserId"></param>
        /// <param name="commentContent"></param>
        /// <param name="videoId"></param>
        /// <param name="commentId"></param>
        /// <returns></returns>
        public int ReplyVideoComment(int loginUserId, int videoId, int commentId, string commentContent)
        {
            var success = false;

            #region ��֤�����Ƿ���ȷ
            AssertUtil.AreBigger(loginUserId, 0, LanguageUtil.Translate("api_Repository_Comments_ReplyVideoComment_loginUserId"));
            AssertUtil.AreBigger(videoId, 0, LanguageUtil.Translate("api_Repository_Comments_ReplyVideoComment_videoId"));
            AssertUtil.AreBigger(commentId, 0, LanguageUtil.Translate("api_Repository_Comments_ReplyVideoComment_commentId"));
            AssertUtil.NotNullOrWhiteSpace(commentContent, LanguageUtil.Translate("api_Repository_Comments_ReplyVideoComment_commentContent"));
            AssertUtil.AreSmallerOrEqual(commentContent.Length, 140, LanguageUtil.Translate("api_Repository_Comments_ReplyVideoComment_commentContentLength"));
            #endregion

            var id = Execute<int>((db) =>
            {
                #region ��֤���ݿ��Ƿ����
                var loginUser = db.User.FirstOrDefault(u => u.State == false && u.Id == loginUserId);
                AssertUtil.IsNotNull(loginUser, LanguageUtil.Translate("api_Repository_Comments_ReplyVideoComment_loginUser"));
                var video = db.Video.FirstOrDefault(v => v.State == false && v.Id == videoId);
                AssertUtil.IsNotNull(video, LanguageUtil.Translate("api_Repository_Comments_ReplyVideoComment_video"));
                var parentComment = db.Comments.FirstOrDefault(c => c.State >= 0
                                                             && c.Id == commentId
                                                             && c.EntityType == (int)CommentEnum.Video
                                                             && c.EntityId == video.Id);
                AssertUtil.IsNotNull(parentComment, LanguageUtil.Translate("api_Repository_Comments_ReplyVideoComment_parentComment"));
                #endregion

                AssertUtil.AreNotEqual(loginUser.Id, parentComment.FromUserId, LanguageUtil.Translate("api_Repository_Comments_ReplyVideoComment_notReplySelf"));
                var comment = new Comments()
                {
                    FromUserId = loginUser.Id,
                    ToUserId = parentComment.FromUserId,
                    EntityType = (int)CommentEnum.Video,
                    EntityId = (int)video.Id,
                    Content = commentContent,
                    CreateUserId = loginUser.Id,
                    CreateTime = DateTime.Now,
                    ParentId = parentComment.Id,
                    LocalPath = GetLocalPath(parentComment.LocalPath, parentComment.Id)
                };
                db.Comments.Add(comment);
                //�ظ�������һ
                parentComment.ReplyNum++;
                db.Entry<Comments>(parentComment).State = EntityState.Modified;
                video.CommentCount = video.CommentCount + 1;
                db.Entry<Video>(video).State = EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;
                success = db.SaveChanges() > 0;
                if (success)
                {
                    comment.LocalPath = GetLocalPath(parentComment.LocalPath, comment.Id);
                    db.Entry<Comments>(comment).State = EntityState.Modified;
                    //�����Ϣ֪ͨ
                    var sysMessage = new SysMessage()
                    {
                        Content = comment.Content,
                        CreateManageId = 1,
                        CreateTime = DateTime.Now,
                        EntityId = comment.Id,
                        EntityType = (int) SysMessageEnum.VideoComment,
                        SendType = (int) SystemMessageEnum.ByUser
                    };
                    //�ظ����˲����ϴ���
                    if (video.CreateManageId != loginUser.Id && parentComment.FromUserId != video.CreateManageId)
                    {
                        sysMessage.ToUserIds = video.CreateManageId + "|" + parentComment.FromUserId; //֪ͨ�ϴ��ߺͽ�����,�ϴ������Լ���Ƶ�»ظ���֪ͨ
                    }
                    else
                    {
                        sysMessage.ToUserIds = parentComment.FromUserId + ""; //֪ͨ������
                    }
                    db.SysMessage.Add(sysMessage);
                    db.Configuration.ValidateOnSaveEnabled = false;
                    success = db.SaveChanges() > 0;
                }
                return comment.Id;
            });
            if (success)
            {
                CreateCache<Video>();
                CreateCache<Comments>();
                CreateCache<SysMessage>();
            }
            return id;
        }
        /// <summary>
        /// ɾ����Ƶ����
        /// </summary>
        /// <param name="loginUserId"></param>
        /// <param name="videoId"></param>
        /// <param name="commentId"></param>
        /// <returns></returns>
        public bool DeleteVideoComment(int loginUserId, int videoId, int commentId)
        {

            #region ��֤�����Ƿ���ȷ
            AssertUtil.AreBigger(loginUserId, 0, LanguageUtil.Translate("api_Repository_Comments_DeleteVideoComment_loginUserId"));
            AssertUtil.AreBigger(videoId, 0, LanguageUtil.Translate("api_Repository_Comments_DeleteVideoComment_videoId"));
            AssertUtil.AreBigger(commentId, 0, LanguageUtil.Translate("api_Repository_Comments_DeleteVideoComment_commentId"));
            #endregion

            var success = Execute<bool>((db) =>
            {
                #region ��֤���ݿ��Ƿ����
                var loginUser = db.User.FirstOrDefault(u => u.State == false && u.Id == loginUserId);
                AssertUtil.IsNotNull(loginUser, LanguageUtil.Translate("api_Repository_Comments_DeleteVideoComment_loginUser"));
                var video = db.Video.FirstOrDefault(v => v.State == false && v.Id == videoId);
                AssertUtil.IsNotNull(video, LanguageUtil.Translate("api_Repository_Comments_DeleteVideoComment_video"));
                var comment = db.Comments.FirstOrDefault(c => c.State >= 0
                                                             && c.Id == commentId
                                                             && c.EntityType == (int)CommentEnum.Video
                                                             && c.EntityId == video.Id);
                AssertUtil.IsNotNull(comment, LanguageUtil.Translate("api_Repository_Comments_DeleteVideoComment_comment"));
                #endregion

                AssertUtil.AreEqual(loginUser.Id, video.CreateManageId, LanguageUtil.Translate("api_Repository_Comments_DeleteVideoComment_videoUpdateDelete"));

                var childCommnets = db.Comments.Where(c => c.State >= (int)CommentStateEnum.Waiting && c.EntityType == (int)CommentEnum.Video && c.LocalPath.StartsWith(comment.LocalPath));
                foreach (var childCommnet in childCommnets)
                {
                    db.Entry<Comments>(childCommnet).State = EntityState.Deleted;
                }
                db.Configuration.ValidateOnSaveEnabled = false;
                return db.SaveChanges() > 0;
            });
            if (success)
            {
                CreateCache<Comments>();
            }
            return success;
        }

        public bool DeleteVideoComment(int loginUserId, int commentId)
        {
            #region ��֤�����Ƿ���ȷ
            AssertUtil.AreBigger(loginUserId, 0, "�û�δ��¼");
            AssertUtil.AreBigger(commentId, 0, "��Ƶ���۲�����");
            #endregion

            var success = Execute<bool>((db) =>
            {
                #region ��֤���ݿ��Ƿ����
                var loginUser = db.User.FirstOrDefault(u => u.State == false && u.Id == loginUserId);
                AssertUtil.IsNotNull(loginUser, "�û������ڻ��߱�����");
                var comment = db.Comments.FirstOrDefault(c => c.State >= (int)CommentStateEnum.Waiting
                                                             && c.Id == commentId
                                                             && c.EntityType == (int)CommentEnum.Video);
                AssertUtil.IsNotNull(comment, "��Ƶ���۱�ɾ��������˲�ͨ��");
                var video = db.Video.FirstOrDefault(v => v.State == false && v.Id == comment.EntityId);
                AssertUtil.IsNotNull(video, "���۵���Ƶ�����ڻ��߱�ɾ��");
                #endregion

                AssertUtil.AreEqual(loginUser.Id, video.CreateManageId, "ֻ����Ƶ�ϴ��߲���ɾ������");

                var childCommnets =db.Comments.Where(c => c.State >= (int)CommentStateEnum.Waiting && c.EntityType == (int)CommentEnum.Video &&c.LocalPath.StartsWith(comment.LocalPath));
                foreach (var childCommnet in childCommnets)
                {
                    db.Entry<Comments>(childCommnet).State = EntityState.Deleted;
                }
                db.Configuration.ValidateOnSaveEnabled = false;
                return db.SaveChanges() > 0;
            });
            if (success)
            {
                CreateCache<Comments>();
            }
            return success;
        }

        #endregion

        #region �û��ռ�

        /// <summary>
        /// �û��ռ�����
        /// </summary>
        /// <param name="loginUserId">��¼�û����</param>
        /// <param name="ownerUserId">���Կռ��û����</param>
        /// <param name="commentContent">��������</param>
        /// <returns></returns>
        public int CreateSpaceComment(int loginUserId, int ownerUserId, string commentContent)
        {
            var success = false;

            #region ��֤����
            AssertUtil.AreBigger(loginUserId, 0, LanguageUtil.Translate("api_Repository_Comments_CreateSpaceComment_loginUserId"));
            AssertUtil.AreBigger(ownerUserId, 0, LanguageUtil.Translate("api_Repository_Comments_CreateSpaceComment_ownerUserId"));
            AssertUtil.NotNullOrWhiteSpace(commentContent, LanguageUtil.Translate("api_Repository_Comments_CreateSpaceComment_commentContent"));
            AssertUtil.AreSmallerOrEqual(commentContent.Length, 140, LanguageUtil.Translate("api_Repository_Comments_CreateSpaceComment_commentContentLength"));
            #endregion

            var id = Execute<int>((db) =>
            {
                #region ��֤���ݿ��Ƿ����
                var loginUser = db.User.FirstOrDefault(u => u.State == false && u.Id == loginUserId);
                AssertUtil.IsNotNull(loginUser, LanguageUtil.Translate("api_Repository_Comments_CreateSpaceComment_loginUser"));
                var toUser = db.User.FirstOrDefault(u => u.State == false && u.Id == ownerUserId);
                AssertUtil.IsNotNull(toUser, LanguageUtil.Translate("api_Repository_Comments_CreateSpaceComment_toUser"));
                #endregion

                var comment = new Comments()
                {
                    FromUserId = loginUser.Id,
                    ToUserId = toUser.Id,
                    EntityType = (int)CommentEnum.User,
                    EntityId = (int)toUser.Id,
                    Content = commentContent,
                    CreateUserId = loginUser.Id,
                    CreateTime = DateTime.Now,
                    LocalPath = ""//������Ƶ���۵�λ��Ϊ���""
                };
                db.Comments.Add(comment);
                success = db.SaveChanges() > 0;
                if (success)
                {
                    comment.LocalPath = GetLocalPath(comment.LocalPath, comment.Id);
                    db.Entry<Comments>(comment).State = EntityState.Modified;
                    //�����û������Լ�
                    if (toUser.Id != loginUser.Id)
                    {
                        //�����Ϣ֪ͨ
                        var sysMessage = new SysMessage()
                        {
                            Content = comment.Content,
                            CreateManageId = 1,
                            CreateTime = DateTime.Now,
                            EntityId = comment.Id,
                            EntityType = (int)SysMessageEnum.SpaceComment,
                            SendType = (int)SystemMessageEnum.ByUser,
                            ToUserIds = toUser.Id + "" //֪ͨ�ռ�������
                        };
                        db.SysMessage.Add(sysMessage);
                    }
                    db.Configuration.ValidateOnSaveEnabled = false;
                    success = db.SaveChanges() > 0;
                }
                return comment.Id;
            });
            if (success)
            {
                CreateCache<Comments>();
                CreateCache<SysMessage>();
            }
            return id;
        }

        /// <summary>
        /// �ظ��û�����
        /// </summary>
        /// <param name="loginUserId">��¼�û����</param>
        /// <param name="commentContent">��������</param>
        /// <param name="ownerUserId">���Ե��û��ռ��û����</param>
        /// <param name="commentId">���Ա��</param>
        /// <returns></returns>
        public int ReplySpaceComment(int loginUserId, int ownerUserId, int commentId, string commentContent)
        {
            var success = false;

            #region ��֤����
            AssertUtil.AreBigger(loginUserId, 0, LanguageUtil.Translate("api_Repository_Comments_ReplySpaceComment_loginUserId"));
            AssertUtil.AreBigger(ownerUserId, 0, LanguageUtil.Translate("api_Repository_Comments_ReplySpaceComment_ownerUserId"));
            AssertUtil.AreBigger(commentId, 0, LanguageUtil.Translate("api_Repository_Comments_ReplySpaceComment_commentId"));
            AssertUtil.NotNullOrWhiteSpace(commentContent, LanguageUtil.Translate("api_Repository_Comments_ReplySpaceComment_commentContent"));
            AssertUtil.AreSmallerOrEqual(commentContent.Length, 140, LanguageUtil.Translate("api_Repository_Comments_ReplySpaceComment_commentContentLength"));
            #endregion

            var id = Execute<int>((db) =>
            {
                #region ��֤���ݿ��Ƿ����
                var loginUser = db.User.FirstOrDefault(u => u.State == false && u.Id == loginUserId);
                AssertUtil.IsNotNull(loginUser, LanguageUtil.Translate("api_Repository_Comments_ReplySpaceComment_loginUser"));
                var toUser = db.User.FirstOrDefault(u => u.State == false && u.Id == ownerUserId);
                AssertUtil.IsNotNull(toUser, LanguageUtil.Translate("api_Repository_Comments_ReplySpaceComment_toUser"));
                var parentComment = db.Comments.FirstOrDefault(c => c.State >= 0
                                                               && c.Id == commentId
                                                               && c.EntityType == (int)CommentEnum.User
                                                               && c.EntityId == toUser.Id);
                AssertUtil.IsNotNull(parentComment, LanguageUtil.Translate("api_Repository_Comments_ReplySpaceComment_parentComment"));
                #endregion

                AssertUtil.AreNotEqual(loginUser.Id, parentComment.FromUserId, LanguageUtil.Translate("api_Repository_Comments_ReplySpaceComment_notReplySelf"));
                var comment = new Comments()
                {
                    FromUserId = loginUser.Id,
                    ToUserId = parentComment.FromUserId,
                    EntityType = (int)CommentEnum.User,
                    EntityId = toUser.Id,
                    Content = commentContent,
                    CreateUserId = loginUser.Id,
                    CreateTime = DateTime.Now,
                    ParentId = parentComment.Id,
                    LocalPath = GetLocalPath(parentComment.LocalPath, parentComment.Id)
                };
                db.Comments.Add(comment);
                parentComment.ReplyNum = parentComment.ReplyNum + 1;
                db.Entry(parentComment).State = EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;
                success = db.SaveChanges() > 0;
                if (success)
                {
                    comment.LocalPath = GetLocalPath(parentComment.LocalPath, comment.Id);
                    db.Entry<Comments>(comment).State = EntityState.Modified;
                    //�����Ϣ֪ͨ
                    var sysMessage = new SysMessage()
                    {
                        Content = comment.Content,
                        CreateManageId = 1,
                        CreateTime = DateTime.Now,
                        EntityId = comment.Id,
                        EntityType = (int)SysMessageEnum.SpaceComment,
                        SendType = (int)SystemMessageEnum.ByUser
                    };
                    //�ظ����Բ��ǿռ�������
                    if (toUser.Id != loginUser.Id && parentComment.FromUserId != toUser.Id)
                    {
                        sysMessage.ToUserIds = toUser.Id + "|" + parentComment.FromUserId;//֪ͨ�ռ������ߺͽ�����
                    }
                    else
                    {
                        sysMessage.ToUserIds = parentComment.FromUserId + "";////֪ͨ������
                    }
                    db.SysMessage.Add(sysMessage);
                    db.Configuration.ValidateOnSaveEnabled = false;
                    success = db.SaveChanges() > 0;
                }
                return comment.Id;
            });
            if (success)
            {
                CreateCache<Comments>();
                CreateCache<SysMessage>();
            }
            return id;
        }

        /// <summary>
        /// ɾ���ռ�����
        /// </summary>
        /// <param name="loginUserId">��¼�û����</param>
        /// <param name="ownerUserId">���Կռ��û����</param>
        /// <param name="commentId">��������</param>
        /// <returns></returns>
        public bool DeleteSpaceComment(int loginUserId, int ownerUserId, int commentId)
        {

            #region ��֤�����Ƿ���ȷ
            AssertUtil.AreBigger(loginUserId, 0, LanguageUtil.Translate("api_Repository_Comments_DeleteSpaceComment_loginUserId"));
            AssertUtil.AreBigger(ownerUserId, 0, LanguageUtil.Translate("api_Repository_Comments_DeleteSpaceComment_ownerUserId"));
            AssertUtil.AreBigger(commentId, 0, LanguageUtil.Translate("api_Repository_Comments_DeleteSpaceComment_commentId"));
            #endregion

            var success = Execute<bool>((db) =>
            {
                #region ��֤���ݿ��Ƿ����
                var loginUser = db.User.FirstOrDefault(u => u.State == false && u.Id == loginUserId);
                AssertUtil.IsNotNull(loginUser, LanguageUtil.Translate("api_Repository_Comments_DeleteSpaceComment_loginUser"));
                var toUser = db.User.FirstOrDefault(u => u.State == false && u.Id == ownerUserId);
                AssertUtil.IsNotNull(toUser, LanguageUtil.Translate("api_Repository_Comments_DeleteSpaceComment_toUser"));
                var comment = db.Comments.FirstOrDefault(c => c.State >= 0
                                                               && c.Id == commentId
                                                               && c.EntityType == (int)CommentEnum.User
                                                               && c.EntityId == toUser.Id);
                AssertUtil.IsNotNull(comment, LanguageUtil.Translate("api_Repository_Comments_DeleteSpaceComment_comment"));
                #endregion

                AssertUtil.AreEqual(loginUser.Id, comment.EntityId, LanguageUtil.Translate("api_Repository_Comments_DeleteSpaceComment_spaceUserDelete"));

                var childCommnets = db.Comments.Where(c => c.State >= (int)CommentStateEnum.Waiting && c.EntityType == (int)CommentEnum.User && c.LocalPath.StartsWith(comment.LocalPath));
                foreach (var childCommnet in childCommnets)
                {
                    db.Entry<Comments>(childCommnet).State = EntityState.Deleted;
                }
                db.Configuration.ValidateOnSaveEnabled = false;
                return db.SaveChanges() > 0;
            });
            if (success)
            {
                CreateCache<Comments>();
            }
            return success;
        }

        /// <summary>
        /// ɾ���ռ�����
        /// </summary>
        /// <param name="loginUserId">��¼�û����</param>
        /// <param name="commentId">��������</param>
        /// <returns></returns>
        public bool DeleteSpaceComment(int loginUserId, int commentId)
        {
            #region ��֤�����Ƿ���ȷ
            AssertUtil.AreBigger(loginUserId, 0, "�û�δ��¼");
            AssertUtil.AreBigger(commentId, 0, "�ظ����Բ�����");
            #endregion

            var success = Execute<bool>((db) =>
            {
                #region ��֤���ݿ��Ƿ����
                var loginUser = db.User.FirstOrDefault(u => u.State == false && u.Id == loginUserId);
                AssertUtil.IsNotNull(loginUser, "�û������ڻ��߱�����");
                var comment = db.Comments.FirstOrDefault(c => c.State >= 0
                                                               && c.Id == commentId
                                                               && c.EntityType == (int)CommentEnum.User);
                AssertUtil.IsNotNull(comment, "�ظ������Կ����Ѿ���ɾ��������˲�ͨ��");
                #endregion

                AssertUtil.AreEqual(loginUser.Id, comment.EntityId, "ֻ�пռ��û�����ɾ������");

                var childCommnets = db.Comments.Where(c => c.State >= (int)CommentStateEnum.Waiting && c.EntityType == (int)CommentEnum.User && c.LocalPath.StartsWith(comment.LocalPath));
                foreach (var childCommnet in childCommnets)
                {
                    db.Entry<Comments>(childCommnet).State = EntityState.Deleted;
                }
                db.Configuration.ValidateOnSaveEnabled = false;
                return db.SaveChanges() > 0;
            });
            if (success)
            {
                CreateCache<Comments>();
            }
            return success;
        }
        #endregion
    }
}