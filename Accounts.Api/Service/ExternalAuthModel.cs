namespace Accounts.Api.Service
{
    public class ExternalAuthModel
    {
        public string Provider { get; set; }
        public string IdToken { get; set; }
        public string AccessToken { get; set; }
    }
    public class GoogleAccessToken

    {

        public string access_token { get; set; }

        public string token_type { get; set; }

        public int expires_in { get; set; }

        public string id_token { get; set; }

        public string refresh_token { get; set; }

    }

    public class GoogleUserOutputData

    {

        public string id { get; set; }

        public string family_name { get; set; }

        public string given_name { get; set; }

        public string email { get; set; }

        public string picture { get; set; }

    }
}
