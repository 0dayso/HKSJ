
using System;
using System.Data;
using System.Collections.Generic;
using System.Web.Http;
using HKSJ.WBVV.Api.Base;
using HKSJ.WBVV.Api.Filters;
using HKSJ.WBVV.Business.Interface;
using HKSJ.WBVV.Common.Extender;
using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Entity.ViewModel;
using HKSJ.WBVV.Entity.ViewModel.Client;
using HKSJ.WBVV.Entity.ViewModel.Manage;
using HKSJ.WBVV.Repository.Interface;
using HKSJ.WBVV.Entity.ApiModel;
using Newtonsoft.Json.Linq;

namespace HKSJ.WBVV.Api
{
    public class CategoryController : ApiControllerBase
    {
        private readonly ICategoryBusiness _icategoryBusiness;
        public CategoryController(ICategoryBusiness icategoryBusiness)
        {
            this._icategoryBusiness = icategoryBusiness;

        }
        /// <summary> 
        /// ��ȡ��������˵�
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IList<MenuView> GetMenuViewList()
        {
            return this._icategoryBusiness.GetMenuViewList();
        }
        /// <summary>
        /// ��ȡ����ʾ��������˵�
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IList<MenuView> GetMenuViewListVisible()
        {
            return this._icategoryBusiness.GetMenuViewListVisible();
        }

        /// <summary>
        /// һ�������б�
        /// </summary>
        /// <returns></returns>
        public IList<HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView> GetOneCategoryViewList()
        {
            return this._icategoryBusiness.GetOneCategoryViewList();
        }

        /// <summary>
        /// ��̨������༯��
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IList<HKSJ.WBVV.Entity.ViewModel.Manage.CategoryView> GetCategoryViewList()
        {
            return this._icategoryBusiness.GetCategoryViewList();
        }
        /// <summary>
        /// ɾ������
        /// </summary>
        /// <returns></returns>
        [HttpPost, CheckApp] //TODO ��Ҫ�ͻ���Ȩ����֤
        public Result DeleteCategory()
        {
            return CommonResult(() => this._icategoryBusiness.DeleteCategory(JObject.Value<int>("id")), (r) => Console.WriteLine(r.ToJSON()));
        }

        /// <summary>
        /// ��ӷ���
        /// </summary>
        /// <returns></returns>
        [HttpPost, CheckApp] //TODO ��Ҫ�ͻ���Ȩ����֤
        public Result CreateCategory()
        {
            return CommonResult(() => this._icategoryBusiness.CreateCategory(JObject.Value<string>("name"), JObject.Value<int>("pid")), (r) => Console.WriteLine(r.ToJSON()));
        }

        /// <summary>
        /// �޸ķ���
        /// </summary>
        /// <returns></returns>
        [HttpPost, CheckApp] //TODO ��Ҫ�ͻ���Ȩ����֤
        public Result UpdateCategory()
        {
            return CommonResult(() => this._icategoryBusiness.UpdateCategory(JObject.Value<string>("name"), JObject.Value<int>("id")), (r) => Console.WriteLine(r.ToJSON()));
        }

        [HttpGet]
        public Result GetParentId(int cid)
        {
            return CommonResult(() => this._icategoryBusiness.GetParentId(cid), (r) => Console.WriteLine(r.ToJSON()));
        }
        [HttpGet]
        public dynamic GetParentInfo(int cid)
        {
            Category category = this._icategoryBusiness.GetParentInfo(cid);
            return new { msg = "success", categorydata = category };
        }

        
    }
}