using System;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities
{
    [Index(nameof(CompanyId))]
    public class AuditLog : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public int EntityType { get; set; }
        public int Action { get; set; }
        public string Description { get; set; }
        public string MACAddress { get; set; }
        public string Device { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
