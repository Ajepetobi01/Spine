using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Spine.Common.ActionResults;
using Spine.Common.Extensions;
using Spine.Services.HttpClients;
using Spine.Services.Interswitch;

namespace Spine.Core.BillsPayments.Queries
{
    public static class GetCategories
    {
        public class Query : IRequest<Response>
        {
        }

        public class Response : BasicActionResult
        {
            public List<GetBillerCategories.Model> Data { get; set; }

            public Response(string message) : base(message)
            {
                ErrorMessage = message;
            }

            public Response(List<GetBillerCategories.Model> data)
            {
                Status = HttpStatusCode.OK;
                Data = data;
            }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly InterswitchClient _client;
            private readonly string TerminalId;

            public Handler(InterswitchClient client, IConfiguration config)
            {
                _client = client;
                TerminalId = config["Interswitch:TerminalId"];
            }

            public async Task<Response> Handle(Query request, CancellationToken token)
            {
                var handler = new GetBillerCategories.Handler();
                var response = await handler.Handle(new GetBillerCategories.Request { TerminalId = TerminalId }, _client);
                if (response.Message.IsNullOrEmpty())
                {
                    return new Response(response.Data);
                }

                else
                    return new Response(response.Message);

            }
        }

    }
}
