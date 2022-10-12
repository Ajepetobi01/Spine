using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Common.Helper
{
    public class ApiUrlHelper
    {
        public string ApiBase { get; set; }
        public ApiUrlHelper()
        {
            ApiBase = "/api/";
        }

        // user profile controller
        public string UserProfileGoogleSignIn() { return $"{ApiBase}user-profile/sign-in/google"; }
        public string UserProfileFacebookSignIn() { return $"{ApiBase}user-profile/sign-in/facebook"; }
    }
}
