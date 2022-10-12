using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public static class UpdateTaxType
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [RequiredNotEmpty]
            public List<TaxModel> Tax { get; set; }
        }

        public class TaxModel
        {
            [Required]
            public Guid? Id { get; set; }

            //[Required]
            //public string Tax { get; set; }

            //[Range(0, 100)]
            //public double TaxRate { get; set; }

            //public bool IsCompound { get; set; }

            public bool IsActive { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.NoContent;
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
                var taxIds = request.Tax.Select(x => x.Id).ToHashSet();

                var taxes = await _dbContext.TaxTypes.Where(x => x.CompanyId == request.CompanyId && taxIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id);

                if (taxes.IsNullOrEmpty()) return new Response("Tax type not found");
                foreach (var item in request.Tax)
                {
                    if (taxes.TryGetValue(item.Id.Value, out var tax))
                    {
                        tax.IsActive = item.IsActive;

                        //tax.Tax = item.Tax;
                        //tax.TaxRate = item.TaxRate;
                        //tax.IsCompound = item.IsCompound;
                        tax.LastModifiedBy = request.UserId;
                    }
                }

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.TaxType,
                    Action = (int)AuditLogTaxTypeAction.Update,
                    UserId = request.UserId,
                    Description = $"Updated tax types"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Tax types could not be updated");
            }
        }

    }
}
