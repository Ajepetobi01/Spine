using System;
using System.ComponentModel.DataAnnotations;
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

namespace Spine.Core.Inventories.Commands
{
    public static class UpdateInventoryNote
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
            //   [Range(1, 500)]
            public string Note { get; set; }

        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.NoContent;
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
                var note = await _dbContext.InventoryNotes.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted);

                if (note == null) return new Response("Note not found");

                note.Note = request.Note;

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
               new AuditModel
               {
                   EntityType = (int)AuditLogEntityType.Inventory,
                   Action = (int)AuditLogInventoryAction.UpdateNote,
                   Description = $"Updated inventory note",
                   UserId = request.UserId
               });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
