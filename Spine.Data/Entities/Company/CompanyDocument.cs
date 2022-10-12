using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities
{
    [Index(nameof(CompanyId))]
    public class CompanyDocument : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        [MaxLength(256)]
        public string DocumentId { get; set; }
        [MaxLength(500)]
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}
