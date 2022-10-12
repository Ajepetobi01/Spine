using System;
namespace Spine.Common.Data.Interfaces
{
    public interface IDeletable
    {
        bool IsDeleted { get; set; }
        Guid? DeletedBy { get; set; }
    }
}
