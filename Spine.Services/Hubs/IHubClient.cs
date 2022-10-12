using System;
using System.Threading.Tasks;

namespace Spine.Services.Hubs
{
    public interface IHubClient
    {
        Task Notify(Guid userId);
    }
}
