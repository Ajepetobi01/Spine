using System;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Core.Inventories.Helper;
using Spine.Data;
using Spine.PdfGenerator;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Order
{
    public static class DownloadPurchaseOrder
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
            public byte[] PdfByte { get; set; }
            public string OrderDate { get; set; }
            public Response(byte[] pdfByte, string orderDate)
            {
                PdfByte = pdfByte;
                OrderDate = orderDate;
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
            private readonly IInventoryHelper _invHelper;
            private readonly IPdfGenerator _pdfGenerator;

            public Handler(SpineContext context, IAuditLogHelper auditHelper, IInventoryHelper invHelper, IPdfGenerator pdfGenerator)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _invHelper = invHelper;
                _pdfGenerator = pdfGenerator;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var order = await _dbContext.PurchaseOrders.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted);
                if (order == null) return new Response("Purchase order not found");

                var (pdfByte, orderDate) = await _invHelper.GeneratePurchaseOrderPdf(_pdfGenerator, _dbContext, order.CompanyId, order.Id);

                return new Response(pdfByte, orderDate);
            }
        }

    }
}
