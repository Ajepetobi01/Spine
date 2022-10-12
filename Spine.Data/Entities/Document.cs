using System;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities
{
    [Index(nameof(CompanyId), nameof(ParentItemId))]
    public class Document : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        /// <summary>
        ///  can be invoiceId, transactionId, inventoryId
        /// </summary>
        public Guid ParentItemId { get; set; }
        public Guid DocumentId { get; set; }

    }
}
