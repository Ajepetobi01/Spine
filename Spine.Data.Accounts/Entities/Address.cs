using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Accounts.Entities
{
    [Index(nameof(CompanyId))]
    public class Address : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        [MaxLength(256)]
        public string AddressLine1 { get; set; }
        [MaxLength(256)]
        public string AddressLine2 { get; set; }
        [MaxLength(256)]
        public string City { get; set; }
        [MaxLength(256)]
        public string State { get; set; }
        [MaxLength(256)]
        public string Country { get; set; }
        [MaxLength(20)]
        public string PostalCode { get; set; }

    }
}
