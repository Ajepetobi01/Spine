using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spine.Core.Subscription.Interface;
using Spine.Data.Entities.Subscription;
using Spine.Payment.Flutterwave.Transactions;
using Spine.Payment.Paystack.Transactions;
using Subscription.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Spine.Core.Subscription.ViewModel;

namespace Subscription.Api.Controllers
{
    [Route("api/v1")]
    [ApiController]
    [AllowAnonymous]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionRepository _service;
        private readonly ILogger<SubscriptionController> _logger;
        private readonly IConfiguration config;

        public SubscriptionController(ISubscriptionRepository service, ILogger<SubscriptionController> logger, IConfiguration config)
        {
            _service = service;
            _logger = logger;
            this.config = config;
        }

        [Route("plan/{planId}")]
        [HttpGet]
        public IActionResult Plan(int planId)
        {
            var exMessage = string.Empty;
            try
            {
                object products = null;

                if (planId > 0)
                {
                    products = _service.GetPlans()
                        .Where(p => p.PlanId == planId).FirstOrDefault();
                }

                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = products
                });


            }
            catch (Exception)
            {
                exMessage = $"Internal Server Error";
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = exMessage
                });
            }
        }

        [Route("all/plans")]
        [HttpGet]
        public IActionResult PlanList()
        {
            var exMessage = string.Empty;
            try
            {
                object products = null;

                products = _service.GetPlans();

                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = products
                });


            }
            catch (Exception ex)
            {
                exMessage = $"Internal Server Error";
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = exMessage
                });
            }
        }

    }
}
