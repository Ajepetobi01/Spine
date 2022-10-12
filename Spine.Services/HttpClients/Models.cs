using System.Net;

namespace Spine.Services.HttpClients
{
    public interface IRequestModel { }


    public interface IApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
    }

    public class ApiErrorModel : IApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ApiSuccessModel<T> : IApiResponse
    {
        public T Model { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }

    public class ApiSuccessModel : IApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
    }

}
