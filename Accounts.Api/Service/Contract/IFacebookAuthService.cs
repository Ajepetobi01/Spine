using Accounts.Api.Service.DTO;
using System.Threading.Tasks;

namespace Accounts.Api.Service.Contract
{
    public interface IFacebookAuthService
    {
        Task<FacebookTokenValidationResult> ValidateAccessTokenAsync(string accessToken);

        Task<FacebookUserInfoResult> GetUserInfoAsync(string accessToken);
    }
}
