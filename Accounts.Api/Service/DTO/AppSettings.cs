namespace Accounts.Api.Service.DTO
{
    public class AppConstant
    {
        public const string AdminRole = "Admin";
        public const string BusinessUserRole = "BusinessUser";
        public const string PublicUserRole = "PublicUser";
        public const string AccessToken = "access_token";
    }
    public class AppSettings
    {
        public string AdminEmail { get; set; }
        public string AdminPassword { get; set; }
        public string AppEmail { get; set; }
        public string AppEmailPassword { get; set; }
        public string ContactUsEmail { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string GoogleClientId { get; set; }
        public string GoogleClientSecretKey { get; set; }
        public string GoogleRedirect_uris { get; set; }
        public string FacebookAppId { get; set; }
        public string FacebookSecretKey { get; set; }
    }
}
