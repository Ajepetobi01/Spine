using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Data.Entities.Transactions;
using Spine.Services;

namespace Spine.Core.Transactions.Commands
{
    public static class CreateTransactionCategory
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }


            public Guid? ParentCategoryId { get; set; }

            [Required]
            public string Name { get; set; }
            public bool IsInflow { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Guid Id { get; set; }
            public Response(Guid id)
            {
                Id = id;
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

            public Handler(SpineContext context, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                if (await _dbContext.TransactionCategories.AnyAsync(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.Name.ToLower() == request.Name.ToLower()))
                    return new Response("Category already exist");

                var newItem = new TransactionCategory
                {
                    CompanyId = request.CompanyId,
                    IsInflow = request.IsInflow,
                    ParentCategoryId = request.ParentCategoryId,
                    Name = request.Name.ToTitleCase()
                };

                _dbContext.TransactionCategories.Add(newItem);

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Transactions,
                    Action = (int)AuditLogTransactionAction.CreateCategory,
                    UserId = request.UserId,
                    Description = $"Created new transaction category {request.Name}"
                });

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    return new Response(newItem.Id);
                }

                return new Response("Category could not be saved");
            }
        }

    }
}
