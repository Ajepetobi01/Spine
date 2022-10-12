using System;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Data.Entities;

namespace Spine.Services
{
    public class AuditModel
    {
        public int EntityType { get; set; }
        public int Action { get; set; }
        public string Description { get; set; }
        public Guid UserId { get; set; }
    }

    public interface IAuditLogHelper
    {
        void SaveAction(SpineContext context, Guid companyId, AuditModel model);
    }

    public class AuditLogHelper : IAuditLogHelper
    {
        public void SaveAction(SpineContext context, Guid companyId, AuditModel model)
        {
            context.AuditLogs.Add(new AuditLog
            {
                CompanyId = companyId,
                EntityType = model.EntityType,
                Action = model.Action,
                UserId = model.UserId,
                Description = model.Description,
                CreatedOn = Constants.GetCurrentDateTime(),
                Id = Guid.NewGuid()
            });

        }
    }

}
