using System;
namespace Spine.Common.Data.Interfaces
{
    public interface IAuditable
    {
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
    }
}
