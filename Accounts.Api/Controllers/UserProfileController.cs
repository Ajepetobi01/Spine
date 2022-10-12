using Accounts.Api.Service;
using Accounts.Api.Service.Contract;
using Accounts.Api.Service.DTO;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly IFacebookAuthService _facebookAuthService;
        private readonly IOptions<AppSettings> appSettings;
        private readonly IJwtHandlerService _jwtHandlerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserProfileController> logger;
        private string GoogleClientId;
        private string GoogleSecretKey;
        private string GoogleRedirectUrl;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="facebookAuthService"></param>
        /// <param name="app"></param>
        /// <param name="jwtHandlerService"></param>
        /// <param name="userManager"></param>
        public UserProfileController(IFacebookAuthService facebookAuthService, IOptions<AppSettings> app, IJwtHandlerService jwtHandlerService,
            UserManager<ApplicationUser> userManager, ILogger<UserProfileController> _logger)
        {
            _facebookAuthService = facebookAuthService;
            appSettings = app;
            _jwtHandlerService = jwtHandlerService;
            _userManager = userManager;
            logger = _logger;
            GoogleClientId = appSettings.Value.GoogleClientId;
            GoogleSecretKey = appSettings.Value.GoogleClientSecretKey;
        }

        #region googlesign

        [Route("google/authentication")]
        [HttpGet]
        public async Task<IActionResult> GoogleAuthentication1()
        {
            //http://www.displaymyhostname.com/
            GoogleRedirectUrl = $"{appSettings.Value.GoogleRedirect_uris}/api/UserProfile/sign-in/google";

            var GetUrl = "https://accounts.google.com/o/oauth2/auth?response_type=code&redirect_uri=" + GoogleRedirectUrl + "&scope=https://www.googleapis.com/auth/userinfo.email%20https://www.googleapis.com/auth/userinfo.profile&client_id=" + GoogleClientId + "";
            //return new RedirectResult(url: "https://accounts.google.com/o/oauth2/auth?response_type=code&redirect_uri=" + GoogleRedirectUrl + "&scope=https://www.googleapis.com/auth/userinfo.email%20https://www.googleapis.com/auth/userinfo.profile&client_id=" + GoogleClientId, permanent: true, preserveMethod: true);
            return new JsonResult(GetUrl);
        }

        /// <summary>
        /// google sign in
        /// </summary>
        /// <param name="externalAuth"></param>
        /// <returns></returns>
        [Route("sign-in/google")]
        [HttpGet]
        public async Task<IActionResult> GoogleSignIn()//ExternalAuthModel externalAuth
        {
            GoogleRedirectUrl = $"{appSettings.Value.GoogleRedirect_uris}/api/UserProfile/sign-in/google";
            var externalAuth = RetrieveGoogleAuthToken(GoogleRedirectUrl);

            var payload = await VerifyGoogleToken(externalAuth);
            var googleUser = GetGoogleUserDetails(externalAuth.AccessToken);
            var user = await _jwtHandlerService.GetOrCreateExternalUserAccount("google", payload.Subject, payload.Email, googleUser.given_name, googleUser.family_name, AppConstant.PublicUserRole);
            var token = await _jwtHandlerService.GenerateToken(user);
            return new JsonResult(token);
        }
        private async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(ExternalAuthModel externalAuth)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { appSettings.Value.GoogleClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(externalAuth.IdToken, settings);
                return payload;
            }
            catch (Exception ex)
            {
                var errorMesage = FormatException(ex);
                throw new Exception(errorMesage);
            }
        }
        private GoogleUserOutputData GetGoogleUserDetails(string accessToken)
        {
            try
            {
                HttpClient client = new HttpClient();
                var urlProfile = "https://www.googleapis.com/oauth2/v1/userinfo?access_token=" + accessToken;
                client.CancelPendingRequests();
                HttpResponseMessage output = client.GetAsync(urlProfile).Result;
                if (output.IsSuccessStatusCode)
                {
                    string outputData = output.Content.ReadAsStringAsync().Result;
                    var googleUser = JsonConvert.DeserializeObject<GoogleUserOutputData>(outputData);
                    return googleUser;
                }
                else
                {
                    var result = output.Content.ReadAsStringAsync().Result;
                    var apiResponseModel0 = JsonConvert.DeserializeObject<GoogleUserOutputData>(result);
                    return apiResponseModel0;
                }

            }

            catch (Exception ex)
            {
                var errorMesage = FormatException(ex);
                throw new Exception(errorMesage);
            }
        }
        private ExternalAuthModel RetrieveGoogleAuthToken(string redirectUrl)
        {
            try
            {
                var url = Request.GetEncodedPathAndQuery();
                if (url != "")
                {
                    string queryString = url.ToString();
                    char[] delimiterChars = { '=' };
                    string[] words = queryString.Split(delimiterChars);
                    string code = words[1];
                    if (code != null)
                    {
                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://accounts.google.com/o/oauth2/token");
                        webRequest.Method = "POST";

                        //GoogleClientId = "306668221210-b4u9pi90n8jm2gimt15imsn83gamgu3v.apps.googleusercontent.com";
                        //GoogleSecretKey = "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";
                        string Parameters = "code=" + code + "&client_id=" + GoogleClientId + "&client_secret=" + GoogleSecretKey
                            + "&redirect_uri=" + redirectUrl + "&grant_type=authorization_code";
                        byte[] byteArray = Encoding.UTF8.GetBytes(Parameters);
                        webRequest.ContentType = "application/x-www-form-urlencoded";
                        webRequest.ContentLength = byteArray.Length;
                        Stream postStream = webRequest.GetRequestStream();

                        // Add the post data to the web request
                        postStream.Write(byteArray, 0, byteArray.Length);
                        postStream.Close();
                        WebResponse response = webRequest.GetResponse();
                        postStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(postStream);
                        string responseFromServer = reader.ReadToEnd();
                        GoogleAccessToken serStatus = JsonConvert.DeserializeObject<GoogleAccessToken>(responseFromServer);
                        if (serStatus != null)
                        {
                            return new ExternalAuthModel { AccessToken = serStatus.access_token, IdToken = serStatus.id_token, Provider = "Google" };
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                // todo: log error;
                logger.Log(LogLevel.Error, ex, "Error in RetrieveGoogleAuthToken");
                RedirectToAction("Error", "Home");
            }
            return null;
        }

        #endregion
        /// <summary>
        /// facebook sign in
        /// </summary>
        /// <param name="externalAuth"></param>
        /// <returns></returns>
        [Route("sign-in/facebook")]
        [HttpPost]
        public async Task<IActionResult> FacebookSignIn(ExternalAuthModel externalAuth)
        {
            var validateTokenResult = await _facebookAuthService.ValidateAccessTokenAsync(externalAuth.IdToken);

            if (!validateTokenResult.Data.IsValid)
                return BadRequest("Invalid user"); // todo: return right error

            var userInfo = await _facebookAuthService.GetUserInfoAsync(externalAuth.IdToken);

            var user = await _jwtHandlerService.GetOrCreateExternalUserAccount("facebook", userInfo.Id, userInfo.Email, userInfo.FirstName, userInfo.LastName, AppConstant.PublicUserRole);
            var token = await _jwtHandlerService.GenerateToken(user);
            return new JsonResult(token);
        }
        

        public static string FormatException(Exception ex)
        {
            var message = ex.Message;

            //Add the inner exception if present (showing only the first 50 characters of the first exception)
            if (ex.InnerException == null) return message;
            if (message.Length > 50)
                message = message.Substring(0, 50);

            message += "...->" + ex.InnerException.Message + "...->";

            if (ex.InnerException.InnerException != null)
            {
                message += "...->" + ex.InnerException.Message + "...->" + ex.InnerException.InnerException.Message;
            }
            return message;
        }
    }

}
