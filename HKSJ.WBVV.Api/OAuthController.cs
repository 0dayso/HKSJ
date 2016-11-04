using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Flurl.Http;
using HKSJ.WBVV.Common.Extender;
using HKSJ.WBVV.Common.Language;

namespace HKSJ.WBVV.Api
{
    /// <summary>
    /// ��ȡtoken
    /// TODO ��Կ+���ܴ�+˽Կ(������)
    /// Author : AxOne
    /// </summary>
    [RoutePrefix("api/v1/oauth")]
    public class OAuthController : ApiController
    {
        /// <summary>
        /// ��ȡtoken
        /// </summary>
        /// <returns></returns>
        [Route("token")]
        public async Task<IHttpActionResult> GetTokenAsync(GrantTypeEnum grantType, string clientId, string clientSecret, string account = "", string pwd = "")
        {
            var dict = new Dictionary<string, string>
            {
                {"clientId", clientId}, 
                {"clientSecret", clientSecret}
            };
            switch (grantType)
            {
                case GrantTypeEnum.ClientCredentials:
                    dict.Add("grant_type", "client_credentials");
                    break;
                case GrantTypeEnum.AccountPassword:
                    dict.Add("grant_type", "password");
                    dict.Add("userName", account);
                    dict.Add("passWord", pwd);
                    break;
                default:
                    return Json(new { Success = false, Data = LanguageUtil.Translate("api_Controller_Oauth_GetTokenAsync_Data") });
            }
            var data = await (@"http://" + Request.RequestUri.Authority + @"/token").PostUrlEncodedAsync(dict).ReceiveJson<Token>();
            return Json(new { Success = true, Data = data });
        }

        public class Token
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string expires_in { get; set; }
        }

        /// <summary>
        /// ��֤����
        /// </summary>
        public enum GrantTypeEnum
        {
            /// <summary>
            /// �ͻ�����֤
            /// </summary>
            ClientCredentials = 1,

            /// <summary>
            /// �û�������֤
            /// </summary>
            AccountPassword = 2
        }

    }

    /// <summary>
    /// ��ȡtoken
    /// TODO ��Կ+���ܴ�+˽Կ(������)
    /// Author : AxOne
    /// </summary>
    [RoutePrefix("api/v2/oauth")]
    public class MyOAuthController : ApiController
    {
        /// <summary>
        /// ��ȡtoken
        /// </summary>
        /// <returns></returns>
        [Route("token")]
        public async Task<IHttpActionResult> GetTokenAsync(string account,string pwd)
        {
            //���token
            var dict = new SortedDictionary<string, string>
            {
                {"grant_type", "password"},
                {"username", account},
                {"password", pwd}
            };
            var data = await (@"http://" + Request.RequestUri.Authority + @"/token").WithBasicAuth("wobo", "5bvv").PostUrlEncodedAsync(dict).ReceiveJson<PToken>();
            return Ok(new { IsError = true, Data = data });
        }

        public class PToken
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public string token_type { get; set; }
            public string expires_in { get; set; }
        }

        public class Token
        {
            public string Access_token { get; set; }
            public string Refresh_token { get; set; }
            public string ClientId { get; set; }
            public string UserName { get; set; }
            public DateTime IssuedUtc { get; set; }
            public DateTime ExpiresUtc { get; set; }
        }
    }


}