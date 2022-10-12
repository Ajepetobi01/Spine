using System;
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
    public static class DeleteTransactionCategory
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public Guid Id { get; set; }

        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.OK;
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
                var category = await _dbContext.TransactionCategories.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted);

                if (category == null) return new Response("Category not found");

                category.IsDeleted = true;
                category.DeletedBy = request.UserId;

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Transactions,
                    Action = (int)AuditLogTransactionAction.DeleteCategory,
                    UserId = request.UserId,
                    Description = $"Deleted transactioncategory {category.Name}"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Category could not be deleted");
            }
        }

    }
}
