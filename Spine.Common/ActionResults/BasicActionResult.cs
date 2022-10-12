using System.Net;
using System.Text.Json.Serialization;

namespace Spine.Common.ActionResults
{
    public class BasicActionResult
    {
        [JsonIgnore]
        public HttpStatusCode Status { get; set; }
        [JsonIgnore]
        public string ErrorMessage { get; set; }
        public string Message { get; set; }

        public BasicActionResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
            Status = HttpStatusCode.BadRequest;
        }
        public BasicActionResult(string message, HttpStatusCode statusCode)
        {
            Message = message;
            Status = statusCode;
        }

        public BasicActionResult()
        {
            Status = HttpStatusCode.OK;
        }

        public BasicActionResult(HttpStatusCode status)
        {
            Status = status;
        }
    }
}
