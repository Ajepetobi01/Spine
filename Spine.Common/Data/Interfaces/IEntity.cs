using System;
using System.ComponentModel.DataAnnotations;

namespace Spine.Common.Data.Interfaces
{
    public interface IEntity
    {
        [Key]
        Guid Id { get; set; }
    }
}
