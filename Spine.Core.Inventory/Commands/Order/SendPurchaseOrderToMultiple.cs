using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Core.Inventories.Helper;
using Spine.Core.Inventories.Jobs;
using Spine.Data;
using Spine.Services;
using Spine.Services.Attributes;

namespace Spine.Core.Inventories.Commands.Order
{
    public static class SendPurchaseOrderToMultiple
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
            public string Subject { get; set; }
            [Required]
            public string Body { get; set; }

            [Required]
            public string To { get; set; }
            public string CC { get; set; }
            public string BCC { get; set; }

            [MaxFileSize(2 * 1024 * 1024)] //2MB
            [AllowedExtensions(new string[] { ".pdf", ".doc", ".docx", ".png", ".jpeg", ".jpg", })]
            public IFormFileCollection Attachments { get; set; }

            [JsonIgnore]
            public List<AttachmentModel> ConvertedAttachments { get; set; }
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
            private readonly CommandsScheduler _scheduler;

            public Handler(SpineContext context, IAuditLogHelper auditHelper, CommandsScheduler scheduler)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _scheduler = scheduler;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var order = await _dbContext.PurchaseOrders.SingleOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted);
                if (order == null) return new Response("Purchase order not found");

                if (order.Status < PurchaseOrderStatus.Issued)
                    order.Status = PurchaseOrderStatus.Issued;
                
                _scheduler.SendNow(new SendOrderToMultipleJobCommand
                    {
                        CompanyId = request.CompanyId,
                        OrderId = request.Id,
                        Subject = request.Subject,
                        Body = request.Body,
                        To = request.To.Split(',').ToList(),
                        CC = request.CC?.Split(',').ToList(),
                        BCC = request.BCC?.Split(',').ToList(),
                        Attachments = request.ConvertedAttachments
                    }
                    , $"Send Invoice {order.Id} to multiple contact");

                await _dbContext.SaveChangesAsync();
                 return new Response();
            }
        }
    }
}
