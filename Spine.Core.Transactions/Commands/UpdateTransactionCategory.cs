using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Transactions.Commands
{
    public static class UpdateTransactionCategory
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public Guid Id { get; set; }

            public Guid? ParentCategoryId { get; set; }

            [Required]
            public string Name { get; set; }
            public bool IsInflow { get; set; }
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

            public Handler(SpineContext context, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var categories = await _dbContext.TransactionCategories.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted &&
                                                (x.Id == request.Id || x.Name.ToLower() == request.Name.ToLower())).ToListAsync();

                if (categories.Count == 0) return new Response("Category not found");
                if (categories.Any(x => x.Id != request.Id))
                {
                    return new Response("Category already exist");
                }

                var category = categories.First();
                category.ParentCategoryId = request.ParentCategoryId;
                category.Name = request.Name;
                category.IsInflow = request.IsInflow;
              
                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Transactions,
                    Action = (int)AuditLogTransactionAction.UpdateCategory,
                    UserId = request.UserId,
                    Description = $"Updated transaction category {request.Id}"
                });

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    return new Response();
                }

                return new Response("Category could not be saved");
            }
        }

    }
}
