using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HKSJ.Utilities;
using HKSJ.WBVV.Common.Assert;
using HKSJ.WBVV.Common.Extender;
using HKSJ.WBVV.Common.Extender.LinqExtender;
using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Entity.Enums;
using HKSJ.WBVV.Repository.Base;
using HKSJ.WBVV.Repository.Interface;

namespace HKSJ.WBVV.Repository
{
  
     public class UserRepository : BaseRepository, IUserRepository
    {
        public IQueryable<User> GetEntityList()
        {
            return base.GetEntityList<User>();
        }

        public IQueryable<User> GetEntityList(OrderCondtion orderCondtion)
        {
            return base.GetEntityList<User>(orderCondtion);
        }

        public IQueryable<User> GetEntityList(IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<User>(orderCondtions);
        }

        public IQueryable<User> GetEntityList(Condtion condtion)
        {
            return base.GetEntityList<User>(condtion);
        }

        public IQueryable<User> GetEntityList(Condtion condtion, OrderCondtion orderCondtion)
        {
            return base.GetEntityList<User>(condtion, orderCondtion);
        }

        public IQueryable<User> GetEntityList(Condtion condtion, IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<User>(condtion, orderCondtions);
        }

        public IQueryable<User> GetEntityList(IList<Condtion> condtions)
        {
            return base.GetEntityList<User>(condtions);
        }

        public IQueryable<User> GetEntityList(IList<Condtion> condtions, OrderCondtion orderCondtion)
        {
            return base.GetEntityList<User>(condtions, orderCondtion);
        }

        public IQueryable<User> GetEntityList(IList<Condtion> condtions, IList<OrderCondtion> orderCondtions)
        {
            return base.GetEntityList<User>(condtions, orderCondtions);
        }
        public User GetEntity(Condtion condtion)
        {
            return base.GetEntity<User>(condtion);
        }

        public User GetEntity(IList<Condtion> condtions)
        {
            return base.GetEntity<User>(condtions);
        }
        public bool CreateEntity(User entity)
        {
            return base.CreateEntity<User>(entity);
        }

        public void CreateEntitys(IList<User> entitys)
        {
            base.CreateEntitys<User>(entitys);
        }

        public bool UpdateEntity(User entity)
        {
            return base.UpdateEntity<User>(entity);
        }

        public void UpdateEntitys(IList<User> entitys)
        {
            base.UpdateEntitys<User>(entitys);
        }

        public bool DeleteEntity(User entity)
        {
            return base.DeleteEntity<User>(entity);
        }

        public void DeleteEntitys(IList<User> entitys)
        {
            base.DeleteEntitys<User>(entitys);
        }

        #region �û���¼

        /// <summary>
        /// �û���¼
        /// </summary>
        /// <param name="account">�ʺ�</param>
        /// <param name="password">����</param>
        /// <param name="ipAddress">ip��ַ</param>
        public void IncomeLogin(string account, string password, string ipAddress)
        {
            var success = Execute(db =>
            {
                account = account.UrlDecode();
                password = password.UrlDecode();
                AssertUtil.NotNullOrWhiteSpace(account, "�û��˺�Ϊ��");
                AssertUtil.NotNullOrWhiteSpace(password, "�û�����Ϊ��");
                AssertUtil.NotNullOrWhiteSpace(ipAddress, "������ô��");
                var user = db.User.FirstOrDefault(u => u.State == false && u.Password == password && (u.Phone == account || u.Account == account));
                if (user == null || user.Id < 1)
                {
                    user = db.User.FirstOrDefault(u => u.State == false && u.Password == password && (u.Email == account || u.Account == account));
                }
                AssertUtil.IsNotNull(user, "�˺Ų����ڻ��������");
                //�ϴε�¼ʱ���ip��ַ
                DateTime? lastLoginTime = user.CurrentLoginTime;
                string lastLoginIp = user.CurrentLoginIp;

                //��¼�µĵ�ǰ��¼�¼����ϴε�¼ʱ��
                user.LastLoginIp = lastLoginIp;
                user.LastLoginTime = lastLoginTime;
                user.CurrentLoginTime = DateTime.Now;
                user.CurrentLoginIp = ipAddress;

                db.Entry<User>(user).State = EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;
                return db.SaveChanges() > 0;
            });
            if (success)
            {
                CreateCache<User>();
            }
        }

        #endregion

        #region �û�ע��
        /// <summary>
        /// �û�ע��
        /// </summary>
        /// <returns></returns>
        public void IncomeRegister(string account, string password, string ipAddress, int type)
        {
            var success = Execute<bool>((db) =>
            {
                account = account.UrlDecode();
                password = password.UrlDecode();
                AssertUtil.NotNullOrWhiteSpace(account, "�û��˺�Ϊ��");
                AssertUtil.NotNullOrWhiteSpace(password, "�û�����Ϊ��");
                var userAccount = db.User.FirstOrDefault(u => u.Account == account);
                AssertUtil.IsNull(userAccount, "�˺��Ѿ�����");

                var user = new User
                {
                    Account = account,
                    SubscribeNum = 0,
                    UserState = 0,
                    BB = 0,
                    Gender = false,
                    PlayCount = 0,
                    FansCount = 0,
                    State = false,
                    Password = Md5Helper.MD5(password, 32),//TODO �û����� MD5����
                    CreateTime = DateTime.Now,
                    Level = 0,
                    UseBB = 0
                };
                if (type == 0)
                {
                    user.Phone = account;
                }
                else
                {
                    user.Email = account;
                }
                db.User.Add(user);
                var result = db.SaveChanges() > 0;
                if (result)
                {
                    user.NickName = "����_" + user.Id;
                    db.Entry(user).State = EntityState.Modified;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    result = db.SaveChanges() > 0;
                }
                return result;
            });
            if (success)
            {
                CreateCache<User>();
            }
        }
        #endregion

        #region �������������ʺŲ���¼
        /// <summary>
        /// �������������ʺŲ���¼
        /// </summary>
        /// <param name="account">�ʺ�</param>
        /// <param name="password">����</param>
        /// <param name="typeCode">���������ͱ���</param>
        /// <param name="relatedId">������Ψһ��ݱ�ʶ</param>
        /// <param name="nickName">�ǳ�</param>
        /// <param name="figureURL">ͷ��</param>
        /// <param name="ipAddress">ip��ַ</param>
        public void ThirdPartyBindAndLogin(string account, string password, string typeCode, string relatedId, string nickName, string figureURL, string ipAddress)
        {
            var success = Execute((db) =>
            {
                account = account.UrlDecode();
                password = password.UrlDecode();
                typeCode = typeCode.UrlDecode();
                relatedId = relatedId.UrlDecode();
                nickName = nickName.UrlDecode();
                figureURL = figureURL.UrlDecode();

                AssertUtil.NotNullOrWhiteSpace(account, "�û��˺�Ϊ��");
                AssertUtil.NotNullOrWhiteSpace(password, "�û�����Ϊ��");
                AssertUtil.NotNullOrWhiteSpace(typeCode, "���������ͱ��붪ʧ,���Ժ�����");
                AssertUtil.NotNullOrWhiteSpace(relatedId, "��������ݱ�ʶ��ʧ,���Ժ�����");
                AssertUtil.NotNullOrWhiteSpace(nickName, "������ʧ�����Ժ�����");
                AssertUtil.NotNullOrWhiteSpace(figureURL, "������ʧ�����Ժ�����");
                AssertUtil.NotNullOrWhiteSpace(ipAddress, "���Ժ�����");

                var user = db.User.FirstOrDefault(u => u.State == false && u.Password == password && (u.Phone == account || u.Account == account));
                if (user == null || user.Id < 1)
                {
                    user = db.User.FirstOrDefault(u => u.State == false && u.Password == password && (u.Email == account || u.Account == account));
                }
                AssertUtil.IsNotNull(user, "�˺Ų����ڻ��������");

                //����Ƿ��Ѱ�
                var userBind = db.UserBind.FirstOrDefault(u => u.State == false && u.TypeCode == typeCode && u.UserId == user.Id && u.RelatedId == relatedId);
                if (userBind == null)
                {
                    //�����ʺ��Ƿ�������������ʺ�
                    if (db.UserBind.Any(u => u.State == false && u.TypeCode == typeCode && u.UserId == user.Id && u.RelatedId != relatedId))
                        AssertUtil.IsTrue(false, "���ʺ��ѱ��󶨣��뻻���ʺ�����...");
                    //���������ʺ��Ƿ�������ʺ�
                    if (db.UserBind.Any(u => u.State == false && u.TypeCode == typeCode && u.UserId != user.Id && u.RelatedId == relatedId))
                        AssertUtil.IsTrue(false, "�������ʺ��Ѱ������ʺţ��뻻���ʺ�����...");

                    //��Ϣû���޸ĵ�����£���ȡ�������ǳƣ�ͷ�����Ϣ
                    if (user.UpdateTime.IsNull() || user.CreateTime >= user.UpdateTime)
                    {
                        user.NickName = nickName;
                        user.Picture = figureURL;
                    }

                    DateTime dt = DateTime.Now;
                    //�󶨵������ʺ�
                    UserBind ub = new UserBind
                    {
                        UserId = user.Id,
                        TypeCode = typeCode,
                        RelatedId = relatedId,
                        CreateUserId = user.Id,
                        CreateTime = dt,
                        UpdateUserId = user.Id,
                        UpdateTime = dt,
                        State = false
                    };

                    user.UpdateTime = dt;

                    db.UserBind.Add(ub);

                    db.SaveChanges();
                }



                //�ϴε�¼ʱ���ip��ַ
                DateTime? lastLoginTime = user.CurrentLoginTime;
                string lastLoginIp = user.CurrentLoginIp;

                //��¼�µĵ�ǰ��¼�¼����ϴε�¼ʱ��
                user.LastLoginIp = lastLoginIp;
                user.LastLoginTime = lastLoginTime;
                user.CurrentLoginTime = DateTime.Now;
                user.CurrentLoginIp = ipAddress;

                db.Entry<User>(user).State = EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;
                return db.SaveChanges() > 0;
            });
            if (success)
            {
                CreateCache<User>();
                CreateCache<UserBind>();
            }
        }
        #endregion

        #region ������ע�����ʺŲ���
        /// <summary>
        /// ������ע�����ʺŲ���
        /// </summary>
        /// <returns></returns>
        public void ThirdPartyBindAndRegister(string account, string password, string typeCode, string relatedId, string nickName, string figureURL, int type, string ipAddress)
        {
            var success = Execute<bool>((db) =>
            {
                account = account.UrlDecode();
                password = password.UrlDecode();
                typeCode = typeCode.UrlDecode();
                relatedId = relatedId.UrlDecode();
                nickName = nickName.UrlDecode();
                figureURL = figureURL.UrlDecode();

                AssertUtil.NotNullOrWhiteSpace(account, "�û��˺�Ϊ��");
                AssertUtil.NotNullOrWhiteSpace(password, "�û�����Ϊ��");
                AssertUtil.NotNullOrWhiteSpace(typeCode, "���������ͱ��붪ʧ,���Ժ�����");
                AssertUtil.NotNullOrWhiteSpace(relatedId, "��������ݱ�ʶ��ʧ,���Ժ�����");
                AssertUtil.NotNullOrWhiteSpace(nickName, "������ʧ�����Ժ�����");
                AssertUtil.NotNullOrWhiteSpace(figureURL, "������ʧ�����Ժ�����");

                var userAccount = db.User.FirstOrDefault(u => u.Account == account || u.Phone == account || u.Email == account);
                AssertUtil.IsNull(userAccount, "�˺��Ѿ�����");

                DateTime dt = DateTime.Now;

                var user = new User
                {
                    Account = account,
                    SubscribeNum = 0,
                    UserState = 0,
                    BB = 0,
                    Gender = false,
                    PlayCount = 0,
                    FansCount = 0,
                    State = false,
                    Password = Md5Helper.MD5(password, 32),//TODO �û����� MD5����
                    CreateTime = dt,
                    UpdateTime = dt,
                    Level = 0,
                    UseBB = 0,
                    NickName = nickName,
                    Picture = figureURL
                };

                if (type == 0)
                {
                    user.Phone = account;
                }
                else
                {
                    user.Email = account;
                }

                db.User.Add(user);
                var result = db.SaveChanges() > 0;

                if (result)
                {
                    db.Entry(user).State = EntityState.Modified;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    result = db.SaveChanges() > 0;
                }

                //�󶨵����������ʺŲ��������жϣ��жϵ������Ƿ��ѱ��󶨺�д���ϵ

                //���������ʺ��Ƿ�������ʺ�
                if (db.UserBind.Any(u => u.State == false && u.TypeCode == typeCode && u.UserId != user.Id && u.RelatedId == relatedId))
                    AssertUtil.IsTrue(false, "�������ʺ��Ѱ������ʺţ��뻻���ʺ�����...");
                //�󶨵������ʺ�
                var ub = new UserBind
                {
                    UserId = user.Id,
                    TypeCode = typeCode,
                    RelatedId = relatedId,
                    CreateUserId = user.Id,
                    CreateTime = dt,
                    UpdateUserId = user.Id,
                    UpdateTime = dt,
                    State = false
                };
                db.UserBind.Add(ub);
                db.SaveChanges();

                return result;
            });
            if (success)
            {
                CreateCache<User>();
                CreateCache<UserBind>();
            }
        }
        #endregion

        #region �Զ�ע�Ტ�󶨵�����
        /// <summary>
        /// �Զ�ע�Ტ�󶨵�����
        /// </summary>
        /// <returns></returns>
        public void AutoRegisterAndBindThirdParty(string typeCode, string relatedId, string nickName, string figureURL, string ipAddress)
        {
            var success = Execute<bool>((db) =>
            {

                typeCode = typeCode.UrlDecode();
                relatedId = relatedId.UrlDecode();
                nickName = nickName.UrlDecode();
                figureURL = figureURL.UrlDecode();

                AssertUtil.NotNullOrWhiteSpace(typeCode, "���������ͱ��붪ʧ,���Ժ�����");
                AssertUtil.NotNullOrWhiteSpace(relatedId, "��������ݱ�ʶ��ʧ,���Ժ�����");
                AssertUtil.NotNullOrWhiteSpace(nickName, "������ʧ�����Ժ�����");
                AssertUtil.NotNullOrWhiteSpace(figureURL, "������ʧ�����Ժ�����");

                DateTime dt = DateTime.Now;
                Random rdm = new Random();
                var user = new User
                {
                    Account = "AutoRegist" + rdm.Next(50000, 999999999).ToString(),
                    Phone = "",
                    SubscribeNum = 0,
                    UserState = 0,
                    BB = 0,
                    Gender = false,
                    PlayCount = 0,
                    FansCount = 0,
                    State = false,
                    Password = Md5Helper.MD5("AutoRegist" + rdm.Next(50000, 999999999).ToString(), 32),
                    CreateTime = dt,
                    UpdateTime = dt,
                    Level = 0,
                    NickName = nickName,
                    Picture = figureURL,
                    UseBB = 0
                };
                db.User.Add(user);
                var result = db.SaveChanges() > 0;
                //�󶨵����������ʺŲ��������жϣ��жϵ������Ƿ��ѱ��󶨺�д���ϵ

                //���������ʺ��Ƿ�������ʺ�
                if (db.UserBind.Any(u => u.State == false && u.TypeCode == typeCode && u.UserId != user.Id && u.RelatedId == relatedId))
                    AssertUtil.IsTrue(false, "�������ʺ��Ѱ������ʺţ��뻻���ʺ�����...");
                //�󶨵������ʺ�
                var ub = new UserBind
                {
                    UserId = user.Id,
                    TypeCode = typeCode,
                    RelatedId = relatedId,
                    CreateUserId = user.Id,
                    CreateTime = dt,
                    UpdateUserId = user.Id,
                    UpdateTime = dt,
                    State = false
                };
                db.UserBind.Add(ub);
                db.SaveChanges();

                return result;
            });
            if (success)
            {
                CreateCache<User>();
                CreateCache<UserBind>();
            }
        }
        #endregion

        #region ͨ��ID��¼��һ������SSO
        /// <summary>
        /// ͨ��ID��¼��һ������SSO
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ipAddress"></param>
        public void IncomeLogin(int userId, string ipAddress)
        {
            var success = Execute(db =>
            {

                AssertUtil.AreBigger(userId, 0, "��������������ô��");
                AssertUtil.NotNullOrWhiteSpace(ipAddress, "������ô��");
                var user = db.User.FirstOrDefault(u => u.State == false && u.Id == userId);

                AssertUtil.IsNotNull(user, "�˺Ų����ڻ��������");
                //�ϴε�¼ʱ���ip��ַ
                DateTime? lastLoginTime = user.CurrentLoginTime;
                string lastLoginIp = user.CurrentLoginIp;

                //��¼�µĵ�ǰ��¼�¼����ϴε�¼ʱ��
                user.LastLoginIp = lastLoginIp;
                user.LastLoginTime = lastLoginTime;
                user.CurrentLoginTime = DateTime.Now;
                user.CurrentLoginIp = ipAddress;

                db.Entry<User>(user).State = EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;
                return db.SaveChanges() > 0;
            });
            if (success)
            {
                CreateCache<User>();
            }
        }
        #endregion
    }
}