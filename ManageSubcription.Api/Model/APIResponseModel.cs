using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Model
{
    public class APIResponseModel
    {
        public int statusCode { get; set; } = 200;

        public bool hasError { get; set; } = false;

        public string message { get; set; } = string.Empty;

        public object data { get; set; }
    }
}
