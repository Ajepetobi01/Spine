using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Converters;
using Spine.Common.Enums;
using Spine.Core.Inventories.Helper;
using Spine.Core.Inventories.Jobs;
using Spine.Data;
using Spine.Data.Entities.Inventories;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Service
{
    public static class AddBulkServices
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            public List<ServiceModel> Services { get; set; }

        }

        public class ServiceModel
        {
            [Required]
            [Description("Name")]
            public string Name { get; set; }

            [Description("Description")]
            public string Description { get; set; }

            [Required]
            [Description("Sales Price")]
            [JsonConverter(typeof(StringToDecimalConverter))]
            public decimal SalesPrice { get; set; }

        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.Created;
            }

            public Response(HttpStatusCode statusCode)
            {
                Status = statusCode;
            }

            public Response(string message)
            {
                ErrorMessage = message;
                Status = HttpStatusCode.BadRequest;
            }
        }


        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly SpineContext _dbContext;
            private readonly IAuditLogHelper _auditHelper;
            private readonly IMapper _mapper;
            private readonly CommandsScheduler _scheduler;

            public Handler(SpineContext context, IMapper mapper, CommandsScheduler scheduler, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _mapper = mapper;
                _scheduler = scheduler;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var date = DateTime.Today;
                var accountingPeriod = await _dbContext.AccountingPeriods.FirstOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId
                    && date >= x.StartDate && date <= x.EndDate);

                if (accountingPeriod == null) return new Response("There is no accounting period for today");
                if(accountingPeriod.IsClosed) return new Response("There is no open accounting period for today");
                
                int skipped = 0;
                var serviceCategory = await _dbContext.ProductCategories
                    .Where(x => x.CompanyId == request.CompanyId && x.IsServiceCategory)
                    .Select(x => x.Id).SingleOrDefaultAsync();

                var receivedItems = new List<ReceivedGoodsModel>();
                foreach (var item in request.Services)
                {
                    var service = _mapper.Map<Inventory>(item);
                    service.CreatedBy = request.UserId;
                    service.CompanyId = request.CompanyId;
                    service.CategoryId = serviceCategory;

                    _dbContext.Inventories.Add(service);

                    _dbContext.InventoryPriceHistories.Add(new InventoryPriceHistory
                    {
                        CompanyId = request.CompanyId,
                        CreatedBy = request.UserId,
                        InventoryId = service.Id,
                        UnitCostPrice = 0.00m,
                        UnitSalesPrice = item.SalesPrice,
                        RestockDate = DateTime.Today,
                        CreatedOn = DateTime.Today
                    });
                    
                    // receivedItems.Add(new ReceivedGoodsModel
                    // {
                    //     AccountingPeriodId = accountingPeriod.Id,
                    //     Amount = 0.00m,
                    //     TaxAmount = 0.00m,
                    //     TaxId = null,
                    //     VendorId = null,
                    //     InventoryId = service.Id,
                    //      Inventory = service.Name,
                    //     DateReceived = date,
                    //     ReceivedBy = request.UserId
                    // });
                    
                    _auditHelper.SaveAction(_dbContext, request.CompanyId,
                      new AuditModel
                      {
                          EntityType = (int)AuditLogEntityType.Inventory,
                          Action = (int)AuditLogInventoryAction.Create,
                          Description = $"Added new service inventory {item.Name}",
                          UserId = request.UserId
                      });
                }

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    //not needed since it's going to be 0 amount
                    // _scheduler.SendNow(new HandleAccountingForConfirmGoodsReceived
                    // {
                    //     CompanyId = request.CompanyId,
                    //     ReceivedGoods = receivedItems,
                    //     UserId = request.UserId,
                    // });

                    return new Response(HttpStatusCode.Created);
                }
                
                return new Response("No records saved");
            }
        }

    }
}
