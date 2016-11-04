using System;
using System.Data;
using System.Collections.Generic;
using System.Web.Http;
using HKSJ.WBVV.Api.Base;
using HKSJ.WBVV.Api.Filters;
using HKSJ.WBVV.Business.Interface;
using HKSJ.WBVV.Repository.Interface;
using HKSJ.WBVV.Entity.ApiModel;
using HKSJ.WBVV.Common.Extender;

namespace HKSJ.WBVV.Api
{
    public class DictionaryItemController : ApiControllerBase
    {
        private readonly IDictionaryItemBusiness _dictionaryItemBusiness;
        public DictionaryItemController(IDictionaryItemBusiness dictionaryItemBusiness)
        {
            this._dictionaryItemBusiness = dictionaryItemBusiness;

        }

        /// <summary>
        /// ���ӽڵ�
        /// </summary>
        /// <returns></returns>
        [HttpPost, CheckApp] //TODO ��Ҫ�ͻ���Ȩ����֤
        public Result CreateDictionaryItem()
        {
            return CommonResult(() => this._dictionaryItemBusiness.CreateDictionaryItem(JObject.Value<string>("name"), JObject.Value<int>("pid")), (r) => Console.WriteLine(r.ToJSON()));
        }

        /// <summary>
        /// ɾ���ڵ�
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, CheckApp] //TODO ��Ҫ�ͻ���Ȩ����֤
        public Result DeleteDictionaryItem()
        {
            return CommonResult(() => this._dictionaryItemBusiness.DeleteDictionaryItem(JObject.Value<int>("id")), (r) => Console.WriteLine(r.ToJSON()));
        }

        /// <summary>
        /// �޸Ľڵ�
        /// </summary>
        /// <returns></returns>
        [HttpPost, CheckApp] //TODO ��Ҫ�ͻ���Ȩ����֤
        public Result UpdateDictionaryItem()
        {
            return CommonResult(() => this._dictionaryItemBusiness.UpdateDictionaryItem(JObject.Value<string>("name"), JObject.Value<int>("id")), (r) => Console.WriteLine(r.ToJSON()));
        }

    }
}