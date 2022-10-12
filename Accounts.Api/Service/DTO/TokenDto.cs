using System;

namespace Accounts.Api.Service.DTO
{
    // <summary>
    /// Toke model class
    /// </summary>
    public class TokenDto
    {
        /// <summary>
        /// token generated from user authentication
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Expriation time
        /// </summary>
        public DateTime Expiration { get; set; }
    }
}
