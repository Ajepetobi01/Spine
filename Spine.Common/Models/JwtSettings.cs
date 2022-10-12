using System;
namespace Spine.Common.Models
{
    public class JwtSettings
    {
        public string Subject { get; set; }
        public string Audience { get; set; }
        
        public string Audiences { get; set; }
        public string Issuer { get; set; }
        public string Key { get; set; }

        public int DurationInMinutes { get; set; }
    }

    public class UserToken
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedByIp { get; set; }
    }
}
