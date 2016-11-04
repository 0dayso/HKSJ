
using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using HKSJ.WBVV.Business.Base;
using HKSJ.WBVV.Business.Interface;
using HKSJ.WBVV.Common.Assert;
using HKSJ.WBVV.Common.Extender;
using HKSJ.WBVV.Common.Extender.LinqExtender;
using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Entity.ViewModel;
using HKSJ.WBVV.Entity.ViewModel.Client;
using HKSJ.WBVV.Entity.ViewModel.Manage;
using HKSJ.WBVV.Repository;
using HKSJ.WBVV.Repository.Interface;
using HKSJ.Utilities;
using HKSJ.WBVV.Common.Language;

namespace HKSJ.WBVV.Business
{

    public class CategoryBusiness : BaseBusiness, ICategoryBusiness
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IDictionaryItemRepository _dictionaryItemRepository;
        private readonly IPlateRepository _plateRepository;
        public CategoryBusiness(
            ICategoryRepository categoryRepository,
            IVideoRepository videoRepository,
            IDictionaryRepository dictionaryRepository,
            IDictionaryItemRepository dictionaryItemRepository,
            IPlateRepository plateRepository
            )
        {
            this._categoryRepository = categoryRepository;
            this._videoRepository = videoRepository;
            this._dictionaryRepository = dictionaryRepository;
            this._dictionaryItemRepository = dictionaryItemRepository;
            this._plateRepository = plateRepository;
        }

        #region manage
        /// <summary>
        /// ��ȡ���еķ���
        /// </summary>
        /// <returns></returns>
        private IQueryable<HKSJ.WBVV.Entity.Category> GetCategoryList()
        {
            return this._categoryRepository.GetEntityList();
        }
        #region һ�������б�
        /// <summary>
        /// һ�������б�
        /// </summary>
        /// <returns></returns>
        public IList<HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView> GetOneCategoryViewList()
        {
            IQueryable<HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView> queryable = GetCategoryList()
                .Where(o => !o.State).Query(CondtionEqualParentId(0)).Select(s => new HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView()
            {
                id = s.Id,
                pId = s.ParentId,
                name = s.Name,
                open = false
            }).AsQueryable();
            return queryable.Any() ? queryable.ToList() : new List<HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView>();
        }
        #endregion

        #region �����б�
        /// <summary>
        /// ��̨������༯��
        /// </summary>
        /// <returns></returns>
        public IList<HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView> GetCategoryViewList()
        {
            IList<HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView> categoryViews = GetCategoryList().Select(s => new HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView()
            {
                id = s.Id,
                pId = s.ParentId,
                name = s.Name,
                open = false
            }).ToList();
            return categoryViews;
        }
        #endregion

        #region ��ӷ���
        /// <summary>
        /// ��ӷ���
        /// </summary>
        /// <param name="name">��������</param>
        /// <param name="parentId">������ϼ����</param>
        /// <returns></returns>
        public int CreateCategory(string name, int parentId)
        {
            CheckNameNotNull(name);
            CheckName(parentId, name);
            string locationpath = "";
            if (parentId > 0)
            {
                Category parentCategory;
                CheckId(parentId, out parentCategory);
                CheckLocalhostPathNotNull(parentCategory.LocationPath);
                CheckLocationPath(parentCategory.LocationPath);
                locationpath = parentCategory.LocationPath + ",";
            }
            var category = new Category()
            {
                Name = name,
                ParentId = parentId,
                CreateManageId = 1,
                CreateTime = DateTime.Now,
                KeyWord = PinyinHelper.PinyinString(name),
                LocationPath = locationpath
            };
            this._categoryRepository.CreateEntity(category);
            category.LocationPath = category.LocationPath + GetLocalhostPath(category.Id);
            this._categoryRepository.UpdateEntity(category);
            return category.Id;
        }
        #endregion

        #region �޸ķ���
        /// <summary>
        /// �޸ķ�������
        /// </summary>
        /// <param name="name">��������</param>
        /// <param name="id">������</param>
        /// <returns></returns>
        public bool UpdateCategory(string name, int id)
        {
            CheckNameNotNull(name);
            CheckIdBiggerZero(id);
            Category category;
            CheckId(id, out category);
            CheckName(category.ParentId, name);
            category.Name = name;
            category.KeyWord = PinyinHelper.PinyinString(name);
            category.UpdateManageId = 1;
            category.UpdateTime = DateTime.Now;
            return this._categoryRepository.UpdateEntity(category);
        }
        #endregion

        #region �ƶ�����
        /// <summary>
        /// �ƶ�����
        /// </summary>
        /// <param name="id">������</param>
        /// <param name="pid">�ƶ����ĸ�������</param>
        /// <returns></returns>
        public bool MoveCategory(int id, int pid)
        {
            CheckIdBiggerZero(id);
            Category category;
            CheckId(id, out category);
            CheckHasVedio(category);
            return true;

        }
        #endregion

        #region ɾ������
        /// <summary>
        /// ɾ������
        /// </summary>
        /// <param name="id">������</param>
        /// <returns></returns>
        public bool DeleteCategory(int id)
        {
            CheckIdBiggerZero(id);
            Category category;
            CheckId(id, out category);
            CheckHasVedio(category);
            CheckHasPlate(category);

            var categorys = (from c in this._categoryRepository.GetEntityList(CondtionInLocationPathAndNotEqualCategoryId(category))
                             select c).AsQueryable<Category>();
            //ɾ�������ӽڵ�
            if (categorys.Any())
            {
                this._categoryRepository.DeleteEntitys(categorys.ToList<Category>());
            }

            var dictionarys = (from cate in GetCategoryListSort()
                               join dic in this._dictionaryRepository.GetEntityList(CondtionEqualState()) on cate.Id equals dic.CategoryId
                               where cate.Id == id
                               select dic).AsQueryable();
            if (dictionarys.Any())
            {
                var dictItem = (from dict in dictionarys
                                join dicItem in this._dictionaryItemRepository.GetEntityList(CondtionEqualState()) on dict.Id equals
                                    dicItem.DictionaryId
                                select dicItem).AsQueryable();
                if (dictItem.Any())
                {
                    //ɾ���ֵ��ӽڵ�
                    var dictItemList = dictItem.ToList();
                    this._dictionaryItemRepository.DeleteEntitys(dictItemList);
                }
                //ɾ���ֵ�ڵ�
                var dictList = dictionarys.ToList();
                this._dictionaryRepository.DeleteEntitys(dictList);
            }
            //ɾ������
            return this._categoryRepository.DeleteEntity(category);
        }
        #endregion

        #endregion

        #region client

        #region Ĭ�������ѯ����
        /// <summary>
        /// ��ȡ���õ����з�����Ϣ,Ĭ������SortNum,CreateTime,UpdateTime
        /// </summary>
        /// <returns></returns>
        IQueryable<Category> GetCategoryListSort()
        {
            var orderCodtion = new List<OrderCondtion>()
            {
                OrderCondtionSortNum(true),
                OrderCondtionCreateTime(true)
            };
            return _categoryRepository.GetEntityList(CondtionEqualState(), orderCodtion);
        }
        #endregion

        #region ��������˵�
        /// <summary>
        /// ��ȡָ�������µķ��༯��
        /// </summary>
        /// <param name="parentId">�������</param>
        /// <returns></returns>
        private IList<Category> GetCategoryListByParentId(int parentId)
        {
            var condtion = CondtionEqualParentId(parentId);
            IQueryable<Category> queryable = GetCategoryListSort().Query(condtion);
            return queryable.ToList();
        }
        /// <summary>
        /// ��ȡ��������˵�
        /// </summary>
        /// <returns></returns>
        public IList<MenuView> GetMenuViewList()
        {
            IList<MenuView> parentCategories = new List<MenuView>();
            var parentCategorys = GetCategoryListByParentId(0);
            foreach (var parentCategory in parentCategorys)
            {
                var category = new MenuView()
                {
                    ParentCategory = new Menu() { Id = parentCategory.Id, Name = parentCategory.Name },
                    //ChildCategorys = GetChildMenuViews(parentCategory.Id).Take(parentCategory.PageSize).ToList()
                    ChildCategorys = GetChildMenuViews(parentCategory.Id).Take(30).ToList()
                };
                parentCategories.Add(category);
            }
            return parentCategories;
        }

        private IList<MenuView> GetChildMenuViews(int parentId)
        {
            IList<MenuView> childCategories = new List<MenuView>();
            var childCategorys = GetCategoryListByParentId(parentId);
            foreach (var childCategory in childCategorys)
            {
                var category = new MenuView()
                {
                    ParentCategory = new Menu() { Id = childCategory.Id, Name = childCategory.Name },
                    ChildCategorys = GetChildMenuViews(childCategory.Id)
                };
                childCategories.Add(category);
            }
            return childCategories;
        }
        #endregion

        #region ��ȡ����ʾ��������˵�
        /// <summary>
        /// ��ȡ����ʾ��ָ�������µķ��༯��
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private IList<Category> GetCategoryListByParentIdVisible(int parentId)
        {
            if (parentId <= 0)
            {
                parentId = 0;
            }
            var condtions = new List<Condtion>
            {
               CondtionEqualParentId(parentId),
               CondtionEqualVisible(true)
            };
            IQueryable<Category> queryable = GetCategoryListSort().Query(condtions);
            return queryable.ToList();
        }
        /// <summary>
        /// ��ȡ����ʾ��������˵�
        /// </summary>
        /// <returns></returns>
        public IList<MenuView> GetMenuViewListVisible()
        {
            IList<MenuView> parentCategories = new List<MenuView>();
            var parentCategorys = GetCategoryListByParentIdVisible(0);
            foreach (var parentCategory in parentCategorys)
            {
                var category = new MenuView()
                {
                    ParentCategory = new Menu() { Id = parentCategory.Id, Name = parentCategory.Name },
                    ChildCategorys = GetChildMenuViews(parentCategory.Id).Take(parentCategory.PageSize).ToList()
                };
                parentCategories.Add(category);
            }
            return parentCategories;
        }

        private IList<MenuView> GetChildMenuViewsVisible(int parentId)
        {
            IList<MenuView> childCategories = new List<MenuView>();
            var childCategorys = GetCategoryListByParentIdVisible(parentId);
            foreach (var childCategory in childCategorys)
            {
                var category = new MenuView()
                {
                    ParentCategory = new Menu() { Id = childCategory.Id, Name = childCategory.Name },
                    ChildCategorys = GetChildMenuViews(childCategory.Id)
                };
                childCategories.Add(category);
            }
            return childCategories;
        }
        #endregion


        #region ����id�õ���������ID
        public int GetParentId(int cid)
        {
            var info = this._categoryRepository.GetEntity(ConditionEqualId(cid));
            if (info == null) return cid;
            if (info.ParentId == 0) return info.Id;
            else
            {
                return GetParentId(info.ParentId);
            }
        }

        #endregion

        #region ���ݷ���id�õ��ϼ�������Ϣ
        public Category GetParentInfo(int cid)
        {
            var childinfo = this._categoryRepository.GetEntity(ConditionEqualId(cid));
            var parentinfo = this._categoryRepository.GetEntity(ConditionEqualId(childinfo.ParentId));
            return parentinfo;
        }

        #endregion
        #endregion

        #region ����������
        /// <summary>
        /// ���������Ʋ�Ϊ��
        /// </summary>
        /// <param name="name"></param>
        private void CheckNameNotNull(string name)
        {
            AssertUtil.NotNullOrWhiteSpace(name, LanguageUtil.Translate("api_Business_Category_CheckNameNotNull"));
        }

        /// <summary>
        /// �����������Ƿ����
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="name"></param>
        private void CheckName(int pid, string name)
        {
            var condtions = new List<Condtion>()
            {
                CondtionEqualName(name),
                CondtionEqualParentId(pid),
            };
            var categoryName = this._categoryRepository.GetEntity(condtions);
            AssertUtil.IsNull(categoryName, LanguageUtil.Translate("api_Business_Category_CheckName").F(name));
        }
        /// <summary>
        /// �����������Ƿ����
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="name"></param>
        private void CheckName(int pid, int id, string name)
        {
            var condtions = new List<Condtion>()
            {
                CondtionEqualName(name),
                CondtionEqualParentId(pid),
                new Condtion()
                {
                    ExpressionLogic = ExpressionLogic.And,
                    ExpressionType = ExpressionType.NotEqual,
                    FiledName = "Id",
                    FiledValue = id
                }
            };
            var categoryName = this._categoryRepository.GetEntity(condtions);
            AssertUtil.IsNull(categoryName, LanguageUtil.Translate("api_Business_Category_CheckName_categoryName").F(name));
        }
        /// <summary>
        /// �����ุ��
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="category"></param>
        private void CheckParentId(int parentId, out Category category)
        {
            var condtions = new List<Condtion>()
            {
                CondtionEqualState(),
                CondtionEqualParentId(parentId)
            };
            category = this._categoryRepository.GetEntity(condtions);
            AssertUtil.IsNotNull(category, LanguageUtil.Translate("api_Business_Category_CheckParentId"));
        }
        /// <summary>
        /// ���locationPath
        /// </summary>
        /// <param name="locationPath"></param>
        private void CheckLocationPathNotNull(string locationPath)
        {
            AssertUtil.NotNullOrWhiteSpace(locationPath, LanguageUtil.Translate("api_Business_Category_CheckLocationPathNotNull"));
        }
        /// <summary>
        /// ���LocationPath
        /// </summary>
        private void CheckLocationPath(string locationPath)
        {
            if (locationPath.IndexOf(',') != -1)
            {
                var arr = locationPath.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                AssertUtil.IsFalse(arr.Length >= 2, LanguageUtil.Translate("api_Business_Category_CheckLocationPath"));
            }
        }

        /// <summary>
        /// �������Ų�С��0
        /// </summary>
        /// <param name="id"></param>
        private void CheckIdBiggerZero(int id)
        {
            AssertUtil.AreBigger(id, 0, LanguageUtil.Translate("api_Business_Category_CheckIdBiggerZero"));
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category"></param>
        private void CheckId(int id, out Category category)
        {
            IList<Condtion> condtions = new List<Condtion>()
            {
                CondtionEqualState(),
                ConditionEqualId(id)
            };
            category = this._categoryRepository.GetEntity(condtions);
            AssertUtil.IsNotNull(category, LanguageUtil.Translate("api_Business_Category_CheckId"));
        }
        /// <summary>
        /// ���������Ƿ���ڿ��õ���Ƶ
        /// </summary>
        /// <param name="category"></param>
        private void CheckHasVedio(Category category)
        {
            var vedios = (from video in this._videoRepository.GetEntityList(CondtionEqualState())
                          join cate in this._categoryRepository.GetEntityList(CondtionEqualState())
                              on video.CategoryId equals cate.Id
                          where cate.LocationPath.StartsWith(category.LocationPath) && video.VideoState == 3 //TODO ��ǿCheckState=1
                          select video
                      ).AsQueryable();
            AssertUtil.IsFalse(vedios.Any(), LanguageUtil.Translate("api_Business_Category_CheckHasVedio").F(vedios.Count()));
        }
        /// <summary>
        /// ���������Ƿ���ڰ��
        /// </summary>
        /// <param name="category"></param>
        private void CheckHasPlate(Category category)
        {
            var plates = (from p in this._plateRepository.GetEntityList(CondtionEqualState())
                          join c in this._categoryRepository.GetEntityList(CondtionEqualState())
                              on p.CategoryId equals c.Id
                          where c.LocationPath.StartsWith(category.LocationPath)
                          select p
                    ).AsQueryable();
            AssertUtil.IsFalse(plates.Any(), LanguageUtil.Translate("api_Business_Category_CheckHasPlate").F(plates.Count()));
        }

        /// <summary>
        /// ���������Ʋ�Ϊ��
        /// </summary>
        /// <param name="localhostPath"></param>
        private void CheckLocalhostPathNotNull(string localhostPath)
        {
            AssertUtil.NotNullOrWhiteSpace(localhostPath, LanguageUtil.Translate("api_Business_Category_CheckLocalhostPathNotNull"));
        }
        #endregion

        #region �������

        /// <summary>
        /// �ȽϷ����������
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Condtion CondtionEqualName(string name)
        {
            var condtion = new Condtion()
            {
                FiledName = "Name",
                FiledValue = name,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.Equal
            };
            return condtion;
        }
        /// <summary>
        /// �ȽϷ������ư���
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Condtion CondtionContainsName(string name)
        {
            var condtion = new Condtion()
            {
                FiledName = "Name",
                FiledValue = name,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.Contains
            };
            return condtion;
        }
        /// <summary>
        /// �ȽϷ��ุ��������
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private Condtion CondtionEqualParentId(int parentId)
        {
            var condtion = new Condtion()
            {
                FiledName = "ParentId",
                FiledValue = parentId,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.Equal
            };
            return condtion;
        }
        /// <summary>
        /// �ȽϷ�����ʾ״̬���
        /// </summary>
        /// <param name="visible"></param>
        /// <returns></returns>
        private Condtion CondtionEqualVisible(bool visible)
        {
            var condtion = new Condtion()
           {
               FiledName = "Visible",
               FiledValue = visible,
               ExpressionLogic = ExpressionLogic.And,
               ExpressionType = ExpressionType.Equal
           };
            return condtion;
        }

        private IList<Condtion> CondtionInLocationPathAndNotEqualCategoryId(Category category)
        {
            IList<Condtion> list = new List<Condtion>();
            var condtion = new Condtion()
            {
                FiledName = "LocationPath",
                FiledValue = category.LocationPath,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.StartWith
            };
            list.Add(condtion);

            condtion = new Condtion()
            {
                FiledName = "Id",
                FiledValue = category.Id,
                ExpressionLogic = ExpressionLogic.And,
                ExpressionType = ExpressionType.NotEqual
            };
            list.Add(condtion);

            return list;
        }

        #endregion

        #region �������


        #endregion
    }
}