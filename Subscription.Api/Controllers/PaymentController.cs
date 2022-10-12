using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spine.Core.Subscription.Interface;
using Spine.Data.Entities.Subscription;
using Spine.Payment.Flutterwave.Transactions;
using Spine.Payment.Paystack.Transactions;
using Subscription.Api.Model;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Spine.Core.Subscription.ViewModel;

namespace Subscription.Api.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ISubscriptionRepository _service;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration config;

        public PaymentController(ISubscriptionRepository service, ILogger<PaymentController> logger, IConfiguration config)
        {
            _service = service;
            _logger = logger;
            this.config = config;
        }

        //subscription
        [Route("transaction")]
        [HttpPost]
        public async Task<IActionResult> InitTransaction([FromBody] PaymentMethodViewMode model)
        {
            if (model == null) return Ok(new APIResponseModel
            {
                hasError = true,
                statusCode = (int)HttpStatusCode.BadRequest,
                message = "Bad Request"
            });

            var errorMessage = string.Empty;

            if (model.CompanyId == Guid.Empty)
                errorMessage = "Valide Company Id required";

            if (model.PlanId <= 0)
                errorMessage = "Valide Plan Id required";

            if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
            {
                hasError = true,
                statusCode = (int)HttpStatusCode.BadRequest,
                message = errorMessage
            });



            var plan = _service.GetPlans()
                        .Where(p => p.PlanId == model.PlanId).FirstOrDefault();


            if (plan == null)
            {
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Invalid plan Id"
                });
            }

            var user = _service.CompanyById(model.CompanyId).FirstOrDefault();
            if (user == null)
            {
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Invalid company Id"
                });
            }

            if (user.ID_Subscription > 0)
            {
                var checkIfUserSubscriptionStillActive = _service.GetTransactionByCompanyId(user.ID_Subscription).FirstOrDefault();
                if (checkIfUserSubscriptionStillActive != null)
                {
                    if (checkIfUserSubscriptionStillActive.ExpiredDate >= DateTime.Now && checkIfUserSubscriptionStillActive.PaymentStatus == true)
                    {
                        return Ok(new APIResponseModel
                        {
                            hasError = true,
                            statusCode = (int)HttpStatusCode.BadRequest,
                            message = "You still have active plan till " + checkIfUserSubscriptionStillActive.ExpiredDate.Value.ToString("dd/MM/yyyy")
                        });
                    }
                }
            }

            //FreePlan
            if (plan.IsFreePlan)
            {
                var company = new CompanySubscription()
                {
                    ID_Company = model.CompanyId,
                    ID_Plan = model.PlanId,
                    PlanType = plan.PlanName,
                    Amount = 0,
                    PaymentStatus = true,
                    IsActive = true,
                    TransactionRef = "Free Plan",
                    PaymentMethod = "Free Plan",
                    TransactionDate = DateTime.Now,
                    ExpiredDate = DateTime.Now.AddMonths(plan.PlanDuration.Value)
                };

                var updateCompany = await _service.AddOnlineDepositOrder(company);
                if (updateCompany)
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        message = "success"
                    });

                }
            }
            else
            {

                if (string.IsNullOrEmpty(model.method))
                    errorMessage = "Payment method must either be PSTACK or RAVE";

                if (model.method.ToUpper().Trim() != "PSTACK" && model.method.ToUpper().Trim() != "RAVE")
                    errorMessage = "Payment method must either be PSTACK or RAVE";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                var amount = decimal.Parse(model.Amount);

                if (amount < 0 || amount == 0)
                {
                    if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Amount must be greated than zero"
                    });
                }

                //Check if admin is runing promo code or there is referral discount
                if (_service.isPromoCodeEnable())
                {
                    if (model.PromoCodeId != Guid.Empty && model.PromoCodeId != null)
                    {
                        if (_service.isPromoCodeValid(model.PromoCodeId.Value))
                        {
                            errorMessage = $"Promo Code for {model.PromoCodeId.Value} already used";
                        }
                        plan.Amount = Math.Round(_service.ValidPromoCodeAmount(model.PromoCodeId.Value), 2);
                        model.Amount = Math.Round(_service.ValidPromoCodeAmount(model.PromoCodeId.Value)).ToString();
                    }
                }
                

                //if referralcode promo enable
                if (_service.isReferralCodeEnable())
                {
                    if (!string.IsNullOrWhiteSpace(user.Ref_ReferralCode))
                    {
                        var referralmodel = new ReferralViewModel
                        {
                            CompanyId = model.CompanyId,
                            PlanId = model.PlanId,
                            ReferralCode = user.Ref_ReferralCode
                        };
                        var response = _service.ReferralCode(referralmodel);
                        model.ReferralCodeId = response.ReferralCodeId;
                        plan.Amount = Math.Round(response.Amount, 2);
                        model.Amount = Math.Round(response.Amount).ToString();
                    }
                }

                if (model.PromoCodeId == Guid.Empty || model.PromoCodeId == null && !_service.isReferralCodeEnable())
                {
                    if (!Math.Round(amount, 2).Equals(Math.Round(plan.Amount, 2)))
                    {
                        return Ok(new APIResponseModel
                        {
                            hasError = true,
                            statusCode = (int)HttpStatusCode.BadRequest,
                            message = "Amount for this plan is " + plan.Amount.ToString("#,##0.00;(#,##0.00)")
                        });
                    }
                }
                var CallbackUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}/api/v1/verify";
                var targetUsr = _service.CompanyById(model.CompanyId).FirstOrDefault();

                switch (model.method.ToUpper())
                {
                    case "PSTACK":

                        //var paystackResponse = await PaystackInitializeDepositTransaction(model);
                        //return Ok(paystackResponse);


                        var intAmount = int.Parse(model.Amount);


                        if (amount == 0)
                        {
                            return Ok(new APIResponseModel
                            {
                                hasError = true,
                                statusCode = (int)HttpStatusCode.BadRequest,
                                message = "Amount must be greated than zero"
                            });
                        }


                        var Paymodel = new PayInitModel()
                        {
                            amount = (intAmount * 100),
                            email = targetUsr.Email,
                            callbackUrl = CallbackUrl
                        };


                        var paystackKey = Convert.ToBoolean(config.GetValue<bool>("InDevelopment")) == true
                            ? config.GetValue<string>("Paystack:TestSecretKey") : config.GetValue<string>("Paystack:LiveSecretKey");

                        if (string.IsNullOrWhiteSpace(paystackKey))
                        {
                            return Ok(new APIResponseModel
                            {
                                hasError = true,
                                statusCode = (int)HttpStatusCode.BadRequest,
                                message = "Invaild Key"
                            });
                        }

                        var paystackAPI = new PaystackPayment(paystackKey, Paymodel.callbackUrl);

                        var response = await paystackAPI.InitializeTransaction(Paymodel.email, Paymodel.amount);

                        if (response.status)
                        {
                            //var plan = _service.GetPlans()
                            //        .Where(p => p.PlanId == param.PlanId).FirstOrDefault();


                            var company = new CompanySubscription()
                            {
                                ID_Company = model.CompanyId,
                                ID_Plan = model.PlanId,
                                PlanType = plan.PlanName,
                                Amount = plan.Amount,
                                PaymentStatus = false,
                                TransactionRef = response.data.reference,
                                PaymentMethod = model.method,
                                TransactionDate = DateTime.Now,
                                ExpiredDate = DateTime.Now.AddMonths(plan.PlanDuration.Value)
                            };
                            var updateCompany = await _service.AddOnlineDepositOrder(company);
                            if (updateCompany)
                            {

                                if (model.PromoCodeId != Guid.Empty && model.PromoCodeId != null)
                                {
                                    await _service.UpdatePromoCodeTransactionRef(model.PromoCodeId.Value, response.data.reference);
                                }

                                if (model.ReferralCodeId != Guid.Empty && model.ReferralCodeId != null)
                                {
                                    await _service.UpdateReferralCodeTransactionRef(model.ReferralCodeId.Value, response.data.reference);
                                }

                                var authUrl = response.data.authorization_url;

                                return Ok(new APIResponseModel
                                {
                                    hasError = false,
                                    statusCode = (int)HttpStatusCode.OK,
                                    message = authUrl
                                });

                            }
                            else
                            {
                                return Ok(new APIResponseModel
                                {
                                    hasError = true,
                                    statusCode = (int)HttpStatusCode.InternalServerError,
                                    message = "There was an error initializing transaction"
                                });
                            }


                        }

                        return Ok(new APIResponseModel
                        {
                            hasError = true,
                            statusCode = (int)HttpStatusCode.InternalServerError,
                            message = "There was an error initializing transaction"
                        });

                    case "RAVE":

                        //var raveResponse = await RaveInitializeDepositTransaction(model);
                        //return Ok(raveResponse);

                        try
                        {
                            var secretKey = Convert.ToBoolean(config.GetValue<bool>("InDevelopment")) == true
                            ? config.GetValue<string>("Flutterwave:TestSecretKey") : config.GetValue<string>("Flutterwave:LiveSecretKey");


                            var baseUrl = config.GetValue<string>("AppBaseUrl");


                            //var RaveCallbackUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}/api/Payment/verify";

                            var raveAPI = new RavePayment(secretKey, CallbackUrl);

                            //var customerInfo = await _service.FetchCustomerByID(param.CustomerId);
                            var trx_ref = DateTime.Now.Ticks.ToString();

                            //var plan = _service.GetPlans()
                            //        .Where(p => p.PlanId == param.PlanId).FirstOrDefault();

                            var company = new CompanySubscription()
                            {
                                ID_Company = model.CompanyId,
                                ID_Plan = model.PlanId,
                                PlanType = plan.PlanName,
                                Amount = plan.Amount,
                                PaymentStatus = false,
                                TransactionRef = trx_ref,
                                PaymentMethod = model.method,
                                TransactionDate = DateTime.Now,
                                ExpiredDate = DateTime.Now.AddMonths(plan.PlanDuration.Value)
                            };

                            var order = await _service.AddOnlineDepositOrder(company);
                            if (order)
                            {
                                //var targetUsr = _service.CompanyById(param.CompanyId).FirstOrDefault();
                                var raveInitResponse = await raveAPI.InitializeTransaction(trx_ref, model.Amount, targetUsr.Email, targetUsr.PhoneNumber,
                                    targetUsr.Name);

                                if (!raveInitResponse.status.Equals("success"))
                                    return Ok(new APIResponseModel { hasError = true, statusCode = (int)HttpStatusCode.InternalServerError, message = "" });

                                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                                Response.Headers.Append("Access-Control-Allow-Origin", "*");

                                if (model.PromoCodeId != Guid.Empty && model.PromoCodeId != null)
                                {
                                    await _service.UpdatePromoCodeTransactionRef(model.PromoCodeId.Value, trx_ref);
                                }

                                if (model.ReferralCodeId != Guid.Empty && model.ReferralCodeId != null)
                                {
                                    await _service.UpdateReferralCodeTransactionRef(model.ReferralCodeId.Value, trx_ref);
                                }

                                var authUrl = raveInitResponse.data.link;

                                return Ok(new APIResponseModel
                                {
                                    hasError = false,
                                    statusCode = (int)HttpStatusCode.OK,
                                    message = authUrl
                                });
                            }

                        }
                        catch (Exception e)
                        {
                            return Ok(new APIResponseModel
                            {
                                hasError = true,
                                statusCode = (int)HttpStatusCode.InternalServerError,
                                message = "There was an error initializing transaction"
                            });
                        }
                        return Ok(new APIResponseModel
                        {
                            hasError = true,
                            statusCode = (int)HttpStatusCode.InternalServerError,
                            message = "There was an error initializing transaction"
                        });

                    default:
                        break;

                }

                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = "There was an error initializing transaction"
                });
            }
            return Ok(new APIResponseModel
            {
                hasError = true,
                statusCode = (int)HttpStatusCode.InternalServerError,
                message = "There was an error initializing transaction"
            });

        }
        private async Task<IActionResult> RaveInitializeDepositTransaction(PaymentMethodViewMode param)
        {

            try
            {
                var secretKey = Convert.ToBoolean(config.GetValue<bool>("InDevelopment")) == true
                ? config.GetValue<string>("Flutterwave:TestSecretKey") : config.GetValue<string>("Flutterwave:LiveSecretKey");


                var baseUrl = config.GetValue<string>("AppBaseUrl");


                var CallbackUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}/api/Payment/verify";

                var raveAPI = new RavePayment(secretKey, CallbackUrl);

                //var customerInfo = await _service.FetchCustomerByID(param.CustomerId);
                var trx_ref = DateTime.Now.Ticks.ToString();

                var plan = _service.GetPlans()
                        .Where(p => p.PlanId == param.PlanId).FirstOrDefault();

                var company = new CompanySubscription()
                {
                    ID_Company = param.CompanyId,
                    ID_Plan = param.PlanId,
                    PlanType = plan.PlanName,
                    Amount = decimal.Parse(param.Amount),
                    PaymentStatus = false,
                    IsActive = false,
                    TransactionRef = trx_ref,
                    PaymentMethod = param.method,
                    TransactionDate = DateTime.Now,
                    ExpiredDate = DateTime.Now.AddMonths(plan.PlanDuration.Value)
                };

                var order = await _service.AddOnlineDepositOrder(company);
                if (order)
                {
                    var targetUsr = _service.CompanyById(param.CompanyId).FirstOrDefault();
                    var raveInitResponse = await raveAPI.InitializeTransaction(trx_ref, param.Amount, targetUsr.Email, targetUsr.PhoneNumber,
                        targetUsr.Name);

                    if (!raveInitResponse.status.Equals("success"))
                        return Ok(new APIResponseModel { hasError = true, statusCode = (int)HttpStatusCode.InternalServerError, message = "" });

                    Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    Response.Headers.Append("Access-Control-Allow-Origin", "*");

                    var authUrl = raveInitResponse.data.link;

                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        message = authUrl
                    });
                }

            }
            catch (Exception e)
            {

            }
            return Ok(new APIResponseModel
            {
                hasError = true,
                statusCode = (int)HttpStatusCode.InternalServerError,
                message = "There was an error initializing transaction"
            });
        }
        private async Task<IActionResult> PaystackInitializeDepositTransaction(PaymentMethodViewMode param)
        {

            var amount = int.Parse(param.Amount);


            if (amount == 0)
            {
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Amount must be greated than zero"
                });
            }


            var CallbackUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}/api/v1/verify";

            var targetUsr = _service.CompanyById(param.CompanyId).FirstOrDefault();

            var model = new PayInitModel()
            {
                amount = (amount * 100),
                email = targetUsr.Email,//"noble4help@gmail.com",
                callbackUrl = CallbackUrl
            };


            var paystackKey = Convert.ToBoolean(config.GetValue<bool>("InDevelopment")) == true
                ? config.GetValue<string>("Paystack:TestSecretKey") : config.GetValue<string>("Paystack:LiveSecretKey");

            if (string.IsNullOrWhiteSpace(paystackKey))
            {
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Invaild Key"
                });
            }

            var paystackAPI = new PaystackPayment(paystackKey, model.callbackUrl);

            var response = await paystackAPI.InitializeTransaction(model.email, model.amount);

            if (response.status)
            {
                var plan = _service.GetPlans()
                        .Where(p => p.PlanId == param.PlanId).FirstOrDefault();


                var company = new CompanySubscription()
                {
                    ID_Company = param.CompanyId,
                    ID_Plan = param.PlanId,
                    PlanType = plan.PlanName,
                    Amount = model.amount,
                    PaymentStatus = false,
                    IsActive = false,
                    TransactionRef = response.data.reference,
                    PaymentMethod = param.method,
                    TransactionDate = DateTime.Now,
                    ExpiredDate = DateTime.Now.AddMonths(plan.PlanDuration.Value)
                };
                var updateCompany = await _service.AddOnlineDepositOrder(company);
                if (updateCompany)
                {
                    var authUrl = response.data.authorization_url;

                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        message = authUrl
                    });

                }
                else
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.InternalServerError,
                        message = "There was an error initializing transaction"
                    });
                }


            }

            return Ok(new APIResponseModel
            {
                hasError = true,
                statusCode = (int)HttpStatusCode.InternalServerError,
                message = "There was an error initializing transaction"
            });
        }
        [Route("verify")]
        [HttpGet]
        public async Task<IActionResult> Confirmation()//[FromQuery] string reference
        {
            var tranxRef = HttpContext.Request.Query["reference"].ToString();

            var paystackKey = Convert.ToBoolean(config.GetValue<bool>("InDevelopment")) == true
                ? config.GetValue<string>("Paystack:TestSecretKey") : config.GetValue<string>("Paystack:LiveSecretKey");

            var paystackAPI = new PaystackPayment(paystackKey, string.Empty);

            var response = await paystackAPI.VerifyTransaction(tranxRef);

            if (response.status && response.data.status == "success")
            {
                var amountPaid = (decimal)(response.data.amount / 100); //Always divide returned amount by 100 to get 
                var transactionRef = response.data.reference;
                if (await _service.UpdateOnlineDepositOrder(transactionRef))
                {
                    await _service.UpdatePromoCode(null, response.data.reference);
                    await _service.UpdateReferralCode(null, response.data.reference);

                    var subscription = await _service.GetSubscription(transactionRef);


                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        message = response.data.status,
                        data = subscription,
                    });
                }

            }

            else

            {

                var tx_ref = HttpContext.Request.Query["tx_ref"];
                var transaction_id = HttpContext.Request.Query["transaction_id"];
                var transStatus = HttpContext.Request.Query["status"];

                if (string.IsNullOrWhiteSpace(transStatus) || string.IsNullOrWhiteSpace(transaction_id)
                     || string.IsNullOrWhiteSpace(tx_ref))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.InternalServerError,
                        message = "Invalid transaction id"
                    });
                }

                var secretKey = Convert.ToBoolean(config.GetValue<bool>("InDevelopment")) == true
                ? config.GetValue<string>("Flutterwave:TestSecretKey") : config.GetValue<string>("Flutterwave:LiveSecretKey");

                var raveAPI = new RavePayment(secretKey);

                var transactionId = int.Parse(transaction_id);

                var respons = await raveAPI.VerifyTransaction(transactionId);

                if (!respons.status.Equals("success"))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Error occur"
                    });
                }

                //Code after payment here
                if (await _service.UpdateOnlineDepositOrder(tx_ref))
                {
                    await _service.UpdatePromoCode(null, tx_ref);
                    await _service.UpdateReferralCode(null, tx_ref);

                    var subscription = await _service.GetSubscription(tx_ref);

                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        message = respons.status,
                        data = subscription,
                    });
                }
            }
            return Ok(new APIResponseModel
            {
                hasError = false,
                statusCode = (int)HttpStatusCode.OK,
                message = "There an error processing your subscription"
            });
        }
        [Route("re-verifypayment")]
        [HttpGet]
        public async Task<IActionResult> Verifypayment([FromQuery] string reference)
        {
            var tranxRef = reference;

            var paystackKey = Convert.ToBoolean(config.GetValue<bool>("InDevelopment")) == true
                ? config.GetValue<string>("Paystack:TestSecretKey") : config.GetValue<string>("Paystack:LiveSecretKey");

            var paystackAPI = new PaystackPayment(paystackKey, string.Empty);

            var response = await paystackAPI.VerifyTransaction(tranxRef);

            if (response.status && response.data.status == "success")
            {
                var amountPaid = (decimal)(response.data.amount / 100); //Always divide returned amount by 100 to get 
                var transactionRef = response.data.reference;
                if (await _service.UpdateOnlineDepositOrder(transactionRef))
                {
                    await _service.UpdatePromoCode(null, response.data.reference);
                    await _service.UpdateReferralCode(null, response.data.reference);

                    var subscription = await _service.GetSubscription(transactionRef);

                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        message = response.data.status,
                        data = subscription,
                    });
                }

            }

            else

            {

                var tx_ref = reference;
                var transaction_id = HttpContext.Request.Query["transaction_id"];
                var transStatus = HttpContext.Request.Query["status"];

                if (string.IsNullOrWhiteSpace(transStatus) || string.IsNullOrWhiteSpace(transaction_id)
                     || string.IsNullOrWhiteSpace(tx_ref))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.InternalServerError,
                        message = "Error occur"
                    });
                }

                var secretKey = Convert.ToBoolean(config.GetValue<bool>("InDevelopment")) == true
                ? config.GetValue<string>("Flutterwave:TestSecretKey") : config.GetValue<string>("Flutterwave:LiveSecretKey");

                var raveAPI = new RavePayment(secretKey);

                var transactionId = int.Parse(transaction_id);

                var respons = await raveAPI.VerifyTransaction(transactionId);

                if (!respons.status.Equals("success"))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Error occur"
                    });
                }

                //Code after payment here
                if (await _service.UpdateOnlineDepositOrder(tx_ref))
                {
                    await _service.UpdatePromoCode(null, tx_ref);
                    await _service.UpdateReferralCode(null, tx_ref);

                    var subscription = await _service.GetSubscription(tx_ref);

                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        message = respons.status,
                        data = subscription,
                    });
                }
            }
            return Ok(new APIResponseModel
            {
                hasError = false,
                statusCode = (int)HttpStatusCode.BadRequest,
                message = "There an error processing your subscription"
            });
        }

        [HttpPost("subscriber")]
        public async Task<IActionResult> payment([FromQuery] string reference, [FromBody] PaymentMethodViewMode model)
        {
            if (model == null) return Ok(new APIResponseModel
            {
                hasError = true,
                statusCode = (int)HttpStatusCode.BadRequest,
                message = "Bad Request"
            });

            var tranxRef = reference;

            var errorMessage = string.Empty;

            if (model.CompanyId == Guid.Empty)
                errorMessage = "Valide Company Id required";

            if (model.PlanId <= 0)
                errorMessage = "Valide Plan Id required";


            if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
            {
                hasError = true,
                statusCode = (int)HttpStatusCode.BadRequest,
                message = errorMessage
            });

            var plan = _service.GetPlans()
                        .Where(p => p.PlanId == model.PlanId).FirstOrDefault();
            if (plan == null)
            {
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Invalid plan Id"
                });
            }


            var user = _service.CompanyById(model.CompanyId).FirstOrDefault();
            if (user == null)
            {
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Invalid company Id"
                });
            }

            if (user.ID_Subscription > 0)
            {
                var checkIfUserSubscriptionStillActive = _service.GetTransactionByCompanyId(user.ID_Subscription).FirstOrDefault();
                if (checkIfUserSubscriptionStillActive != null)
                {
                    if (checkIfUserSubscriptionStillActive.ExpiredDate >= DateTime.Now && checkIfUserSubscriptionStillActive.PaymentStatus == true)
                    {
                        return Ok(new APIResponseModel
                        {
                            hasError = true,
                            statusCode = (int)HttpStatusCode.BadRequest,
                            message = "You still have active plan till " + checkIfUserSubscriptionStillActive.ExpiredDate.Value.ToString("dd/MM/yyyy")
                        });
                    }
                }
            }

            //FreePlan
            if (plan.IsFreePlan)
            {
                var company = new CompanySubscription()
                {
                    ID_Company = model.CompanyId,
                    ID_Plan = model.PlanId,
                    PlanType = plan.PlanName,
                    Amount = 0,
                    PaymentStatus = true,
                    IsActive = true,
                    TransactionRef = "Free Plan",
                    PaymentMethod = "Free Plan",
                    TransactionDate = DateTime.Now,
                    ExpiredDate = DateTime.Now.AddMonths(plan.PlanDuration.Value)
                };

                var updateCompany = await _service.AddOnlineDepositOrder(company);
                if (updateCompany)
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.OK,
                        message = "success"
                    });

                }
            }
            else
            {


                if (string.IsNullOrEmpty(tranxRef))
                {
                    errorMessage = "Reference number is required";
                }

                //checkIfPaymentStatusAlreadyUse
                if (_service.ReQueryPayment(tranxRef))
                {
                    errorMessage = "Reference number already used";
                }

                if (string.IsNullOrEmpty(model.method))
                    errorMessage = "Payment method must either be PSTACK or RAVE";

                if (model.method.ToUpper().Trim() != "PSTACK" && model.method.ToUpper().Trim() != "RAVE")
                    errorMessage = "Payment method must either be PSTACK or RAVE";

                var amount = decimal.Parse(model.Amount);

                if (amount < 0 || amount == 0)
                {
                    if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Amount must be greated than zero"
                    });
                }

                //Check if admin is runing promo code or there is referral discount
                if (model.PromoCodeId != Guid.Empty && model.PromoCodeId != null)
                {
                    if (_service.isPromoCodeValid(model.PromoCodeId.Value))
                    {
                        errorMessage = $"Promo Code for {model.PromoCodeId.Value} already used";
                    }

                    plan.Amount = Math.Round(_service.ValidPromoCodeAmount(model.PromoCodeId.Value),2);
                }

                if (model.ReferralCodeId != Guid.Empty && model.ReferralCodeId != null)
                {
                    if (_service.isReferralCodeValid(model.PromoCodeId.Value))
                    {
                        errorMessage = $"Referral Code for {model.PromoCodeId.Value} already used";
                    }

                    plan.Amount = Math.Round(_service.ValidReferralCodeAmount(model.ReferralCodeId.Value),2);
                }

                //Validate amount
                if (!Math.Round(amount, 2).Equals(Math.Round(plan.Amount, 2)))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Amount for this plan is " + plan.Amount.ToString("#,##0.00;(#,##0.00)")
                    });
                }

                var targetUsr = _service.CompanyById(model.CompanyId).FirstOrDefault();

                var company = new CompanySubscription()
                {
                    ID_Company = model.CompanyId,
                    ID_Plan = model.PlanId,
                    PlanType = plan.PlanName,
                    Amount = amount,
                    PaymentStatus = false,
                    TransactionRef = reference,
                    PaymentMethod = model.method,
                    TransactionDate = DateTime.Now,
                    ExpiredDate = DateTime.Now.AddMonths(plan.PlanDuration.Value)
                };
                await _service.AddOnlineDepositOrder(company);

            }


            var paystackKey = Convert.ToBoolean(config.GetValue<bool>("InDevelopment")) == true
                ? config.GetValue<string>("Paystack:TestSecretKey") : config.GetValue<string>("Paystack:LiveSecretKey");

            var paystackAPI = new PaystackPayment(paystackKey, string.Empty);

            var response = await paystackAPI.VerifyTransaction(tranxRef);

            if (response.status && response.data.status == "success")
            {
                var amountPaid = (decimal)(response.data.amount / 100); //Always divide returned amount by 100 to get 
                var transactionRef = response.data.reference;
                if (await _service.UpdateOnlineDepositOrder(transactionRef))
                {
                    if (model.PromoCodeId != Guid.Empty && model.PromoCodeId != null)
                    {
                        await _service.UpdatePromoCode(model.PromoCodeId.Value);
                    }

                    if (model.ReferralCodeId != Guid.Empty && model.ReferralCodeId != null)
                    {
                        await _service.UpdateReferralCode(model.ReferralCodeId.Value);
                    }
                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        message = response.data.status
                    });
                }

            }

            else

            {

                var tx_ref = reference;
                var transaction_id = HttpContext.Request.Query["transaction_id"];
                var transStatus = HttpContext.Request.Query["status"];

                if (string.IsNullOrWhiteSpace(transStatus) || string.IsNullOrWhiteSpace(transaction_id)
                     || string.IsNullOrWhiteSpace(tx_ref))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.InternalServerError,
                        message = "Error occur"
                    });
                }

                var secretKey = Convert.ToBoolean(config.GetValue<bool>("InDevelopment")) == true
                ? config.GetValue<string>("Flutterwave:TestSecretKey") : config.GetValue<string>("Flutterwave:LiveSecretKey");

                var raveAPI = new RavePayment(secretKey);

                var transactionId = int.Parse(transaction_id);

                var respons = await raveAPI.VerifyTransaction(transactionId);

                if (!respons.status.Equals("success"))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Error occur"
                    });
                }

                //Code after payment here
                if (await _service.UpdateOnlineDepositOrder(tx_ref))
                {
                    if (model.PromoCodeId != Guid.Empty && model.PromoCodeId != null)
                    {
                        await _service.UpdatePromoCode(model.PromoCodeId.Value);
                    }

                    if (model.ReferralCodeId != Guid.Empty && model.ReferralCodeId != null)
                    {
                        await _service.UpdateReferralCode(model.ReferralCodeId.Value);
                    }

                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        message = respons.status
                    });
                }

            }
            return Ok(new APIResponseModel
            {
                hasError = false,
                statusCode = (int)HttpStatusCode.BadRequest,
                message = "There an error processing your subscription"
            });
        }

        [HttpPost("promotionalcode")]
        public IActionResult PromotionCode([FromBody] PromoCodeViewModel model)
        {
            try
            {
                if (model == null) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Bad Request"
                });

                if (!_service.isPromoCodeEnable()) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Promo Code not enable"
                });

                var response = _service.PromoCode(model);
                if (response.IsSaved)
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        data = response
                    });
                }

                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = "Internal Server Error"
                });
            }
            catch (Exception ex)
            {
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = ex.Message
                });
            }
        }

        [HttpPost("referralcode")]
        public IActionResult ReferralCode([FromBody] ReferralViewModel model)
        {
            try
            {
                if (model == null) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Bad Request"
                });

                if (!_service.isReferralCodeEnable()) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Referral Code not enable"
                });

                var response = _service.ReferralCode(model);
                if (response.IsSaved)
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        data = response
                    });
                }

                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = "Internal Server Error"
                });
            }
            catch (Exception ex)
            {
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = ex.Message
                });
            }
        }

        [Route("get/customersubcription/{subcriptionid}")]
        [HttpGet]
        public IActionResult GetSubscription(int subcriptionid)
        {
            var exMessage = string.Empty;
            try
            {
                object subscription = null;

                if (subcriptionid > 0)
                {
                    var query = _service.GetTransactionByCompanyId(subcriptionid).ToList();

                    subscription = (from x in query
                                    select new
                                    {
                                        ID_Subscription = x.ID_Subscription,
                                        ID_Company = x.ID_Company,
                                        ID_Plan = x.ID_Plan,
                                        Amount = x.Amount,
                                        PaymentStatus = x.PaymentStatus,
                                        TransactionRef = x.TransactionRef,
                                        TransactionDate = x.TransactionDate == null ? null : x.TransactionDate.Value.ToString("dd/MM/yyyy"),
                                        PaymentMethod = x.PaymentMethod,
                                        ExpiredDate = x.ExpiredDate == null ? null : x.ExpiredDate.Value.ToString("dd/MM/yyyy")
                                    });
                }

                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = subscription
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

        //[Route("customersubcription/create")]
        //[HttpPost]
        //public async Task<IActionResult> AddSubscription([FromBody] OnlineDepositViewModel model)
        //{
        //    try
        //    {
        //        if (model == null) return Ok(new APIResponseModel
        //        {
        //            hasError = true,
        //            statusCode = (int)HttpStatusCode.BadRequest,
        //            message = "Bad Request"
        //        });

        //        var errorMessage = string.Empty;


        //        if (model.CompanyId == Guid.Empty)
        //            errorMessage = "Company Id is a required field";

        //        var plan = _service.GetPlans()
        //                .Where(p => p.PlanId == model.PlanId).FirstOrDefault();
        //        if (plan == null)
        //        {
        //            return Ok(new APIResponseModel
        //            {
        //                hasError = true,
        //                statusCode = (int)HttpStatusCode.BadRequest,
        //                message = "Invalid plan Id"
        //            });
        //        }

        //        if (!plan.IsFreePlan)
        //        {
        //            if (string.IsNullOrWhiteSpace(model.Amount))
        //                errorMessage = "Amount is a required field";

        //            if (string.IsNullOrWhiteSpace(model.TransactionRef))
        //                errorMessage = "TransactionRef is a required field";

        //            if (string.IsNullOrWhiteSpace(model.PaymentMethod))
        //                errorMessage = "PaymentMethod is a required field";

        //            if (!string.IsNullOrWhiteSpace(model.PaymentMethod))
        //            {
        //                if (!model.PaymentMethod.ToUpper().Equals("PSTACK") && !model.PaymentMethod.ToUpper().Equals("RAVE"))
        //                    errorMessage = "Invalid payment method";
        //            }
        //        }

        //        if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
        //        {
        //            hasError = true,
        //            statusCode = (int)HttpStatusCode.BadRequest,
        //            message = errorMessage
        //        });

        //        var amount = 0m;
        //        if (!plan.IsFreePlan)
        //        {
        //            amount = decimal.Parse(model.Amount);

        //            if (amount < 0 || amount == 0)
        //            {
        //                return Ok(new APIResponseModel
        //                {
        //                    hasError = true,
        //                    statusCode = (int)HttpStatusCode.BadRequest,
        //                    message = "Amount must be greated than zero"
        //                });
        //            }

        //            if (amount < plan.Amount)
        //            {
        //                return Ok(new APIResponseModel
        //                {
        //                    hasError = true,
        //                    statusCode = (int)HttpStatusCode.BadRequest,
        //                    message = "Amount for this plan is " + plan.Amount.ToString("#,##0.00;(#,##0.00)")
        //                });
        //            }
        //        }

        //        var user = _service.CompanyById(model.CompanyId).FirstOrDefault();
        //        if (user != null)
        //        {
        //            if (user.ID_Subscription > 0)
        //            {
        //                var checkIfUserSubscriptionStillActive = _service.GetTransactionByCompanyId(user.ID_Subscription).FirstOrDefault();

        //                if (checkIfUserSubscriptionStillActive.ExpiredDate >= DateTime.Now && checkIfUserSubscriptionStillActive.PaymentStatus == true)
        //                {
        //                    return Ok(new APIResponseModel
        //                    {
        //                        hasError = true,
        //                        statusCode = (int)HttpStatusCode.OK,
        //                        message = "You still have active plan till " + checkIfUserSubscriptionStillActive.ExpiredDate.Value.ToString("dd/MM/yyyy")
        //                    });
        //                }
        //            }
        //        }
        //        else
        //        {
        //            return Ok(new APIResponseModel
        //            {
        //                hasError = true,
        //                statusCode = (int)HttpStatusCode.OK,
        //                message = "Invalid company Id"
        //            });
        //        }

        //        var company = new CompanySubscription()
        //        {
        //            ID_Company = model.CompanyId,
        //            ID_Plan = model.PlanId,
        //            PlanType = plan.PlanName,
        //            Amount = amount,
        //            PaymentStatus = true,
        //            IsActive = true,
        //            TransactionRef = model.TransactionRef,
        //            PaymentMethod = model.PaymentMethod,
        //            TransactionDate = DateTime.Now,
        //            ExpiredDate = plan.PlanDuration <= 1 ? DateTime.Now.AddDays(30) : DateTime.Now.AddYears(1)
        //        };

        //        var updateCompany = await _service.AddOnlineDepositOrder(company);
        //        if (updateCompany)
        //        {
        //            return Ok(new APIResponseModel
        //            {
        //                hasError = true,
        //                statusCode = (int)HttpStatusCode.OK,
        //                message = "success"
        //            });

        //        }


        //        return Ok(new APIResponseModel
        //        {
        //            hasError = true,
        //            statusCode = (int)HttpStatusCode.InternalServerError,
        //            message = "Internal Server Error"
        //        });
        //    }
        //    catch (Exception)
        //    {
        //        return Ok(new APIResponseModel
        //        {
        //            hasError = true,
        //            statusCode = (int)HttpStatusCode.InternalServerError,
        //            message = "Internal Server Error"
        //        });
        //    }
        //}

        [Route("get/company/{companyid}")]
        [HttpGet]
        public IActionResult GetCompany(Guid companyid)
        {
            var exMessage = string.Empty;
            try
            {
                object company = null;

                if (companyid != Guid.Empty)
                {
                    company = _service.CompanyById(companyid).ToList();

                }

                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = company
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

        //[Route("transactions/update/{TransactionRef}")]
        //[HttpPut]
        //public async Task<IActionResult> UpdateSubscription(string TransactionRef)
        //{
        //    try
        //    {
        //        var errorMessage = string.Empty;
        //        if (string.IsNullOrWhiteSpace(TransactionRef))
        //            errorMessage = "TransactionRef is a required field";

        //        if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
        //        {
        //            hasError = true,
        //            statusCode = (int)HttpStatusCode.BadRequest,
        //            message = errorMessage
        //        });

        //        var isRecordExist = _service.GetTransaction().Where(x => x.TransactionRef == TransactionRef).FirstOrDefault();
        //        if (isRecordExist != null)
        //        {
        //            var order = await _service.UpdateOnlineDepositOrder(TransactionRef);
        //            if (order)
        //            {
        //                return Ok(new APIResponseModel
        //                {
        //                    hasError = true,
        //                    statusCode = (int)HttpStatusCode.OK,
        //                    message = "success"
        //                });
        //            }
        //        }
        //        else
        //        {
        //            return Ok(new APIResponseModel
        //            {
        //                hasError = true,
        //                statusCode = (int)HttpStatusCode.InternalServerError,
        //                message = "Invaild transactionRef no"
        //            });
        //        }



        //        return Ok(new APIResponseModel
        //        {
        //            hasError = true,
        //            statusCode = (int)HttpStatusCode.InternalServerError,
        //            message = "Internal Server Error"
        //        });
        //    }
        //    catch (Exception)
        //    {
        //        return Ok(new APIResponseModel
        //        {
        //            hasError = true,
        //            statusCode = (int)HttpStatusCode.InternalServerError,
        //            message = "Internal Server Error"
        //        });
        //    }
        //}

        //public static string CovertDate(string getdate)
        //{
        //    try
        //    {
        //        string[] dateTokens = getdate.Split('/', '-');
        //        string strDay = dateTokens[0];
        //        string strMonth = dateTokens[1];
        //        string strYear = dateTokens[2];
        //        string date = strMonth + "/" + strDay + "/" + strYear;

        //        return date;
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //    }
        //}
    }
}
