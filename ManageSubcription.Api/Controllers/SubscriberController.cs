using AutoMapper;
using ManageSubcription.Api.Authorizations;
using ManageSubcription.Api.Helpers;
using ManageSubcription.Api.Model;
using ManageSubcription.Api.Services;
using ManageSubcription.Api.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spine.Common.Enums;
using Spine.Core.ManageSubcription.Filter;
using Spine.Core.ManageSubcription.Interface;
using Spine.Core.ManageSubcription.ViewModel;
using Spine.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Controllers
{
    [Route("api/v1/subscriber")]
    [ApiController]
    [Authorize]
    public class SubscriberController : MyControllerBase
    {
        private readonly IManageSubcriptionRepository _service;
        private readonly ILogger<SubscriberController> _logger;
        private readonly IConfiguration config;
        private readonly IMapper mapper;
        private readonly IUriService uriService;

        public SubscriberController(IManageSubcriptionRepository service, ILogger<SubscriberController> logger,
            IConfiguration config, IMapper mapper, IUriService uriService) : base(logger)
        {
            _service = service;
            _logger = logger;
            this.config = config;
            this.mapper = mapper;
            this.uriService = uriService;
        }

        [HttpGet("all-un-subscriber/User")]
        [HasPermission(Permissions.ViewSubscriber)]
        public async Task<IActionResult> GetAllUnSubscriberUser([FromQuery] GetAllPostQuery query, [FromQuery] PaginationQuery paginationQuery)
        {
            var pagination = mapper.Map<PaginationFilter>(paginationQuery);
            var filter = mapper.Map<GetAllPostFilter>(query);
            var posts = _service.GetUnSubscribers(filter, pagination).ToList();
            var totalRecords = await _service.GetUnSubscribers(filter).CountAsync();
            var postsReponse = mapper.Map<List<SubscriptionDTO>>(posts);
            if (pagination == null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(new PagedResponse<SubscriptionDTO>(postsReponse));
            }
            var paginationResponse = PaginationHelper.PagedResponse(uriService, pagination, postsReponse, totalRecords);
            return Ok(paginationResponse);
        }

        [Route("all-record")]
        [HttpGet]
        [HasPermission(Permissions.ViewSubscriber)]
        public async Task<IActionResult> GetAllSubscriber([FromQuery] GetAllPostQuery query, [FromQuery] PaginationQuery paginationQuery)
        {
            var pagination = mapper.Map<PaginationFilter>(paginationQuery);
            var filter = mapper.Map<GetAllPostFilter>(query);
            var posts = _service.GetSubscribers(filter, pagination).ToList();
            var totalRecords = await _service.GetSubscribers(filter).CountAsync();
            var postsReponse = mapper.Map<List<SubscriptionDTO>>(posts);
            if (pagination == null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(new PagedResponse<SubscriptionDTO>(postsReponse));
            }
            var paginationResponse = PaginationHelper.PagedResponse(uriService, pagination, postsReponse, totalRecords);
            return Ok(paginationResponse);
        }
        [Route("almost-expired")]
        [HttpGet]
        [HasPermission(Permissions.ViewSubscriber)]
        public async Task<IActionResult> ExpiredSubscriber([FromQuery] GetAllPostQuery query, [FromQuery] PaginationQuery paginationQuery)
        {
            var pagination = mapper.Map<PaginationFilter>(paginationQuery);
            var filter = mapper.Map<GetAllPostFilter>(query);
            var posts = _service.GetAlmostExpirySubscribers(filter, pagination).ToList();
            var totalRecords = await _service.GetAlmostExpirySubscribers(filter).CountAsync();
            var postsReponse = mapper.Map<List<SubscriptionDTO>>(posts);
            if (pagination == null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(new PagedResponse<SubscriptionDTO>(postsReponse));
            }
            var paginationResponse = PaginationHelper.PagedResponse(uriService, pagination, postsReponse, totalRecords);
            return Ok(paginationResponse);
        }
        [Route("onboradinganalysis")]
        [HttpGet]
        [HasPermission(Permissions.ViewSubscriber)]
        public async Task<IActionResult> OnboradingAnalysis([FromQuery] GetAllPostQuery query, [FromQuery] PaginationQuery paginationQuery)
        {
            var pagination = mapper.Map<PaginationFilter>(paginationQuery);
            var filter = mapper.Map<GetAllPostFilter>(query);
            var posts = _service.GetOnboradingAnalysis(filter, pagination).ToList();
            var totalRecords = await _service.GetOnboradingAnalysis(filter).CountAsync();
            var postsReponse = mapper.Map<List<SubscriptionDTO>>(posts);
            if (pagination == null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(new PagedResponse<SubscriptionDTO>(postsReponse));
            }
            var paginationResponse = PaginationHelper.PagedResponse(uriService, pagination, postsReponse, totalRecords);
            return Ok(paginationResponse);
        }
        [Route("dashboard")]
        [HttpGet]
        public IActionResult AdminDashboardStatictis()
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.DashboardList()
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
        [HttpGet("referral/report")]
        [HasPermission(Permissions.ViewSubscriber)]
        public IActionResult ReferralReport([FromQuery] GetAllPostQuery query, [FromQuery] PaginationQuery paginationQuery)
        {
            var pagination = mapper.Map<PaginationFilter>(paginationQuery);
            var filter = mapper.Map<GetAllPostFilter>(query);
            var posts = _service.GetReferralSubscribers(filter, pagination).ToList();
            var totalRecords = _service.GetReferralSubscribers(filter).Count();
            var postsReponse = mapper.Map<List<ReferralSubscriptionDTO>>(posts);
            if (pagination == null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(new PagedResponse<ReferralSubscriptionDTO>(postsReponse));
            }
            var paginationResponse = PaginationHelper.PagedResponse(uriService, pagination, postsReponse, totalRecords);
            return Ok(paginationResponse);
        }
        [HttpGet("all-Imported/{batchno}")]
        [HasPermission(Permissions.ViewSubscriber)]
        public IActionResult GetImportedSubscriber([FromRoute] string batchno, [FromQuery] GetAllPostQuery query, [FromQuery] PaginationQuery paginationQuery)
        {
            var pagination = mapper.Map<PaginationFilter>(paginationQuery);
            var filter = mapper.Map<GetAllPostFilter>(query);
            var posts = _service.GetImportedSubscribers(batchno, filter, pagination).ToList();
            if (filter?.daysTwoExpired > 0)
            {
                posts = posts.Where(x => x.DaysToExpired <= filter.daysTwoExpired).ToList();
            }
            var totalRecords = _service.GetImportedSubscribers(batchno, filter).Count();
            var postsReponse = mapper.Map<List<SubscriptionDTO>>(posts);
            if (pagination == null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(new PagedResponse<SubscriptionDTO>(postsReponse));
            }
            var paginationResponse = PaginationHelper.PagedResponse(uriService, pagination, postsReponse, totalRecords);
            return Ok(paginationResponse);
        }
        [HttpGet("un-subscriber/companyId")]
        [HasPermission(Permissions.ViewSubscriber)]
        public IActionResult GetUnSubscriberById([FromQuery] Guid companyId)
        {
            try
            {
                if (companyId != Guid.Empty)
                {
                    object role = _service.GetUnSubacriberByCompayId(companyId);
                    return Ok(new APIResponseModel
                    {
                        statusCode = (int)HttpStatusCode.OK,
                        data = role
                    });
                }
                else
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = $"Id is required"
                    });
                }

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
        
        [HttpGet("companyId")]
        [HasPermission(Permissions.ViewSubscriber)]
        public IActionResult GetSubscriberById([FromQuery] Guid companyId)
        {
            try
            {
                if (companyId != Guid.Empty)
                {
                    object role = _service.GetSubacriberByCompayId(companyId).FirstOrDefault();
                    return Ok(new APIResponseModel
                    {
                        statusCode = (int)HttpStatusCode.OK,
                        data = role
                    });
                }
                else
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = $"Id is required"
                    });
                }

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
        [Route("total/subscribers")]
        [HttpGet]
        public IActionResult TotalSubscriber()
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.TotalNumberOfSubscribers()
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

        [Route("total/active/subscribers")]
        [HttpGet]
        public IActionResult TotalActiveSubscriber()
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.TotalNumberOfActiveSubscribers()
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

        [Route("total/in-active/subscribers")]
        [HttpGet]
        public IActionResult TotalInActiveSubscriber()
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.TotalNumberOfInActiveSubscribers()
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

        [Route("total/subscribers/{referralcode}")]
        [HttpGet]
        public IActionResult TotalSubscriberByReferralCode(string referralcode)
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.TotalNumberOfSubscriberByReferralCode(referralcode)
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

        [Route("total/active/subscribers/{referralcode}")]
        [HttpGet]
        public IActionResult TotalActiveSubscriberByReferralcode(string referralcode)
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.TotalNumberOfActiveSubscriberByReferralCode(referralcode)
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

        [HttpGet("total/in-active/subscribers/{referralcode}")]
        public IActionResult TotalInActiveSubscriberByReferralcode(string referralcode)
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.TotalNumberOfInActiveSubscriberByReferralCode(referralcode)
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
        
        [HttpGet("total/transaction/amount")]
        public IActionResult TotalTransactionAmount()
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.TotalTransaction()
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
        [HttpGet("birthday")]
        public async Task<IActionResult> SubscriberBirthDay([FromQuery] PostBirthDateQuery query, [FromQuery] PaginationQuery paginationQuery)
        {
            var pagination = mapper.Map<PaginationFilter>(paginationQuery);
            var filter = mapper.Map<PostBirthFilter>(query);
            var posts = _service.GetBirthDayList(filter, pagination).ToList();
            var totalRecords = await _service.GetBirthDayList(filter).CountAsync();
            var postsReponse = mapper.Map<List<BirthDayVM>>(posts);
            if (pagination == null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(new PagedResponse<BirthDayVM>(postsReponse));
            }
            var paginationResponse = PaginationHelper.PagedResponse(uriService, pagination, postsReponse, totalRecords);
            return Ok(paginationResponse);
        }

        [Route("subscribers/plans")]
        [HttpGet]
        public IActionResult GetAllSubscriberByPlan()
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.TotalNumberOfSubscribersByPlan()
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
        /// <summary>
        /// get companies(subscriber) upload template
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("bulk-template")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HasPermission(Permissions.DownloadSubscriberTemplate)]
        public async Task<IActionResult> GetSubscriberUploadTemplate([FromServices] IExcelTemplateGenerator templateGenerator)
        {
            var (stream, contentType, fileName) = await templateGenerator.GenerateTemplate(BulkImportType.Subscriber, null);

            return File(stream, contentType, fileName);
        }
        /// <summary>
        /// upload bulk companies(subscriber)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost("upload-bulk")]
        [HasPermission(Permissions.UploadSubscriber)]
        public async Task<IActionResult> UploadBulkSubscriber(IFormFile file, [FromServices] IExcelReader excelReader)
        {
            try
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension is not (".xls" or ".xlsx" or ".xlsm"))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Upload a valid excel file"
                    });
                }
                DataTable excelData;
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    stream.Position = 0;
                    excelData = await excelReader.ReadExcelFile(stream);
                    if (excelData == null || excelData.Rows.Count < 1)
                    {
                        return Ok(new APIResponseModel
                        {
                            hasError = true,
                            statusCode = (int)HttpStatusCode.BadRequest,
                            message = "You cannot upload an empty file"
                        });
                    }
                    if (excelData.Columns.Count == 0 || excelData.Columns[4].ColumnName != "BusinessName")
                    {
                        return Ok(new APIResponseModel
                        {
                            hasError = true,
                            statusCode = (int)HttpStatusCode.BadRequest,
                            message = "Use the template provided"
                        });
                    }
                }

                var jsonString = excelReader.DataTableToJSONWithJSONNet(excelData);
                var subcribers = JsonConvert.DeserializeObject<List<CompanyUploadModel>>(jsonString);

                var response = await _service.SaveSubcribers(subcribers, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault());
                if (response.Status.Equals("success"))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        data = response.Message
                    });
                }
                else
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.InternalServerError,
                        data = response.Message
                    });
                }
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

        [Route("create")]
        [HttpPost]
        [HasPermission(Permissions.AddSubscriber)]
        public async Task<IActionResult> AddSubscriber([FromBody] CompanyParam model)
        {
            try
            {
                if (model == null) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Bad Request"
                });


                var errorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(model.BusinessName))
                    errorMessage = "Business name is required";
                if (string.IsNullOrWhiteSpace(model.FirstName))
                    errorMessage = "Firstname is required";
                if (string.IsNullOrWhiteSpace(model.LastName))
                    errorMessage = "LastName is required";
                if (string.IsNullOrWhiteSpace(model.Email))
                    errorMessage = "Email is required";
                if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                    errorMessage = "Phone Number is required";
                if (string.IsNullOrWhiteSpace(model.OperatingSector))
                    errorMessage = "Please select sector you operate in";
                if (string.IsNullOrWhiteSpace(model.BusinessType))
                    errorMessage = "Business type is required";
                //if (string.IsNullOrWhiteSpace(model.DateOfBirth))
                //    errorMessage = "Date Of Birth type is required";

                //var CheckBusinessName = _service.QueryCompany().Where(x => (x.Name.ToLower() == model.BusinessName.ToLower() || x.Email == model.Email.ToLower()));
                //if (CheckBusinessName != null) errorMessage = "Business name or email is already taken";

                //var CheckEmail = _service.QueryCompany().Where(x => x.Email == model.Email.ToLower());
                //if (CheckEmail != null) errorMessage = "Email is already in use";


                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                var dateofbirth = new DateTime();
                if (!string.IsNullOrWhiteSpace(model.DateOfBirth))
                {
                    try
                    {
                        model.DateOfBirth = CovertDate(model.DateOfBirth);
                        dateofbirth = Convert.ToDateTime(model.DateOfBirth);
                    }
                    catch
                    {
                        return Ok(new APIResponseModel
                        {
                            hasError = true,
                            statusCode = (int)HttpStatusCode.InternalServerError,
                            message = "Date of birth not valid expected date is dd/MM/yyyy"
                        });
                    }
                }


                var response = await _service.SaveSubcriber(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault());
                if (response.Status.Equals("success"))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        data = response.Message
                    });
                }
                else
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.InternalServerError,
                        data = response.Message
                    });
                }

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

        [HttpPut("{CompanyId}")]
        [HasPermission(Permissions.UpdateSubscriber)]
        public IActionResult UpdateSubscriber([FromRoute] Guid CompanyId, [FromBody] UpdateCompanyParam model)
        {
            try
            {
                if (model == null) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Bad Request"
                });

                if (CompanyId == Guid.Empty)
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Valid CompanyId is required"
                    });
                }


                var errorMessage = string.Empty;


                if (string.IsNullOrWhiteSpace(model.BusinessName))
                    errorMessage = "Business name is required";
                if (string.IsNullOrWhiteSpace(model.FirstName))
                    errorMessage = "Firstname is required";
                if (string.IsNullOrWhiteSpace(model.LastName))
                    errorMessage = "LastName is required";
                if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                    errorMessage = "Phone Number is required";
                if (string.IsNullOrWhiteSpace(model.OperatingSector))
                    errorMessage = "Please select sector you operate in";
                if (string.IsNullOrWhiteSpace(model.BusinessType))
                    errorMessage = "Business type is required";
                //if (string.IsNullOrWhiteSpace(model.DateOfBirth))
                //    errorMessage = "Date Of Birth type is required";

                //var CheckBusinessName = _service.QueryCompany().Where(x => (x.Name.ToLower() == model.BusinessName.ToLower() || x.Email == model.Email.ToLower()));
                //if (CheckBusinessName != null) errorMessage = "Business name or email is already taken";

                //var CheckEmail = _service.QueryCompany().Where(x => x.Email == model.Email.ToLower());
                //if (CheckEmail != null) errorMessage = "Email is already in use";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });


                var dateofbirth = new DateTime();
                if (!string.IsNullOrWhiteSpace(model.DateOfBirth))
                {
                    try
                    {
                        model.DateOfBirth = CovertDate(model.DateOfBirth);
                        dateofbirth = Convert.ToDateTime(model.DateOfBirth);
                    }
                    catch
                    {
                        return Ok(new APIResponseModel
                        {
                            hasError = true,
                            statusCode = (int)HttpStatusCode.InternalServerError,
                            message = "Date of birth not valid expected date is dd/MM/yyyy"
                        });
                    }
                }

                var targetStatus = _service.UpdateSubcriber(CompanyId, model, UserId.GetValueOrDefault());
                if (targetStatus.Status.Equals("success"))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        data = targetStatus
                    });
                }
                else
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.InternalServerError,
                        data = targetStatus
                    });
                }



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
        [HttpPost("subscription/for/upload/transaction")]
        [HasPermission(Permissions.UploadSubscriber)]
        public async Task<IActionResult> SubscriptionWithPayment([FromBody] GetUploadSubscriber model)
        {
            try
            {
                if (model.TotalAmount < 0 || model.TotalAmount == 0)
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Amount must be greated than zero"
                    });
                }

                if (model.SubscriberPlanList.Count() == 0)
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Add list of subscriber"
                    });
                }

                if (await _service.SaveUploadSubscription(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.OK,
                        message = "success"
                    });

                }
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = $"Internal Server Error"
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
        [HttpPost("subscription/transaction/withouthpayment")]
        [HasPermission(Permissions.UploadSubscriber)]
        public async Task<IActionResult> SubscriptionWithoutPayment([FromBody] GetUploadSubscriber model)
        {
            try
            {
                //if (model.TotalAmount < 0 || model.TotalAmount == 0)
                //{
                //    return Ok(new APIResponseModel
                //    {
                //        hasError = true,
                //        statusCode = (int)HttpStatusCode.BadRequest,
                //        message = "Amount must be greated than zero"
                //    });
                //}

                if (model.SubscriberPlanList.Count() == 0)
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Add list of subscriber"
                    });
                }

                if (await _service.SaveUploadSubscriptionNoPayment(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.OK,
                        message = "success"
                    });

                }
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = $"Internal Server Error"
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

        [HttpPost("enabledisable/subscriber/{companyid}")]
        [HasPermission(Permissions.EnableDisableSubscriber)]
        public IActionResult EnableAndDisableSubscriber(Guid companyid)
        {
            try
            {
                if (_service.EnableSubscriber(companyid))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.OK,
                        message = ""
                    });
                }
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = $"Internal Server Error"
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
        private static string CovertDate(string getdate)
        {
            try
            {
                string[] dateTokens = getdate.Split('/', '-');
                string strDay = dateTokens[0];
                string strMonth = dateTokens[1];
                string strYear = dateTokens[2];
                string date = strMonth + "/" + strDay + "/" + strYear;

                return date;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
