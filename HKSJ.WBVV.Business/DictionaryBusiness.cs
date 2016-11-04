


using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using HKSJ.Utilities;
using HKSJ.WBVV.Business.Base;
using HKSJ.WBVV.Business.Interface;
using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Entity.ViewModel;
using HKSJ.WBVV.Entity.ViewModel.Client;
using HKSJ.WBVV.Repository.Interface;
using HKSJ.WBVV.Common.Extender.LinqExtender;
using HKSJ.WBVV.Entity.ViewModel.Manage;
using HKSJ.WBVV.Common.Assert;
using HKSJ.WBVV.Common.Language;

namespace HKSJ.WBVV.Business
{

    public class DictionaryBusiness : BaseBusiness, IDictionaryBusiness
    {
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IDictionaryItemRepository _dictionaryItemRepository;
        private readonly ICategoryRepository _categoryRepository;
        public DictionaryBusiness(IDictionaryRepository dictionaryRepository, IDictionaryItemRepository dictionaryItemRepository, ICategoryRepository categoryRepository)
        {
            this._dictionaryRepository = dictionaryRepository;
            this._dictionaryItemRepository = dictionaryItemRepository;
            this._categoryRepository = categoryRepository;
        }
        IQueryable<Dictionary> GetDictionaryQueryable()
        {
            var condtion = CondtionEqualState();
            return this._dictionaryRepository.GetEntityList(condtion).AsQueryable();
        }

        #region ��̨--�������ݹ���--�ֵ伯��
        /// <summary>
        /// ��̨--�������ݹ���--�ֵ伯��
        /// </summary>
        /// <returns></returns>
        public IList<HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView> GetCategoryAndDictionaryViewList()
        {
            var collects = new List<HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView>();
            var queryable = (from newTable in
                                 (
                                     ((
                                     from category in _categoryRepository.GetEntityList()
                                     where
                                       category.ParentId == 0

                                     select new HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView
                                     {
                                         id = category.Id + 1000,
                                         name = category.Name,
                                         pId = category.ParentId
                                     }
                                     ).Union
                                     (
                                     from dictionary in _dictionaryRepository.GetEntityList()
                                     select new HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView
                                     {
                                         id = dictionary.Id + 2000,
                                         name = dictionary.Name,
                                         pId = dictionary.CategoryId + 1000
                                     }
                                     ).Union
                                     (
                                     from dictionaryitem in _dictionaryItemRepository.GetEntityList()
                                     select new HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView
                                     {
                                         id = dictionaryitem.Id + 3000,
                                         name = dictionaryitem.Name,
                                         pId = dictionaryitem.DictionaryId + 2000
                                     }
                                     )))
                             select new HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView
                            {
                                id = newTable.id,
                                name = newTable.name,
                                pId = newTable.pId,
                                open = false
                            }).AsQueryable();
            collects = queryable.Any() ? queryable.ToList() : new List<HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView>();

            return collects;
        }

        #endregion

        #region �ֵ���ɾ�Ĳ���
        /// <summary>
        /// ����ֵ�
        /// </summary>
        /// <param name="name">�ֵ�����</param>
        /// <param name="parentId">�ֵ��ϼ�������</param>
        /// <returns></returns>
        public int CreateDictionary(string name, int parentId)
        {
            CheckNameNotNull(name);
            if (parentId > 0)
            {
                Category parentCategory;
                CheckCategoryId(parentId, out parentCategory);
                CheckLocalhostPathNotNull(parentCategory.LocationPath);
                CheckLocationPath(parentCategory.LocationPath);
            }
            var dictionary = new Dictionary()
            {
                Name = name,
                CategoryId = parentId,
                CreateManageId = 1,
                CreateTime = DateTime.Now,
                KeyWord = PinyinHelper.PinyinString(name),
            };
            this._dictionaryRepository.CreateEntity(dictionary);
            return dictionary.Id;
        }

        /// <summary>
        /// �޸��ֵ�����
        /// </summary>
        /// <param name="name">�ֵ�����</param>
        /// <param name="id">������</param>
        /// <returns></returns>
        public bool UpdateDictionary(string name, int id)
        {
            CheckNameNotNull(name);
            CheckIdBiggerZero(id);
            Dictionary dictionary;
            CheckDictionaryId(id, out dictionary);
            dictionary.Name = name;
            dictionary.KeyWord = PinyinHelper.PinyinString(name);
            dictionary.UpdateManageId = 1;
            dictionary.UpdateTime = DateTime.Now;
            return this._dictionaryRepository.UpdateEntity(dictionary);
        }

        /// <summary>
        /// ɾ���ֵ估�����ֵ�
        /// </summary>
        /// <param name="id">�ֵ���</param>
        /// <returns></returns>
        public bool DeleteDictionary(int id)
        {
            CheckIdBiggerZero(id);
            Dictionary dictionary;
            CheckDictionaryId(id, out dictionary);

            var dictItem = (from dict in this._dictionaryRepository.GetEntityList(CondtionEqualState())
                            join dicItem in this._dictionaryItemRepository.GetEntityList(CondtionEqualState()) on dict.Id equals
                                dicItem.DictionaryId
                            where dict.Id == dictionary.Id
                            select dicItem).AsQueryable();
            if (dictItem.Any())
            {
                //ɾ���ֵ�ڵ�
                var dictItemList = dictItem.ToList();
                this._dictionaryItemRepository.DeleteEntitys(dictItemList);

            }
            //ɾ���ֵ�
            return this._dictionaryRepository.DeleteEntity(dictionary);

        }

        #endregion

        #region һ�������µ�ɸѡ�б�
        /// <summary>
        /// ��ȡѡ�еĹ�������
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IList<DictionaryView> GetDictionaryViewList(int categoryId, string filter)
        {
            IList<DictionaryView> dictionaryViews = GetDictionaryViewList(categoryId);
            var dics = GetDictionarys('g','r','c',filter);
            if (dics.Count <= 0 || dictionaryViews.Count <= 0) return dictionaryViews;
            IList<DictionaryView> dictionaryViewList = new List<DictionaryView>();
            foreach (var dictionaryView in dictionaryViews)
            {
                var dictionary = new DictionaryView()
                {
                    Id = dictionaryView.Id,
                    Key = dictionaryView.Key,
                    GroupType=dictionaryView.GroupType,
                    DictionaryItems = new List<DictionaryItemView>()
                };
                foreach (var dictionaryItem in dictionaryView.DictionaryItems)
                {
                    foreach (var item in dics)
                    {
                        var g = item.Key;
                        var dic = item.Value;
                        if (string.IsNullOrEmpty(g)) continue;
                        if (dic.Count <= 0) continue;
                        foreach (var i in dic)
                        {
                            dictionaryItem.IsCheck = g == dictionaryItem.GroupType && dictionaryItem.DictionaryId == i.Key &&
                                                     dictionaryItem.Id == i.Value;
                            if (dictionaryItem.IsCheck)
                            {
                                break;
                            }
                        }
                        if (dictionaryItem.IsCheck)
                        {
                            break;
                        }
                    }
                    dictionary.DictionaryItems.Add(dictionaryItem);
                }
                dictionaryViewList.Add(dictionary);
            }
            return dictionaryViewList;
        }

        /// <summary>
        /// һ�������µ�ɸѡ�б�
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public IList<DictionaryView> GetDictionaryViewList(int categoryId)
        {
            IList<DictionaryView> dictionaryViews = new List<DictionaryView>();
            if (categoryId <= 0)
            {
                return dictionaryViews;
            }
            var condtions = new List<Condtion>()
            {
                ConditionEqualId(categoryId),
                CondtionEqualState(),
                CondtionEqualVisible(true)
            };
            var category = this._categoryRepository.GetEntity(condtions);
            if (category == null)
            {
                return dictionaryViews;
            }
            var cateCondtions = new List<Condtion>()
            {
                CondtionEqualParentId(category.Id),
                CondtionEqualState()
            };
            //��ȡһ�������µ�����
            var categorys = this._categoryRepository.GetEntityList(cateCondtions).Select(d => new DictionaryItemView()
            {
                DictionaryId = d.ParentId,
                Id = d.Id,
                Name = d.Name
            });
            if (categorys.Any())
            {
                dictionaryViews.Add(new DictionaryView()
                {
                    Key = LanguageUtil.Translate("api_Business_Dictionary_GetDictionaryViewList_Key"),
                    GroupType = "c",
                    Id = category.Id,
                    DictionaryItems = categorys.Select(c => new DictionaryItemView()
                    {
                        DictionaryId = c.DictionaryId,
                        Id = c.Id,
                        GroupType = "c",
                        Name = c.Name
                    }).ToList()
                });
            }
            var dicCondtions = new List<Condtion>()
            {
                CondtionEqualCategoryId(category.Id),
                CondtionEqualState()
            };
            //��ȡһ�������µ��ֵ�
            var dictionarys = this._dictionaryRepository.GetEntityList(dicCondtions);
            foreach (var dictionary in dictionarys)
            {
                var dicItemCondtions = new List<Condtion>() { CondtionEqualState(), CondtionEqualDictionaryId(dictionary.Id) };
                var dicItems = this._dictionaryItemRepository.GetEntityList(dicItemCondtions).Select(d => new DictionaryItemView()
                {
                    Id = d.Id,
                    DictionaryId = d.DictionaryId,
                    Name = d.Name,
                    GroupType = "d"
                });
                if (dicItems.Any())
                {
                    var dictionaryView = new DictionaryView()
                    {
                        Id = dictionary.Id,
                        Key = dictionary.Name,
                        GroupType = "d",
                        //�ֵ�ڵ�
                        DictionaryItems = dicItems.ToList()
                    };
                    dictionaryViews.Add(dictionaryView);
                }
            }
            return dictionaryViews;
        }


        /// <summary>
        /// һ�������µ������б�
        /// Author:axone
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public IList<DictionaryView> GetDictionaryAndItemViewList(int categoryId)
        {
            IList<DictionaryView> dictionaryViews = new List<DictionaryView>();
            if (categoryId <= 0)
            {
                return dictionaryViews;
            }
            var condtions = new List<Condtion>
            {
                ConditionEqualId(categoryId),
                CondtionEqualState(),
                CondtionEqualVisible(true)
            };
            var category = _categoryRepository.GetEntity(condtions);
            if (category == null)
            {
                return dictionaryViews;
            }
            var dicCondtions = new List<Condtion>
            {
                CondtionEqualCategoryId(category.Id),
                CondtionEqualState()
            };
            var dictionarys = _dictionaryRepository.GetEntityList(dicCondtions);
            foreach (var dictionary in dictionarys)
            {
                var dicItemCondtions = new List<Condtion> { CondtionEqualState(), CondtionEqualDictionaryId(dictionary.Id) };
                var dicItems = _dictionaryItemRepository.GetEntityList(dicItemCondtions).Select(d => new DictionaryItemView
                {
                    Id = d.Id,
                    DictionaryId = d.DictionaryId,
                    Name = d.Name,
                    GroupType = "d"
                });
                if (!dicItems.Any()) continue;
                var dictionaryView = new DictionaryView
                {
                    Id = dictionary.Id,
                    Key = dictionary.Name,
                    GroupType = "d",
                    DictionaryItems = dicItems.ToList()
                };
                dictionaryViews.Add(dictionaryView);
            }
            return dictionaryViews;
        }
        #endregion

        #region ����������
        /// <summary>
        /// ����ֵ����Ʋ�Ϊ��
        /// </summary>
        /// <param name="name"></param>
        private void CheckNameNotNull(string name)
        {
            AssertUtil.NotNullOrWhiteSpace(name, LanguageUtil.Translate("api_Business_Dictionary_CheckNameNotNull"));
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category"></param>
        private void CheckCategoryId(int id, out Category category)
        {
            var condtionId = ConditionEqualId(id);
            category = this._categoryRepository.GetEntity(condtionId);
            AssertUtil.IsNotNull(category, LanguageUtil.Translate("api_Business_Dictionary_CheckCategoryId"));
        }

        /// <summary>
        /// ����ֵ���
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category"></param>
        private void CheckDictionaryId(int id, out Dictionary dictionary)
        {
            var condtionId = ConditionEqualId(id);
            dictionary = this._dictionaryRepository.GetEntity(condtionId);
            AssertUtil.IsNotNull(dictionary, LanguageUtil.Translate("api_Business_Dictionary_CheckDictionaryId"));
        }

        /// <summary>
        /// ���������Ʋ�Ϊ��
        /// </summary>
        /// <param name="localhostPath"></param>
        private void CheckLocalhostPathNotNull(string localhostPath)
        {
            AssertUtil.NotNullOrWhiteSpace(localhostPath, LanguageUtil.Translate("api_Business_Dictionary_CheckLocalhostPathNotNull"));
        }

        /// <summary>
        /// ���LocationPath
        /// </summary>
        private void CheckLocationPath(string locationPath)
        {
            if (locationPath.IndexOf(',') != -1)
            {
                var arr = locationPath.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                AssertUtil.IsFalse(arr.Length >= 2, LanguageUtil.Translate("api_Business_Dictionary_CheckLocationPath"));
            }
        }

        /// <summary>
        /// �������Ų�С��0
        /// </summary>
        /// <param name="id"></param>
        private void CheckIdBiggerZero(int id)
        {
            AssertUtil.AreBigger(id, 0, LanguageUtil.Translate("api_Business_Dictionary_CheckIdBiggerZero"));
        }


        #endregion

        #region �������
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
        /// <summary>
        /// �Ƚ��ֵ���������
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
        /// �Ƚ��ֵ������
        /// </summary>
        /// <param name="dictionaryId"></param>
        /// <returns></returns>
        private Condtion CondtionEqualDictionaryId(int dictionaryId)
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
        /// �Ƚ��ֵ������
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



        #endregion

        #region �������

        #endregion
    }
}