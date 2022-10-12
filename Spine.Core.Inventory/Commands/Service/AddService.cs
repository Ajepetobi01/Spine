using System;
using System.Collections.Generic;
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
using Spine.Common.Enums;
using Spine.Core.Inventories.Helper;
using Spine.Core.Inventories.Jobs;
using Spine.Data;
using Spine.Data.Entities.Inventories;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Service
{
    public static class AddService
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required]
            public string Name { get; set; }

            public string Description { get; set; }

            [Required]
            public decimal UnitSalesPrice { get; set; }

        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.OK;
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
                
                if (await _dbContext.Inventories.AnyAsync(x => x.CompanyId == request.CompanyId && !x.IsDeleted &&
                    x.Name.ToLower() == request.Name.ToLower()))
                {
                    return new Response("Name must be unique");
                }

                var serviceCategory = await _dbContext.ProductCategories
                    .Where(x => x.CompanyId == request.CompanyId && x.IsServiceCategory)
                    .Select(x => x.Id).SingleOrDefaultAsync();
                
                var service = _mapper.Map<Inventory>(request);
                service.CategoryId = serviceCategory;
                _dbContext.Inventories.Add(service);

                _dbContext.InventoryPriceHistories.Add(new InventoryPriceHistory
                {
                    CompanyId = request.CompanyId,
                    CreatedBy = request.UserId,
                    InventoryId = service.Id,
                    UnitCostPrice = 0.00m,
                    UnitSalesPrice = request.UnitSalesPrice,
                    RestockDate = DateTime.Today,
                    CreatedOn = DateTime.Today
                });
                
                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.Inventory,
                        Action = (int)AuditLogInventoryAction.Create,
                        Description = $"Added new service inventory {request.Name}",
                        UserId = request.UserId
                    });

                // var receivedItems = new List<ReceivedGoodsModel>
                // {
                //     new ReceivedGoodsModel
                //     {
                //         AccountingPeriodId = accountingPeriod.Id,
                //         Amount = 0.00m,
                //         TaxAmount = 0.00m,
                //         TaxId = null,
                //         VendorId = null,
                //         InventoryId = service.Id,
                //          Inventory = service.Name,
                //         DateReceived = date,
                //         ReceivedBy = request.UserId
                //     }
                // };

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
                
                return new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
