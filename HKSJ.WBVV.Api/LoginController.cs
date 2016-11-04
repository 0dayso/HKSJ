using System;
using System.Collections.Generic;
using System.Web.Http;
using HKSJ.WBVV.Api.Base;
using HKSJ.WBVV.Api.Filters;
using HKSJ.WBVV.Business.Interface;
using HKSJ.WBVV.Common.Extender;
using HKSJ.WBVV.Entity.ApiModel;

namespace HKSJ.WBVV.Api
{
    /// <summary>
    /// ��¼
    /// Author : AxOne
    /// </summary>
    public class LoginController : ApiControllerBase
    {
        private readonly IUserBusiness _userBusiness;

        public LoginController(IUserBusiness iuserBusiness)
        {
            this._userBusiness = iuserBusiness;
        }

        [HttpPost, CheckApp]
        //[Authorize] // TODO ��Ҫ�ͻ���Ȩ����֤
        public Result UserLogin()
        {
            this._userBusiness.IpAddress = IpAddress;
            return CommonResult(() => _userBusiness.LoginUser(JObject.Value<string>("account"), JObject.Value<string>("pwd")), r => Console.WriteLine(r.ToJSON()));
        }

        [HttpPost, CheckApp]
        //[Authorize] // TODO ��Ҫ�ͻ���Ȩ����֤
        public Result UserLoginById()
        {
            this._userBusiness.IpAddress = IpAddress;
            return CommonResult(() => _userBusiness.LoginUser(JObject.Value<int>("userId")), r => Console.WriteLine(r.ToJSON()));
        }

        [HttpPost, CheckApp]
        //[Authorize] // TODO ��Ҫ�ͻ���Ȩ����֤
        public Result CheckPwd()
        {
            return CommonResult(() => _userBusiness.CheckPwd(JObject.Value<int>("uid"), JObject.Value<string>("opwd")), r => Console.WriteLine(r.ToJSON()));
        }

        [HttpPost, CheckApp]
        //[Authorize] // TODO ��Ҫ�ͻ���Ȩ����֤
        public Result CheckPhone()
        {
            return CommonResult(() => _userBusiness.CheckPhone(JObject.Value<int>("uid"), JObject.Value<string>("ophone")), r => Console.WriteLine(r.ToJSON()));
        }

        [HttpPost, CheckLogin]
        public Result CheckNickName()
        {
            return CommonResult(() => _userBusiness.CheckNickName(JObject.Value<int>("uid"), JObject.Value<string>("nickName")), r => Console.WriteLine(r.ToJSON()));
        }


        #region �������󶨡���¼����

        [HttpPost, CheckApp]
        //[Authorize] // TODO ��Ҫ�ͻ���Ȩ����֤
        public Result ThirdPartyBindAndLogin()
        {
            this._userBusiness.IpAddress = IpAddress;
            return CommonResult(() => _userBusiness.ThirdPartyBindAndLogin(JObject.Value<string>("account"), JObject.Value<string>("pwd"), JObject.Value<string>("typeCode"), JObject.Value<string>("relatedId"), JObject.Value<string>("nickName"), JObject.Value<string>("figureURL")), r => Console.WriteLine(r.ToJSON()));
        }

        #endregion

    }
}