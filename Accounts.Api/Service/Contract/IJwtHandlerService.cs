using Accounts.Api.Service.DTO;
using Spine.Data.Entities;
using System.Threading.Tasks;

namespace Accounts.Api.Service.Contract
{
    public interface IJwtHandlerService
    {
        /// <summary>
        /// Generate token for user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<TokenDto> GenerateToken(ApplicationUser user);

        /// <summary>
        /// Get user by provider and key. Create new user if user doesn't exist
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="key"></param>
        /// <param name="email"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        Task<ApplicationUser> GetOrCreateExternalUserAccount(string provider, string key, string email, string firstName, string lastName, string role);

        /// <summary>
        /// Get logged in user id from token
        /// </summary>
        /// <returns></returns>
        string GetLoggedInUserId();

        /// <summary>
        /// Get logged in user details
        /// </summary>
        /// <returns></returns>
        Task<ApplicationUser> GetLoggedInUser();
    }
}
