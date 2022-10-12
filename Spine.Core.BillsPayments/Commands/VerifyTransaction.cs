using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Spine.Common.ActionResults;
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Services.HttpClients;
using Spine.Services.Interswitch;

namespace Spine.Core.BillsPayments.Commands
{
    public static class VerifyTransaction
    {
        public class Request : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required]
            public string RequestReference { get; set; }

        }

        public class Response : BasicActionResult
        {
            public QueryTransaction.Model Data { get; set; }

            public Response(string message) : base(message)
            {
                ErrorMessage = message;
            }

            public Response(QueryTransaction.Model data)
            {
                Status = HttpStatusCode.OK;
                Data = data;
            }
        }

        public class Handler : IRequestHandler<Request, Response>
        {
            private readonly SpineContext _dbContext;
            private readonly InterswitchClient _client;
            private readonly string TerminalId;

            public Handler(SpineContext context, InterswitchClient client, IConfiguration configuration)
            {
                _dbContext = context;
                _client = client;

                TerminalId = configuration["Interswitch:TerminalId"];
            }

            public async Task<Response> Handle(Request request, CancellationToken token)
            {
                var handler = new QueryTransaction.Handler();
                var response = await handler.Handle(new QueryTransaction.Request
                {
                    TerminalId = TerminalId,
                    RequestReference = request.RequestReference
                }, _client);

                if (response.Message.IsNullOrEmpty())
                {
                    var req = await _dbContext.BillPayments.Where(x => x.RequestReference == request.RequestReference
                                                        && response.Data.TransactionRef == x.TransactionReference)
                                                        .SingleOrDefaultAsync();
                    if (req != null)
                    {
                        req.TransactionStatus = response.Data.Status;
                        await _dbContext.SaveChangesAsync();
                    }

                    return new Response(response.Data);
                }

                else
                    return new Response(response.Message);

            }
        }

    }
}
