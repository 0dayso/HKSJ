
using System;
using System.Data;
using System.Collections.Generic;
using System.Web.Http;
using HKSJ.WBVV.Api.Base;
using HKSJ.WBVV.Api.Filters;
using HKSJ.WBVV.Business.Interface;
using HKSJ.WBVV.Entity.ViewModel;
using HKSJ.WBVV.Entity.ViewModel.Client;
using HKSJ.WBVV.Entity.ViewModel.Manage;
using HKSJ.WBVV.Entity.ApiModel;
using HKSJ.WBVV.Common.Extender;

namespace HKSJ.WBVV.Api
{
    public class DictionaryController : ApiControllerBase
    {
        private readonly IDictionaryBusiness _dictionaryBusiness;
        public DictionaryController(IDictionaryBusiness dictionaryBusiness)
        {
            this._dictionaryBusiness = dictionaryBusiness;

        }
        /// <summary>
        /// һ�������µ�ɸѡ�б�
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet]
        public IList<DictionaryView> GetDictionaryViewList(int categoryId)
        {
            return this._dictionaryBusiness.GetDictionaryViewList(categoryId);
        }

        /// <summary>
        /// һ�������µ������б�
        /// Author:axone
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        [HttpGet]
        public IList<DictionaryView> GetDictionaryAndItemViewList(int cid)
        {
            return _dictionaryBusiness.GetDictionaryAndItemViewList(cid);
        }

        /// <summary>
        /// ��ȡѡ�еĹ�������
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public IList<DictionaryView> GetDictionaryViewList(int categoryId, string filter)
        {
            return this._dictionaryBusiness.GetDictionaryViewList(categoryId, filter);
        }

        /// <summary>
        /// ��̨�������ݹ���
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IList<HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView> GetCategoryAndDictionaryViewList()
        {
            return this._dictionaryBusiness.GetCategoryAndDictionaryViewList();
        }

        /// <summary>
        /// �����ֵ�
        /// </summary>
        /// <returns></returns>
        [HttpPost, CheckApp] //TODO ��Ҫ�ͻ���Ȩ����֤
        public Result CreateDictionary()
        {
            return CommonResult(() => this._dictionaryBusiness.CreateDictionary(JObject.Value<string>("name"), JObject.Value<int>("pid")), (r) => Console.WriteLine(r.ToJSON()));
        }

        /// <summary>
        /// ɾ���ֵ䣨�����ӽڵ�һ��ɾ����
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, CheckApp] //TODO ��Ҫ�ͻ���Ȩ����֤
        public Result DeleteDictionary()
        {
            return CommonResult(() => this._dictionaryBusiness.DeleteDictionary(JObject.Value<int>("id")), (r) => Console.WriteLine(r.ToJSON()));
        }

        /// <summary>
        /// �޸��ֵ�
        /// </summary>
        /// <returns></returns>
        [HttpPost, CheckApp] //TODO ��Ҫ�ͻ���Ȩ����֤
        public Result UpdateDictionary()
        {
            return CommonResult(() => this._dictionaryBusiness.UpdateDictionary(JObject.Value<string>("name"), JObject.Value<int>("id")), (r) => Console.WriteLine(r.ToJSON()));
        }


    }
}