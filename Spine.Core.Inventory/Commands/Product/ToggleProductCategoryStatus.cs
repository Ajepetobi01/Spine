using System;
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

namespace Spine.Core.Inventories.Commands.Product
{
    public static class ToggleProductCategoryStatus
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
                var category = await _dbContext.ProductCategories.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.Id).SingleOrDefaultAsync();

                if (category == null) return new Response("Category not found");
                if (category.IsServiceCategory) return new Response("This category cannot be marked inactive");
                
                if (category.Status == Status.Active) category.Status = Status.Inactive;
                else if (category.Status == Status.Inactive) category.Status = Status.Active;

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response(HttpStatusCode.OK)
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
