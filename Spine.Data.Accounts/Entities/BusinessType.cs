using System;
using System.ComponentModel.DataAnnotations;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Accounts.Entities
{
    public class BusinessType
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(256)]
        public string Type { get; set; }

    }
}
