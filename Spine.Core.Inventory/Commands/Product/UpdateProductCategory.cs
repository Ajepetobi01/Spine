using System;
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
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Product
{
    public static class UpdateProductCategory
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public Guid Id { get; set; }

            [Required]
            public string Name { get; set; }
            
            [Required]
            public Guid? InventoryAccountId { get; set; }
            [Required]
            public Guid? SalesAccountId { get; set; }
            [Required]
            public Guid? CostOfSalesAccountId { get; set; }
            
            public bool ApplyTaxOnPO { get; set; }

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

            public Handler(SpineContext context, IMapper mapper, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _mapper = mapper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var categories = await _dbContext.ProductCategories.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted &&
                                                    (x.Id == request.Id || x.Name.ToLower() == request.Name.ToLower())).ToListAsync();

                if (categories.Count == 0) return new Response("Category not found");
                if (categories.Any(x => x.Id != request.Id))
                {
                    return new Response("Another category with this name exists");
                }

                var category = categories.First();

                category.Name = request.Name;
                category.InventoryAccountId = request.InventoryAccountId;
                category.SalesAccountId = request.SalesAccountId;
                category.CostOfSalesAccountId = request.CostOfSalesAccountId;
                category.ApplyTaxOnPO = request.ApplyTaxOnPO;

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.Inventory,
                        Action = (int)AuditLogInventoryAction.UpdateInventoryCategory,
                        Description = $"Updated name for product category  with id {category.Id} to {request.Name}",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response(HttpStatusCode.OK)
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
